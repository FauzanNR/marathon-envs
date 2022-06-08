using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//using Kinematic;
using Mujoco;

using Unity.Mathematics;

namespace Kinematic
{

    /// <summary>
    /// Provides access to COM properties of ArticulationBodies or Rigidbodies arranged in a kinematic chain hierarchy
    /// </summary>
    public class BodyChain
    {
        protected IReadOnlyList<IKinematic> chain;

        protected float mass; //chain.Select(k => k.Mass).Sum();
        public float Mass {get => mass;}
        public Vector3 CenterOfMass { get => chain.Select(k => (Vector3)(k.Mass * k.CenterOfMass)).Sum() / Mass;
                    
                }
        public Vector3 CenterOfMassVelocity {get => chain.Select(k => k.Mass * k.Velocity).Sum() / Mass;}
        public IEnumerable<Vector3> CentersOfMass {get => chain.Select(k => (Vector3) k.CenterOfMass);}
        public IEnumerable<Vector3> Velocities {get => chain.Select(k => k.Velocity);}
        //public IEnumerable<Matrix4x4> TransformMatrices { get => chain.Select(k => k.TransformMatrix); }
        public Vector3 RootForward { get => chain[0].Forward;  }



        public BodyChain() { }

        public BodyChain(Transform chainRoot)
        {
            chain = GetKinematicChain(chainRoot);

            mass = chain.Select(k => k.Mass).Sum();
        }

        public BodyChain(IEnumerable<Transform> bodies)
        {
            chain = bodies.Select(b => b.GetKinematic()).ToList().AsReadOnly();

            mass = chain.Select(k => k.Mass).Sum();
        }

        //Recursively find a read-only list of IKinematic, independent if its Rigidbodies or ArticulationBodies
        protected IReadOnlyList<IKinematic> GetKinematicChain(Transform root)
        {
            if(root.GetComponentInChildren<ArticulationBody>())
            {
                return root.GetComponentsInChildren<ArticulationBody>().Select(ab =>new ArticulationBodyAdapter(ab)).ToList().AsReadOnly();
            }
            else if(root.GetComponentInChildren<Rigidbody>())
            {
                return root.GetComponentsInChildren<Rigidbody>().Select(rb => new RigidbodyAdapter(rb)).ToList().AsReadOnly();
            }
            else
            {
                return root.GetComponentsInChildren<MjBody>().Select(rb => new MjBodyAdapter(rb)).ToList().AsReadOnly();
            }

        }

        public static IReadOnlyList<IKinematic> GetChainFromColliders(IEnumerable<Collider> colliders)
        {

            return colliders.Select(col => col.attachedRigidbody != null ?
                                                        (IKinematic)new RigidbodyAdapter(col.attachedRigidbody) :
                                                        (col.attachedArticulationBody != null ?
                                                            new ArticulationBodyAdapter(col.attachedArticulationBody) : new MjBodyAdapter(col.transform.parent.GetComponent<MjBody>()))).ToList().AsReadOnly();



        }

    }





    #region Adapters for Rigidbody, MjBody and ArticulationBody
    // We support both Rigidbodies and ArticulationBodies, the adapter pattern is used to unify their operation inside BodyChain


    public interface IReducedState
    {

        //Here everything is in reduced Coordinates

        public float3 JointAcceleration { get; }
        public float3 JointVelocity { get; }
        public float3 JointPosition { get; }

    }




    public interface IKinematic : IReducedState
    {
        public Vector3 Velocity { get; }

        public Vector3 AngularVelocity { get; }
        public float Mass { get; }
        public float3 CenterOfMass { get; }

        public Matrix4x4 TransformMatrix { get; }

        public Vector3 GetPointVelocity(float3 worldPoint);

        public Vector3 GetRelativePointVelocity(Vector3 localPoint);
        public string Name { get; }

        public GameObject gameObject { get; }

        public Vector3 Forward { get; }


        public float3x3 JointAxes { get; }

        public int index { get; } //position in the hierarchy
        public bool isRoot { get; } //is it a root

        public float3 InertiaTensor { get; }

        public float3 AnchorPosition { get; }
        

    }

