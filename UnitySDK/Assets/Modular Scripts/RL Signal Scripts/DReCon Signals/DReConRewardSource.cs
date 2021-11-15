using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Kinematic;
using DReCon;

public class DReConRewardSource : RewardSource
{
    [SerializeField]
    private Transform kinematicTransform;

    [SerializeField]
    private Transform simulationTransform;

    [SerializeField]
    private GameObject kinematicHead;

    [SerializeField]
    private GameObject simulationHead;

    private IKinematic kinHead;
    private IKinematic simHead;

    private BoundingBoxChain kinChain;
    private BoundingBoxChain simChain;

    int nBodies;

    public override float Reward { get => CalculateReward(); }

    public override void OnAgentInitialize()
    {
        kinChain = new BoundingBoxChain(kinematicTransform);
        simChain = new BoundingBoxChain(simulationTransform);

        kinHead = kinematicHead.GetComponent<ArticulationBody>() ? 
            (IKinematic) new ArticulationBodyAdapter(kinematicHead.GetComponent<ArticulationBody>()) : new RigidbodyAdapter(kinematicHead.GetComponent<Rigidbody>());
        
        simHead = simulationHead.GetComponent<ArticulationBody>() ? 
            (IKinematic) new ArticulationBodyAdapter(simulationHead.GetComponent<ArticulationBody>()) : new RigidbodyAdapter(simulationHead.GetComponent<Rigidbody>());

        nBodies = kinChain.Count;
    }

    private float CalculateReward()
    {

        ReferenceFrame fKin = new ReferenceFrame(kinChain.RootForward, kinChain.CenterOfMass);
        ReferenceFrame fSim = new ReferenceFrame(kinChain.RootForward, simChain.CenterOfMass); // Same orientation, different origin

        (float positionDiff, float velocityDiff, float localposeDiff) = BoundingBoxChain.CalulateDifferences(kinChain, simChain);
        float comVDiff = (fKin.WorldDirectionToCharacter(kinChain.CenterOfMassVelocity) - fSim.WorldDirectionToCharacter(simChain.CenterOfMassVelocity)).magnitude;

        float eFall = Mathf.Clamp01(1.3f - 1.4f * (simHead.CenterOfMass - kinHead.CenterOfMass).magnitude);

        return eFall * (Mathf.Exp(-10 / nBodies * positionDiff)
                        + Mathf.Exp(-1 / nBodies * velocityDiff)
                        + Mathf.Exp(-10 / nBodies * localposeDiff)
                        + Mathf.Exp(-comVDiff));
    }

    private void OnDrawGizmos()
    {
        return;
        kinChain.Draw();
        simChain.Draw();
    }


    /// <summary>
    /// Keeps track of Collider components, their local bounding boxes, and the global points of the bounding box face centers for comparison with other chains.
    /// </summary>
    private class BoundingBoxChain: BodyChain
    {
        private IReadOnlyList<Collider> colliders;
        private IReadOnlyList<Bounds> bounds;
        private IReadOnlyList<IKinematic> chain;

        private IEnumerable<BoundingPoints> Points { get => chain.Zip(bounds, 
            (bod, bound) => new BoundingPoints(bound, bod)); }

        private IEnumerable<Quaternion> Rotations { get => colliders.Select(col => col.transform.rotation); }

        public int Count { get => colliders.Count; }

        public BoundingBoxChain(Transform chainRoot)
        {
            SetupFromColliders(chainRoot.GetComponentsInChildren<Collider>().ToList().AsReadOnly());
        }

        private void SetupFromColliders(IEnumerable<Collider> colliders)
        {
            this.colliders = colliders.ToList().AsReadOnly();
            bounds = colliders.Select(col => GetColliderBounds(col)).ToList().AsReadOnly();

            chain = colliders.Select(col => col.attachedArticulationBody == null ? 
            (IKinematic)new RigidbodyAdapter(col.attachedRigidbody) : new ArticulationBodyAdapter(col.attachedArticulationBody)).ToList().AsReadOnly();

        }

