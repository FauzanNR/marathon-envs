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

    //we need 6 extra zeros to apply nothing to the root Articulation when we do apply actions
    float[] nullactions4root = new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };


    public override int ActionSpaceSize
    {
        get => GetActionsFromState().Length;
    }


    // Use this for initialization
    void Awake()
    {
        //Setup();

        _motors = GetMotors();

        updateRule.Initialize(this, Time.fixedDeltaTime);
        Debug.LogWarning("TODO: check if the update time is the fixedDeltaTime or something else");
        if (updateRule != null)
            updateRule.Initialize(this);
        else
            Debug.LogError("there is no motor update rule");

     
    }


 
    public override float[] GetActionsFromState()
    {
        
        var vectorActions = new List<float>();
        foreach (var m in GetArticulationMotors())
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

        /*
        List<int> startIndices = new List<int>();

        root.GetDofStartIndices(startIndices);
        */


        root.SetJointForces(nullactions4root.Concat(actions).ToList());
       
     
    }

    /*
    void  ApplyRuleAsRelativeTorques(IKinematic[] joints, float3[] targetRotation)
    {



        float3[] torques = updateRule.GetJointForces( targetRotation);
        for (int i = 0; i < _motors.Length; i++)
        {
            Debug.LogError("TODO: Articulation: " + _motors[i].Name + " has a torque calculated for it of: " + torques[i] + "now we need to apply it");
            //_motors[i].AddRelativeTorque(torques[i]);

        }

    }
    */

    List<ArticulationBody> GetArticulationMotors()
    {


        return GetComponentsInChildren<ArticulationBody>()
                .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
                .Where(x => !x.isRoot)
                .Distinct()
                .ToList();


    }

    public override IKinematic[] GetMotors()
    {
          List<IKinematic> result = new List<IKinematic>();
       
        List<ArticulationBody> abl = GetArticulationMotors();


        foreach (ArticulationBody a in abl)
        {
            result.Add(new ArticulationBodyAdapter(a));
        }

        return result.ToArray();

    }





}