    public static class KinematicExtensions
    {
        public static IKinematic GetKinematic(this Transform transform)
        {
            return transform.GetComponent<ArticulationBody>() ? (IKinematic)
                                        new ArticulationBodyAdapter(transform.GetComponent<ArticulationBody>()) :
                                            (transform.GetComponent<Rigidbody>() ?
                                            new RigidbodyAdapter(transform.GetComponent<Rigidbody>()) :
                                                new MjBodyAdapter(transform.GetComponent<MjBody>()));
        }
    }


    public class RigidbodyAdapter : IKinematic
    {
        readonly private Rigidbody _rb;

        public RigidbodyAdapter(Rigidbody rigidbody)
        {
            this._rb = rigidbody;
        }

        public Vector3 Velocity => _rb.velocity;

        public Vector3 LocalVelocity => _rb.transform.parent.InverseTransformDirection(Velocity);

        public Vector3 AngularVelocity => _rb.angularVelocity;
        public Vector3 LocalAngularVelocity => _rb.transform.parent.InverseTransformDirection(_rb.angularVelocity);

        public float Mass => _rb.mass;

        public float3 CenterOfMass => _rb.transform.TransformPoint(_rb.centerOfMass);

        public string Name => _rb.name;

        public Matrix4x4 TransformMatrix => _rb.transform.localToWorldMatrix;

        
        public Vector3 GetPointVelocity(float3 worldPoint)
        {
            return _rb.GetPointVelocity(worldPoint);
        }
        

        public Vector3 GetRelativePointVelocity(Vector3 localPoint)
        {
            return _rb.GetRelativePointVelocity(localPoint);
        }

        public GameObject gameObject { get => _rb.gameObject; }


        public float3 JointPosition => Mathf.Deg2Rad * Utils.GetSwingTwist(_rb.transform.localRotation);
        public float3 JointVelocity
        {
            get
            {
                //notice this only makes sense because the DoF axes matrix is the identitiy, i.e. the relation between the localRotaiton and the degrees of Freedom is the identity matrix

                return LocalAngularVelocity;
            }
        }


        public float3 JointAcceleration
        {
            get
            {

                Debug.LogWarning("the acceleration of the rigidbody should not matter, why are you trying to read it? I am returing null");
                return Vector3.zero;
            }
        }

        public Vector3 Forward => _rb.transform.forward;
        public float3x3 JointAxes { get => float3x3.identity; }

        public int index { get =>  -1; }
        public bool isRoot { get => false; }
        public float3 InertiaTensor { get => _rb.inertiaTensor; }
        public float3 AnchorPosition { get {
                Debug.LogWarning("you are asking an anchor for a rigidBody, which does not have an anchor, I return zero ");
                return float3.zero;
            } 
        }

    }

    public class ArticulationBodyAdapter : IKinematic
    {
        readonly private ArticulationBody _ab;

        readonly private float3x3 jointAxes;

        public ArticulationBodyAdapter(ArticulationBody articulationBody)
        {
            this._ab = articulationBody;

            jointAxes = float3x3.identity;
            if (_ab.twistLock == ArticulationDofLock.LockedMotion)
                jointAxes.c0 = float3.zero;
            if (_ab.swingYLock == ArticulationDofLock.LockedMotion)
                jointAxes.c1 = float3.zero;
            if (_ab.swingZLock == ArticulationDofLock.LockedMotion)
                jointAxes.c2 = float3.zero;
        }

        public Vector3 Velocity => _ab.velocity;
        public Vector3 LocalVelocity => _ab.transform.parent.InverseTransformDirection(_ab.velocity);

        public Vector3 AngularVelocity => _ab.angularVelocity;
        public Vector3 LocalAngularVelocity => _ab.transform.parent.InverseTransformDirection(_ab.angularVelocity);

        public float Mass => _ab.mass;

        public float3 CenterOfMass => _ab.transform.TransformPoint(_ab.centerOfMass);

        public string Name => _ab.name;

        public Matrix4x4 TransformMatrix => _ab.transform.localToWorldMatrix;

        public Vector3 GetPointVelocity(float3 worldPoint)
        {
            return _ab.GetPointVelocity(worldPoint);
        }

        public Vector3 GetRelativePointVelocity(Vector3 localPoint)
        {
            return _ab.GetRelativePointVelocity(localPoint);
        }

        public GameObject gameObject { get => _ab.gameObject; }

