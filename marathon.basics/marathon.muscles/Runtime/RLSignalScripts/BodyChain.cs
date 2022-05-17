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
        public Vector3 CenterOfMass {get => chain.Select(k => k.Mass * k.CenterOfMass).Sum() / Mass;}
        public Vector3 CenterOfMassVelocity {get => chain.Select(k => k.Mass * k.Velocity).Sum() / Mass;}
        public IEnumerable<Vector3> CentersOfMass {get => chain.Select(k => k.CenterOfMass);}
        public IEnumerable<Vector3> Velocities {get => chain.Select(k => k.Velocity);}
        //public IEnumerable<Matrix4x4> TransformMatrices { get => chain.Select(k => k.TransformMatrix); }
        public Vector3 RootForward { get => chain[0].TransformMatrix.GetColumn(2);  }

        public BodyChain() { }

        public BodyChain(Transform chainRoot)
        {
            chain = GetKinematicChain(chainRoot);

            mass = chain.Select(k => k.Mass).Sum();
        }

        public BodyChain(IEnumerable<Transform> bodies)
        {
            chain = bodies.Select(b => b.GetComponent<ArticulationBody>() ? (IKinematic)
                                        new ArticulationBodyAdapter(b.GetComponent<ArticulationBody>()) :
                                            (b.GetComponent<Rigidbody>()?
                                            new RigidbodyAdapter(b.GetComponent<Rigidbody>()) :
                                                new MjBodyAdapter(b.GetComponent<MjBody>()))).ToList().AsReadOnly();

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
        public Vector3 CenterOfMass { get; }

        public Matrix4x4 TransformMatrix { get; }

        public Vector3 GetPointVelocity(Vector3 worldPoint);

        public Vector3 GetRelativePointVelocity(Vector3 localPoint);
        public string Name { get; }

        public GameObject gameObject { get; }

        public static IKinematic GetKinematic(Transform transform)
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

        public Vector3 CenterOfMass => _rb.transform.TransformPoint(_rb.centerOfMass);

        public string Name => _rb.name;

        public Matrix4x4 TransformMatrix => _rb.transform.localToWorldMatrix;

        
        public Vector3 GetPointVelocity(Vector3 worldPoint)
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

                Debug.LogWarning("the velocity of the rigidbody should not matter, why are you trying to read it, I am not sure what I am returning is in reduced coord.");
                return _rb.angularVelocity;
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

    }

    public class ArticulationBodyAdapter : IKinematic
    {
        readonly private ArticulationBody _ab;

        public ArticulationBodyAdapter(ArticulationBody articulationBody)
        {
            this._ab = articulationBody;
        }

        public Vector3 Velocity => _ab.velocity;
        public Vector3 LocalVelocity => _ab.transform.parent.InverseTransformDirection(_ab.velocity);

        public Vector3 AngularVelocity => _ab.angularVelocity;
        public Vector3 LocalAngularVelocity => _ab.transform.parent.InverseTransformDirection(_ab.angularVelocity);

        public float Mass => _ab.mass;

        public Vector3 CenterOfMass => _ab.transform.TransformPoint(_ab.centerOfMass);

        public string Name => _ab.name;

        public Matrix4x4 TransformMatrix => _ab.transform.localToWorldMatrix;

        public Vector3 GetPointVelocity(Vector3 worldPoint)
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


    }

    public class MjBodyAdapter : IKinematic
    {
        readonly private MjBody mjBody;

        readonly private MjScene scene;

        readonly private float mass;

       // readonly private Transform inertialTransform;

        readonly private Vector3 inertiaLocalPos;

        readonly private Matrix4x4 inertiaRelMatrix;

        public MjBodyAdapter(MjBody mjBody)
        {
            this.mjBody = mjBody;
            scene = MjScene.Instance; 
            mass = mjBody.GetMass();
            inertiaLocalPos = mjBody.GetLocalCenterOfMass();
            inertiaRelMatrix = mjBody.GetLocalCenterOfMassMatrix();

         //   inertialTransform = mjBody.transform.GetComponentInDirectChildren<BoxCollider>().transform; // Could be queried for Matrix and pos, but is unfortunately not up to date with the Mj simulation
        }

        public Vector3 Velocity => mjBody.GlobalVelocity();

        public Vector3 AngularVelocity => mjBody.GlobalAngularVelocity();

        public float Mass => mass;

        public Vector3 CenterOfMass =>mjBody.GetTransformMatrix().MultiplyPoint3x4(inertiaLocalPos);

        public string Name => mjBody.name;

        public Matrix4x4 TransformMatrix => mjBody.GetTransformMatrix() * inertiaRelMatrix;

        

        public Vector3 GetPointVelocity(Vector3 worldPoint)
        {
            
            return Vector3.Cross((worldPoint - CenterOfMass), AngularVelocity) + Velocity;
        }

        public Vector3 GetRelativePointVelocity(Vector3 localPoint)
        {
            return Vector3.Cross(localPoint, AngularVelocity) + Velocity;
        }

        public GameObject gameObject { get => mjBody.gameObject; }


        public float3 JointPosition
        {
            get
            {

                Debug.LogError("the reduced position of the mjBody is NOT implemented");
                return Vector3.zero;
            }

        }
        public float3 JointVelocity
        {
            get
            {

                Debug.LogError("the reduced acceleration of the mjBody is NOT implemented");
                return Vector3.zero;
            }

        }


        public float3 JointAcceleration
        {
            get
            {

                Debug.LogError("the reduced acceleration of the mjBody is NOT implemented");
                return Vector3.zero;
            }
        }



    }
    #endregion
}

