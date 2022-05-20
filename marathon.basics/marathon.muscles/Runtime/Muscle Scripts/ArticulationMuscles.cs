using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using MotorUpdate;
using Unity.Mathematics;
using Unity.MLAgents;//for the DecisionRequester


using Kinematic;

public class ArticulationMuscles : ModularMuscles
{


    [SerializeField]
    ArticulationBody root;


   // protected IKinematic[] _motors;



    //we need 6 extra zeros to apply nothing to the root Articulation when we do apply actions
    float[] nullactions4root = new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };


    public override void OnAgentInitialize()
    {

     //   _motors = Utils.GetMotors(gameObject);


        if (updateRule != null)
        {
                updateRule.Initialize(this, Utils.GetActionTimeDelta(gameObject.GetComponent<DecisionRequester>() ) );
        }
           
        else
            Debug.LogError("there is no motor update rule");


    }




    public override int ActionSpaceSize
    {
        get => GetActionsFromState().Length;
    }



 
    public override float[] GetActionsFromState()
    {
        
        var vectorActions = new List<float>();
        foreach (var m in Utils.GetArticulationMotors(gameObject))
        {
            if (m.isRoot)
                continue;
            int i = 0;
            if (m.jointType != ArticulationJointType.SphericalJoint)
                continue;
            if (m.twistLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = m.xDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var deg = m.jointPosition[i++] * Mathf.Rad2Deg;
                var target = (deg - midpoint) / scale;
                vectorActions.Add(target);
            }
            if (m.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = m.yDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var deg = m.jointPosition[i++] * Mathf.Rad2Deg;
                var target = (deg - midpoint) / scale;
                vectorActions.Add(target);
            }
            if (m.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = m.zDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var deg = m.jointPosition[i++] * Mathf.Rad2Deg;
                var target = (deg - midpoint) / scale;
                vectorActions.Add(target);
            }
        }
        return vectorActions.ToArray();
    }
  

    public override void ApplyActions(float[] actions, float actionTimeDelta)
    {

        Debug.LogError("we need to actually ask the update rule for the torques to apply");

        /*
        float[] torques = updateRule.GetJointForces(currentStates.ToArray(), ITargetState.ToArray());

        foreach ((var action, ActuatorReferencePair arp) in nextActions.Zip(arps, Tuple.Create))
        {


            currentStates.Add(new StaticState((float)e.data->qpos[arp.act.Joint.QposAddress],
                                         (float)e.data->qvel[arp.act.Joint.DofAddress],
                                         (float)e.data->qacc[arp.act.Joint.DofAddress]));

            var targetState = trackPosition ? new float[] { (float)e.data->qpos[arp.reference.QposAddress]+action,
                                                                    trackVelocity? (float)e.data->qvel[arp.reference.DofAddress] : 0f, 0f} : new float[] { action, 0f, 0f };

            ITargetState.Add(new StaticState(targetState[0], targetState[1], targetState[2]));
        }

        */

        root.SetJointForces(nullactions4root.Concat(actions).ToList());
       
     
    }

   





}
