using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The kinematic rig maps the source avatar's movement to the standard MarathonController hierarchy, so properties of the kinematic controller's segments can be queried.
/// A new class inheriting from KinematicRig should be implemented for new animation -> ragdoll mapping.
/// </summary>
public class KinematicRig : MonoBehaviour, IKinematicReference
{
    [SerializeField]
    private Transform kinematicRagdollRoot;

    [SerializeField]
    private Transform trackedTransformRoot;

    [SerializeField]
    private Vector3 offset;


    // Private, since other scripts should reference rigidbodies from the hierarchy, and not depend on KinematicRig implementation if possible
    private IReadOnlyList<Rigidbody> riggedRigidbodies;
    private IReadOnlyList<Transform> trackedTransforms;

    public List<Transform> RagdollTransforms => riggedRigidbodies.Select(rb => rb.transform).ToList();

    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        TrackKinematics();
    }

    public void Initialize()
    {
        riggedRigidbodies = kinematicRagdollRoot.GetComponentsInChildren<Rigidbody>();
        trackedTransforms = trackedTransformRoot.GetComponentsInChildren<Transform>();
        (riggedRigidbodies, trackedTransforms) = MarathonControllerMapping(riggedRigidbodies, trackedTransforms);

        Debug.Log(string.Join(", ", riggedRigidbodies.Select(rb => rb.name.Split(':').Last())));
        Debug.Log(string.Join(", ", trackedTransforms.Select(t => t.name.Split(':').Last())));
    }

    public void TrackKinematics()
    {
        foreach((var rb, var tr) in riggedRigidbodies.Zip(trackedTransforms, Tuple.Create))
        {
            rb.MoveRotation(tr.rotation);
            rb.MovePosition(tr.position + offset);
        }
    }

    private (IReadOnlyList<Rigidbody>, IReadOnlyList<Transform>) MarathonControllerMapping(IReadOnlyList<Rigidbody> rigidbodies, IReadOnlyList<Transform> transforms)
    {
        List<string> rigidbodyNames = riggedRigidbodies.Select(rb => rb.name.Split(':').Last()).ToList();
        transforms = transforms.Where(t => rigidbodyNames.Contains(t.name.Split(':').Last())).ToList();

        return (rigidbodies, transforms);
    }
}

/// <summary>
/// Temporary interface so both KinematicRig and MapAnimation2Ragdoll works
/// </summary>
public interface IKinematicReference
{
    public List<Transform> RagdollTransforms { get; }
}