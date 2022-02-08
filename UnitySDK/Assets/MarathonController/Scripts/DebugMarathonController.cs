using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class DebugMarathonController : MonoBehaviour
{
  


    [Tooltip("Apply a random number to each action each framestep, set the character to have no motor update, or imitate a target animation")]
    public DebugModes debugMode;
    public enum DebugModes { 
        randomActions,
        moveWithDebugJoints,
        imitateTargetAnim
       

    }


    [Range(0f, 1f)]
    public float RandomRange = 1f;

    [Tooltip("Action applied to each motor")]
    public float[] Actions;


    [HideInInspector]
    public List<Rigidbody> targets4imitation;


    public void ApplyDebugActions(List<ArticulationBody> _motors, Muscles _ragDollMuscles, float actionTimeDelta) {

        GetDebugActions(_motors);


        switch (debugMode)
        {

            case DebugMarathonController.DebugModes.imitateTargetAnim:
                _ragDollMuscles.MimicRigidBodies(targets4imitation, actionTimeDelta);
                break;
            case DebugMarathonController.DebugModes.randomActions:
               
                _ragDollMuscles.UpdateMuscles(Actions, actionTimeDelta);
                break;
                //the  case  MarathonTestBedController.DebugModes.moveWithDebugJoints is handled directly by the DebugJoints components.

        }

    }

    float[]  GetDebugActions(List<ArticulationBody> _motors)
    {
        var debugActions = new List<float>();
        foreach (var m in _motors)
        {
            if (m.isRoot)
                continue;
           

            if (m.jointType != ArticulationJointType.SphericalJoint)
                continue;
            if (m.twistLock == ArticulationDofLock.LimitedMotion)
                debugActions.Add(0);
            if (m.swingYLock == ArticulationDofLock.LimitedMotion)
                debugActions.Add(0);
            if (m.swingZLock == ArticulationDofLock.LimitedMotion)
                debugActions.Add(0);
        }


        switch (debugMode)
        {
            case DebugModes.randomActions:
                Actions = debugActions.Select(x => Random.Range(- RandomRange, RandomRange)).ToArray();
                break;
            case DebugModes.moveWithDebugJoints:
                //Actions = debugActions.Select(x => 0f).ToArray();
                //THIS IS DONE DIRECTLY IN DebugJoints
                break;
            case DebugModes.imitateTargetAnim:
                //THIS IS DONE DIRECTLY IN THE ProcRagdollAgent

                break;

        }


       Actions = debugActions.ToArray();
        return Actions;
    }



}
