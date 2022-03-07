using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mujoco;
using System;

public class MjKinematicRig : MonoBehaviour, IKinematicReference
{
    [SerializeField]
    private Transform weldRoot;

    [SerializeField]
    private Transform trackedTransformRoot;

    [SerializeField]
    private Vector3 offset;


    // Private, since other scripts should reference rigidbodies from the hierarchy, and not depend on KinematicRig implementation if possible
    private IReadOnlyList<Transform> riggedTransforms;
    private IReadOnlyList<Transform> trackedTransforms;

    public List<Transform> RagdollTransforms => riggedTransforms.ToList();

    private List<MjMocapBody_> mjMocapBodies;

    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TrackKinematics();
    }

    public void Initialize()
    {
        Func<string, string> MocapName = animatedName => $"Mocap{Utils.SegmentName(animatedName)}";
        riggedTransforms = weldRoot.GetComponentsInChildren<MjWeld>().Select(krt => krt.Body1.transform).Where(t => t.name.Contains("Mocap") && t.gameObject.activeSelf).ToList().AsReadOnly();

        trackedTransforms = riggedTransforms.Select(rt => trackedTransformRoot.GetComponentsInChildren<Transform>().First(tt => MocapName(tt.name).Equals(rt.name))).ToList().AsReadOnly();

        mjMocapBodies = riggedTransforms.Select(t => t.GetComponent<MjMocapBody_>()).ToList();
        Debug.Log(string.Join(", ", mjMocapBodies.Select(mcbd => mcbd.name)));
    }

    public unsafe void TrackKinematics()
    {
        foreach ((var mjb, var tr) in riggedTransforms.Zip(trackedTransforms, Tuple.Create))
        {
            mjb.position = tr.position;
            mjb.rotation = tr.rotation;
        }

        foreach (var mcbd in mjMocapBodies)
        {
            mcbd.OnSyncState(MjScene.Instance.Data);
        }


    }

    private struct KinematicState
    {
        public readonly Vector3 angularVelocity;
        public readonly Vector3 linearVelocity;
        public readonly Vector3 position;
        public readonly Quaternion rotation;

        public KinematicState(Vector3 angularVelocity, Vector3 linearVelocity, Vector3 position, Quaternion rotation)
        {
            this.angularVelocity = angularVelocity;
            this.linearVelocity = linearVelocity;
            this.position = position;
            this.rotation = rotation;
        }
    }

    public void ReplaceGeoms()
    {
        foreach(var geom in weldRoot.parent.GetComponentsInChildren<MjGeom>())
        {

            if(geom.transform.parent.GetComponent<MjBody>() == null)
            {
                continue;
            }

            if(geom.transform.parent.GetComponentInDirectChildren<Collider>() != null && geom.transform.parent.GetComponentInDirectChildren<Collider>().name.Contains(geom.name.Replace("Geom", "")))
            {
                continue;
            }

            //var bodyGO = geom.transform.parent;
            // var colObject = new GameObject($"{geom.name.Replace("Geom", "")}Collider");

            //colObject.transform.SetParent(bodyGO);
            var colObject = geom.gameObject;
            colObject.transform.SetPositionAndRotation(geom.transform.position, geom.transform.rotation);
            

            switch (geom.ShapeType)
            {
                case MjShapeComponent.ShapeTypes.Capsule:
                    var capsule = colObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
                    capsule.radius = geom.Capsule.Radius;
                    capsule.height = 2 * (geom.Capsule.HalfHeight + capsule.radius);

                    break;

                case MjShapeComponent.ShapeTypes.Box:
                    var box = colObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
                    box.size = 2 * geom.Box.Extents;

                    break;

                case MjShapeComponent.ShapeTypes.Sphere:
                    var sphere = colObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
                    sphere.radius = geom.Sphere.Radius;

                    break;
            }

            //DestroyImmediate(geom.gameObject);
        }
    }
}