        public static (float, float, float) CalulateDifferences(BoundingBoxChain chainA, BoundingBoxChain chainB)
        {
            var pointsA = chainA.Points;
            var pointsB = chainB.Points;
            float posDiff = pointsA.Zip(pointsB, (a, b) => BoundingPoints.PositionalDifference(a, b)).Sum();
            float velDiff = pointsA.Zip(pointsB, (a, b) => BoundingPoints.VelocityDifference(a, b)).Sum();
            var oof = Enumerable.Range(0, 10).Select(i => new Quaternion());
            float localPoseDiff = chainA.Rotations.Zip(chainB.Rotations, (ra, rb) => Quaternion.Angle(ra, rb)*Mathf.Deg2Rad).Sum();
           
            return (posDiff, velDiff, localPoseDiff);
        }

        #region Axis aligned bounding box methods
        private Bounds GetColliderBounds(Collider collider)
        {
            switch (collider)
            {
                case BoxCollider b:
                    return GetBounds(b);
                case SphereCollider s:
                    return GetBounds(s);
                case CapsuleCollider c:
                    return GetBounds(c);
                default:
                    throw new NotImplementedException("Collider type not supported");
            }
        }

        private Bounds GetBounds(BoxCollider box)
        {
            return new Bounds(box.center, box.size);
        }

        private Bounds GetBounds(SphereCollider sphere)
        {
            float d = sphere.radius * 2;
            return new Bounds(sphere.center, new Vector3(d, d, d));
        }

        private Bounds GetBounds(CapsuleCollider capsule)
        {
            float d = capsule.radius * 2;
            float h = capsule.height;
            Vector3 size = Vector3.one * d;
            size[capsule.direction] = h;
            return new Bounds(capsule.center, size);
        }
        #endregion

        public void Draw()
        {
            foreach (var bps in Points)
            {
                bps.Draw();
            }
        }

        // Produce the 6 face center points
        private struct BoundingPoints
        {
            private Vector3[] points;
            private Vector3 center;

            private IKinematic body;
            private Matrix4x4 LocalToWorldMatrix => body.TransformMatrix;
            private Vector3 WorldAngularVelocity => body.AngularVelocity;
            private Vector3 WorldLinearVelocity => body.Velocity;

            private IEnumerable<Vector3> WorldPoints // Note: can't access instance variables of struct in Linq anonymous functions
            { 
                get
                {
                    foreach(Vector3 p in points)
                    {
                        yield return LocalToWorldMatrix.MultiplyPoint3x4(p);
                    }
                }
            }

            private IEnumerable<Vector3> WorldVelocity
            {
                get
                {
                    foreach (Vector3 p in WorldPoints)
                    {
                        /*
                        //Potential way to do it by hand
                        Vector3 a = localToWorldMatrix.MultiplyPoint3x4(center); // World point on rotational axis (COM)
                        Vector3 n = worldAngularVelocity; // Vector along axis, magnitude scaled with velocity

                        yield return Vector3.Cross((p-a), n) + worldLinearVelocity;
                        */
                        yield return body.GetPointVelocity(p);
                    }
                }
            }


            public BoundingPoints(Bounds bounds, IKinematic body)
            {
                points = new Vector3[6];
                Vector3 c = bounds.center;

                //0: Right, 1: Left, 2: Top, 3: Bottom, 4: Front, 5: Back
                points[0] = new Vector3(bounds.max.x, c.y, c.z);
                points[1] = new Vector3(bounds.min.x, c.y, c.z);
                points[2] = new Vector3(c.x, bounds.max.y, c.z);
                points[3] = new Vector3(c.x, bounds.min.y, c.z);
                points[4] = new Vector3(c.x, c.y, bounds.max.z);
                points[5] = new Vector3(c.x, c.y, bounds.min.z);

                this.body = body;
                this.center = c;
            }

            public static float PositionalDifference(BoundingPoints pointsA, BoundingPoints pointsB)
            {
                return pointsA.WorldPoints.Zip(pointsB.WorldPoints, (a, b) => (a - b).magnitude).Sum();
            }

            public static float VelocityDifference(BoundingPoints pointsA, BoundingPoints pointsB)
            {
                return pointsA.WorldVelocity.Zip(pointsB.WorldVelocity, (a, b) => (a - b).magnitude).Sum();
            }

            public void Draw()
            {
                Gizmos.color = Color.white;
                foreach(var p in WorldPoints)
                {
                    Gizmos.DrawSphere(p, 0.01f);
                }
            }
        }

    }
}
