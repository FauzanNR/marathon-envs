using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Kinematic;

namespace Kinematic
{

    /// <summary>
    /// Provides access to COM properties of ArticulationBodies or Rigidbodies arranged in a kinematic chain hierarchy
    /// </summary>
    public class BodyChain
    {
        private IReadOnlyList<IKinematic> chain;

        public float Mass {get => chain.Select(k => k.Mass).Sum();}
        public Vector3 CenterOfMass {get => chain.Select(k => k.Mass * k.CenterOfMass).Sum() / Mass;}
        public Vector3 CenterOfMassVelocity {get => chain.Select(k => k.Mass * k.Velocity).Sum() / Mass;}
        public IEnumerable<Vector3> CentersOfMass {get => chain.Select(k => k.CenterOfMass);}
        public IEnumerable<Vector3> Velocities {get => chain.Select(k => k.Velocity);}
        public IEnumerable<Matrix4x4> TransformMatrices { get => chain.Select(k => k.TransformMatrix); }
        public Vector3 RootForward { get => chain[0].TransformMatrix.GetColumn(2); }

        public BodyChain() { }

        public BodyChain(Transform chainRoot)
        {
            chain = GetKinematicChain(chainRoot);
        }

        public BodyChain(IEnumerable<Transform> bodies)
        {
            chain = bodies.Select(b => b.GetComponent<ArticulationBody>() ? (IKinematic)
                                        new ArticulationBodyAdapter(b.GetComponent<ArticulationBody>()) : 
                                        new RigidbodyAdapter(b.GetComponent<Rigidbody>())).ToList().AsReadOnly();
        }

        //Recursively find a read-only list of IKinematic, independent if its Rigidbodies or ArticulationBodies
        IReadOnlyList<IKinematic> GetKinematicChain(Transform root)
        {
            if(root.GetComponentInChildren<ArticulationBody>())
            {
                return root.GetComponentsInChildren<ArticulationBody>().Select(ab =>new ArticulationBodyAdapter(ab)).ToList().AsReadOnly();
            }
            else
            {
                return root.GetComponentsInChildren<Rigidbody>().Select(rb => new RigidbodyAdapter(rb)).ToList().AsReadOnly();
            }
        }

    
    
    }

    #region Adapters for Rigidbody and ArticulationBody
    // As we support both Rigidbodies and ArticulationBodies, the adapter pattern is used to unify their operation inside BodyChain

    public interface IKinematic
    {
        public Vector3 Velocity { get; }

        public Vector3 AngularVelocity { get; }
        public float Mass { get; }
        public Vector3 CenterOfMass { get; }

        public Matrix4x4 TransformMatrix { get; }
        public string Name { get; }
    }

    public class RigidbodyAdapter : IKinematic
    {
        readonly private Rigidbody rigidbody;

        public RigidbodyAdapter(Rigidbody rigidbody)
        {
            this.rigidbody = rigidbody;
        }

        public Vector3 Velocity => rigidbody.velocity;

        public Vector3 AngularVelocity => rigidbody.angularVelocity;

        public float Mass => rigidbody.mass;

        public Vector3 CenterOfMass => rigidbody.transform.TransformPoint(rigidbody.centerOfMass);

        public string Name => rigidbody.name;

        public Matrix4x4 TransformMatrix => rigidbody.transform.localToWorldMatrix;
    }

    public class ArticulationBodyAdapter : IKinematic
    {
        readonly private ArticulationBody articulationBody;

        public ArticulationBodyAdapter(ArticulationBody articulationBody)
        {
            this.articulationBody = articulationBody;
        }

        public Vector3 Velocity => articulationBody.velocity;

        public Vector3 AngularVelocity => articulationBody.angularVelocity;

        public float Mass => articulationBody.mass;

        public Vector3 CenterOfMass => articulationBody.transform.TransformPoint(articulationBody.centerOfMass);

        public string Name => articulationBody.name;

        public Matrix4x4 TransformMatrix => articulationBody.transform.localToWorldMatrix;

    }
    #endregion
}

