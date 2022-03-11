using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mujoco;
using System;
using Kinematic;

public class MjKinematicRig : MonoBehaviour, IKinematicReference
{
    [SerializeField]
    private Transform weldRoot;

    [SerializeField]
    private Transform trackedTransformRoot;

    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private string prefix;

    // Private, since other scripts should reference rigidbodies from the hierarchy, and not depend on KinematicRig implementation if possible
    private IReadOnlyList<Transform> riggedTransforms;
    private IReadOnlyList<Transform> trackedTransforms;

    public IReadOnlyList<Transform> RagdollTransforms => bodies.Select(bd => bd.transform).ToList();

    private IReadOnlyList<MjMocapBody_> mjMocapBodies;

    private IReadOnlyList<MjBody> bodies => weldRoot.parent.GetComponentsInChildren<MjBody>().ToList();

    public IReadOnlyList<MjBody> Bodies { get => bodies; }

    public IReadOnlyList<Vector3> RagdollLinVelocities => throw new NotImplementedException();

    public IReadOnlyList<Vector3> RagdollAngularVelocities => throw new NotImplementedException();

    public IReadOnlyList<IKinematic> Kinematics => weldRoot.parent.GetComponentsInChildren<MjBody>().Select(mjb => (IKinematic) new MjBodyAdapter(mjb)).ToList();


    public void Awake()
    {
        OnAgentInitialize();
    }

    void OnAnimatorIK(int k)
    {
        TrackKinematics();
    }

    public void OnAgentInitialize()
    {
        Func<string, string> MocapName = animatedName => $"{prefix}{Utils.SegmentName(animatedName)}";
        riggedTransforms = weldRoot.GetComponentsInChildren<MjWeld>().Select(krt => krt.Body1.transform).Where(t => t.name.Contains("Mocap") && t.gameObject.activeSelf).ToList().AsReadOnly();

        trackedTransforms = riggedTransforms.Select(rt => trackedTransformRoot.GetComponentsInChildren<Transform>().First(tt => MocapName(tt.name).Equals(rt.name))).ToList().AsReadOnly();

        mjMocapBodies = riggedTransforms.Select(t => t.GetComponent<MjMocapBody_>()).ToList();

    }

    public unsafe void TrackKinematics()
    {
        foreach ((var mjb, var tr) in riggedTransforms.Zip(trackedTransforms, Tuple.Create))
        {
            mjb.position = tr.position;
            mjb.rotation = tr.rotation;
        }

/*        foreach (var mcbd in mjMocapBodies)
        {
            mcbd.OnSyncState(MjScene.Instance.Data);
        }*/

    }

    public void TeleportRoot(Vector3 position, Quaternion rotation)
    {
        TrackKinematics();
        MjScene.Instance.TeleportMjRoot(weldRoot.parent.GetComponentInChildren<MjFreeJoint>(), position, rotation);
    }

    public void TeleportRoot(Vector3 position)
    {
        TrackKinematics();
        MjScene.Instance.TeleportMjRoot(weldRoot.parent.GetComponentInChildren<MjFreeJoint>(), position, riggedTransforms[0].rotation);
    }


   
}

