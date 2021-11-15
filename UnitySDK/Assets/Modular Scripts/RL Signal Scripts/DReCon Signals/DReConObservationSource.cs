using DReCon;
using Kinematic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// State vector construction, as described by Bergamin et al.
/// </summary>
public class DReConObservationSource : ObservationSource
{
    
    [SerializeField]
    private Transform kinematicTransform;

    [SerializeField]
    private Transform simulationTransform;
    
    [SerializeField]
    [Tooltip("Order of transforms should be the same in both source and ragdoll")]
    private List<Transform> kinematicSubset;

    [SerializeField]
    [Tooltip("Order of transforms should be the same in both source and ragdoll")]
    private List<Transform> simulationSubset;


    private BodyChain kinChain;
    private BodyChain simChain;
    private BodyChain kinSubsetBodies;
    private BodyChain simSubsetBodies;

    //Editor friendly GameObject that contains an otherwise hard to reference interface on a component
    [SerializeField]
    private GameObject inputObject;
    private IAnimationController userInputs;

    [SerializeField]
    private GameObject agentObject;
    IRememberPreviousActions agent;

    public override int Size => 87 + agentObject.GetComponent<DReConAgent>().ActionSpaceSize;

    public override void OnAgentInitialize()
    {
        userInputs = inputObject.GetComponent<IAnimationController>();
        agent = agentObject.GetComponent<IRememberPreviousActions>();

        kinChain = new BodyChain(kinematicTransform);
        simChain = new BodyChain(simulationTransform);
        kinSubsetBodies = new BodyChain(kinematicSubset);
        simSubsetBodies = new BodyChain(simulationSubset);
    }

    public override void FeedObservationsToSensor(VectorSensor sensor)
    {
        ReferenceFrame fKin = new ReferenceFrame(kinChain.RootForward, kinChain.CenterOfMass);
        ReferenceFrame fSim = new ReferenceFrame(kinChain.RootForward, simChain.CenterOfMass); // Same orientation, different origin

        Vector3 kinCOMV = fKin.WorldDirectionToCharacter(kinChain.CenterOfMassVelocity);
        Vector3 simCOMV = fSim.WorldDirectionToCharacter(simChain.CenterOfMassVelocity);

        Vector3 inputDesiredVelocity = fKin.WorldToCharacter(userInputs.GetDesiredVelocity());
        Vector2 inputDesiredHorizontalVelocity = inputDesiredVelocity.Horizontal();

        // TODO: Specific actions, not implemented. Could also replaced by one-hot encoding.
        bool inputJump = false;
        bool inputBackflip = false;

        Vector2 horizontalVelocityDifference = inputDesiredHorizontalVelocity - simCOMV.Horizontal();

        sensor.AddObservation(kinCOMV);
        sensor.AddObservation(simCOMV);
        sensor.AddObservation(kinCOMV-simCOMV);
        sensor.AddObservation(inputDesiredHorizontalVelocity);
        sensor.AddObservation(inputJump);
        sensor.AddObservation(inputBackflip);
        sensor.AddObservation(horizontalVelocityDifference);

        Debug.Log(kinCOMV);
        Debug.Log(simCOMV);


        foreach (var ((pSim, vSim), (pKin, vKin) ) in GetZippedStats(simSubsetBodies).Zip(GetZippedStats(kinSubsetBodies), Tuple.Create))
        {
            sensor.AddObservation(fSim.WorldToCharacter(pSim));
            sensor.AddObservation(fSim.WorldToCharacter(vSim));

            // For consistency with previous Marathon Controller Implementation, although original DReCon uses -1 times this
            sensor.AddObservation(fSim.WorldDirectionToCharacter(pKin - pSim));
            sensor.AddObservation(fSim.WorldDirectionToCharacter(vKin - vSim));
        }
        sensor.AddObservation(agent.PreviousActions);
    }

    IEnumerable<Tuple<Vector3, Vector3>> GetZippedStats(BodyChain ch)
    {
        return ch.CentersOfMass.Zip(simSubsetBodies.Velocities, Tuple.Create);
    }

    private void OnDrawGizmos()
    {
        return;
        if (!Application.isPlaying) return;
        
        ReferenceFrame fKin = new ReferenceFrame(kinChain.RootForward, kinChain.CenterOfMass);
        ReferenceFrame fSim = new ReferenceFrame(kinChain.RootForward, simChain.CenterOfMass);

        fKin.Draw();
        fSim.Draw();
    }
}

namespace DReCon
{
    struct ReferenceFrame
    {
        Matrix4x4 space;
        Matrix4x4 inverseSpace;

        public Matrix4x4 Matrix { get => space; }

        public ReferenceFrame(Vector3 heading, Vector3 centerOfMass)
        {
            // Instead of using the heading as the LookAt direction, we use world up, and set heading as the LookAt "up"
            // This gives us the horizontal projection of the heading for free
            space = Matrix4x4.LookAt(centerOfMass, centerOfMass + Vector3.up, heading);
            // In this representation z -> up, y -> forward, x -> left

            // So this means we have to roll the axes if we want z -> forward, y -> up, x -> right for consistency
            // Note that as long as the state representation from this source was consistent, this step would not actually be necessary
            // It just changes the order the dimension components are fed into the sensor.
            space = new Matrix4x4(-space.GetColumn(0), space.GetColumn(2), space.GetColumn(1), space.GetColumn(3));
            inverseSpace = space.inverse;
        }

        public Vector3 WorldToCharacter(Vector3 position)
        {
            return inverseSpace.MultiplyPoint3x4(position);
        }

        public Vector3 CharacterToWorld(Vector3 position)
        {
            return space.MultiplyPoint3x4(position);
        }

        public Vector3 WorldDirectionToCharacter(Vector3 vector)
        {
            return inverseSpace.MultiplyVector(vector);
        }

        public Vector3 CharacterDirectionToWorld(Vector3 vector)
        {
            return space.MultiplyVector(vector);
        }

        public override string ToString()
        {
            return $"Orientation: {space.rotation.eulerAngles}\nPosition: {(Vector3)space.GetColumn(3)}";
        }

        public void Draw()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(space.GetColumn(3), space.GetColumn(0) * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(space.GetColumn(3), space.GetColumn(1) * 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(space.GetColumn(3), space.GetColumn(2) * 0.5f);
        }
    }
}