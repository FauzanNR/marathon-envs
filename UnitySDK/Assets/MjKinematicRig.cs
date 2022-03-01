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
    private IReadOnlyList<MjBody> riggedMjBodies;

    public List<Transform> RagdollTransforms => riggedTransforms.ToList();

    public IEnumerable<MjBody> MjBodies => riggedMjBodies;
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
        Debug.Log(string.Join(", ", riggedTransforms.Select(t => t.name)));
        Debug.Log(string.Join(", ", riggedTransforms.Select(rt => MocapName(rt.name))));
        trackedTransforms = riggedTransforms.Select(rt => trackedTransformRoot.GetComponentsInChildren<Transform>().First(tt => MocapName(tt.name).Equals(rt.name))).ToList().AsReadOnly();
        Debug.Log(string.Join(", ", trackedTransforms.Select(t => t.name)));
    }

    public void TrackKinematics()
    {
        foreach ((var mjb, var tr) in riggedTransforms.Zip(trackedTransforms, Tuple.Create))
        {
            mjb.position = tr.position;
            mjb.rotation = tr.rotation;
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
}

