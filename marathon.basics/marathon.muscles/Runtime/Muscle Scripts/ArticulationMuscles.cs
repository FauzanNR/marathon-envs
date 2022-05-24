using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using MotorUpdate;




using Kinematic;

public class ArticulationMuscles : ModularMuscles
{
    /*Assumptions in this model:
        * 1. We provide a kinematic reference directly to the PD controllers
        * 2. The action array trained has per main purpose to learn  what to "add" to  the kinematic reference to keep balance
        */

    [SerializeField]
    ArticulationBody root;


    protected ArticulationBody[] _motors;

    [SerializeField]
    Transform kinematicRef;

    //we need 6 extra zeros to apply nothing to the root Articulation when we do apply actions
    float[] nullactions4root = new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

    [SerializeField]
    protected List<ArticulationBody> actuatorSubset;

    protected IReadOnlyList<ActuatorReferencePair> actuatorPairs;
    public virtual IReadOnlyList<ArticulationBody> Actuators { get => Utils.GetArticulationMotors(gameObject); }
    public override void OnAgentInitialize()
    {

        base.OnAgentInitialize();

        IReadOnlyList<ArticulationBody> subset = actuatorSubset == null ? new List<ArticulationBody> { } : actuatorSubset;
        actuatorPairs = Actuators.Select(a => new ActuatorReferencePair(a, FindReference(a), subset.Contains(a))).ToList();


    }


    private Rigidbody FindReference(ArticulationBody act)
    {
        return kinematicRef ? kinematicRef.GetComponentsInChildren<Rigidbody>().First(rj => rj.name.Contains(act.name)) : null;
    }

    protected class ActuatorReferencePair
    {
        public readonly ArticulationBody act;
        public readonly RigidbodyAdapter reference;
        public readonly bool active;

        public readonly ArticulationBodyAdapter aba;

        public ActuatorReferencePair(ArticulationBody act, Rigidbody reference, bool active)
        {
            this.act = act;
            this.reference = new RigidbodyAdapter(reference);
            aba = new ArticulationBodyAdapter(act);
            this.active = active;
        }

        


    }


        public override int ActionSpaceSize
    {
        get => GetActionsFromState().Length;
    }



 
    public override float[] GetActionsFromState()
    {
        
        var vectorActions = new List<float>();
        //foreach (var m in _motors)
        foreach (var actupair in actuatorPairs)
        {
            var m = actupair.act;
            if (m.isRoot)
                continue;
            //int i = 0;
            if (m.jointType != ArticulationJointType.SphericalJoint)
                continue;
            if (m.twistLock != ArticulationDofLock.LockedMotion)
            {

                vectorActions.Add(0f);
                //  vectorActions.Add(m.jointPosition[i++]);
            }
            if (m.swingYLock != ArticulationDofLock.LockedMotion)
            {

                vectorActions.Add(0f);
                //  vectorActions.Add(m.jointPosition[i++]);
            }
            if (m.swingZLock != ArticulationDofLock.LockedMotion)
            {

                vectorActions.Add(0f);
                //  vectorActions.Add(m.jointPosition[i++]);
            }
        }
        return vectorActions.ToArray();
    }
  

    public override void ApplyActions(float[] actions)
    {

       

        var currentStates = new List<IState>();
        var targetStates = new List<IState>();

        int j = 0;

        foreach (ActuatorReferencePair actPair in actuatorPairs)
        {

            if (actPair.act.isRoot)
            {
                Debug.LogError("The ROOT should not be in the actuators");
            }
                
         
            Vector3 refPosInReducedCoordinates = actPair.reference.JointPosition;
            Vector3 refVelInReducedCoordinates = actPair.reference.JointVelocity;

            if (actPair.act.twistLock != ArticulationDofLock.LockedMotion)
            {
                currentStates.Add(new StaticState(actPair.aba.JointPosition.x, actPair.aba.JointVelocity.x, actPair.aba.JointAcceleration.x));
                targetStates.Add(new StaticState(refPosInReducedCoordinates.x + actions[j], refVelInReducedCoordinates.x + actions[j] / _deltaTime, 0));
                j++;

            }
            if (actPair.act.swingYLock != ArticulationDofLock.LockedMotion)
            {
                currentStates.Add(new StaticState(actPair.aba.JointPosition.y, actPair.aba.JointVelocity.y, actPair.aba.JointAcceleration.y));
                targetStates.Add(new StaticState(refPosInReducedCoordinates.y + actions[j], refVelInReducedCoordinates.y + actions[j] / _deltaTime, 0));
                j++;
            }
            if (actPair.act.swingZLock != ArticulationDofLock.LockedMotion)
            {
                currentStates.Add(new StaticState(actPair.aba.JointPosition.z, actPair.aba.JointVelocity.z, actPair.aba.JointAcceleration.z));
                targetStates.Add(new StaticState(refPosInReducedCoordinates.z + actions[j], refVelInReducedCoordinates.z + actions[j] / _deltaTime, 0));
                j++;
            }


        }


        float[] torques = updateRule.GetJointForces(currentStates.ToArray(), targetStates.ToArray());

        root.SetJointForces(nullactions4root.Concat(torques).ToList());
       
     
    }

}