        public float3 JointPosition => Utils.GetArticulationReducedSpaceInVector3(_ab.jointPosition);
        public float3 JointVelocity => Utils.GetArticulationReducedSpaceInVector3(_ab.jointVelocity);

        public float3 JointAcceleration => Utils.GetArticulationReducedSpaceInVector3(_ab.jointAcceleration);

        public Vector3 Forward => _ab.transform.forward;

        public float3x3 JointAxes { get => jointAxes; }

        public int index { get => _ab.index; }
        public bool isRoot { get => _ab.isRoot; }

        public float3 InertiaTensor { get => _ab.inertiaTensor; }

        public float3 AnchorPosition { get => _ab.anchorPosition; }

    }

    public class MjBodyAdapter : IKinematic
    {
        readonly private MjBody _mb;

        readonly private MjScene scene;

        readonly private float mass;

       // readonly private Transform inertialTransform;

        readonly private Vector3 inertiaLocalPos;

        readonly private Matrix4x4 inertiaRelMatrix;

        readonly private IReadOnlyList<MjHingeJoint> joints;

        readonly private float3x3 jointAxes;

        public float3x3 JointAxes { get => jointAxes; }


        public MjBodyAdapter(MjBody mjBody)
        {
            this._mb = mjBody;
            scene = MjScene.Instance; 
            mass = mjBody.GetMass();
            inertiaLocalPos = mjBody.GetLocalCenterOfMass();
            inertiaRelMatrix = mjBody.GetLocalCenterOfMassMatrix();
            joints = _mb.GetComponentsInDirectChildren<MjHingeJoint>();
            jointAxes = new float3x3();
            for (int i=0; i<joints.Count; i++)
            {
                Vector3 localJointAxis = joints[i].transform.localRotation * Vector3.right;
                jointAxes[0][i] = localJointAxis.x;
                jointAxes[1][i] = localJointAxis.y;
                jointAxes[2][i] = localJointAxis.z;
            }

         //   inertialTransform = mjBody.transform.GetComponentInDirectChildren<BoxCollider>().transform; // Could be queried for Matrix and pos, but is unfortunately not up to date with the Mj simulation
        }

        public Vector3 Velocity => _mb.GlobalVelocity();

        public int DoFCount => joints.Count; 

        public Vector3 AngularVelocity => _mb.GlobalAngularVelocity();

        public float Mass => mass;

        public float3 CenterOfMass =>_mb.GetTransformMatrix().MultiplyPoint3x4(inertiaLocalPos);

        public string Name => _mb.name;

        public Matrix4x4 TransformMatrix => _mb.GetTransformMatrix() * inertiaRelMatrix;

        

        public Vector3 GetPointVelocity(float3 worldPoint)
        {
            
            return Vector3.Cross((worldPoint - CenterOfMass), AngularVelocity) + Velocity;
        }

        public Vector3 GetRelativePointVelocity(Vector3 localPoint)
        {
            return Vector3.Cross(localPoint, AngularVelocity) + Velocity;
        }

        public GameObject gameObject { get => _mb.gameObject; }


        public float3 JointPosition
        {
            get
            {
                float3 pos = new float3() ;
                for (int i=0; i< joints.Count; i++)
                {
                    pos[i] = joints[i].GetPositionRad();
                }

                return pos;
            }

        }
        public float3 JointVelocity
        {
            get
            {

                float3 vel = new float3();
                for (int i = 0; i < joints.Count; i++)
                {
                    vel[i] = joints[i].GetVelocityRad();
                }

                return vel;
            }

        }


        public float3 JointAcceleration
        {
            get
            {

                float3 acc = new float3();
                for (int i = 0; i < joints.Count; i++)
                {
                    acc[i] = joints[i].GetAccelerationRad();
                }

                return acc;
            }
        }

        public Vector3 Forward => _mb.GetTransformMatrix().GetColumn(2);


        public int index { get => _mb.MujocoId; }
        public bool isRoot { get => Utils.IsRoot(_mb); }


        public float3 InertiaTensor { get => _mb.GetInertia(); }


        public float3 AnchorPosition
        {
            get
            {
                return _mb.GetComponentInDirectChildren<MjBaseJoint>().transform.localPosition;
            }
        }

    }
    #endregion
}

