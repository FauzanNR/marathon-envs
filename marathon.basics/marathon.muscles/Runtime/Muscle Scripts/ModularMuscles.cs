using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotorUpdate;
using Kinematic;

using Unity.Mathematics;

public abstract class ModularMuscles : Muscles
{

    [SerializeField]
    protected MotorUpdateRule updateRule;

    //protected IArticulation[] _motors;
    protected IKinematic[] _motors;

    //public abstract IArticulation[] GetMotors();
    public abstract IKinematic[] GetMotors();

    void Awake()
    {
        //Setup();

        _motors = GetMotors();


        if (updateRule != null)
            updateRule.Initialize(this);
        else
            Debug.LogError("there is no motor update rule");


    }


    public void ApplyRuleAsRelativeTorques( float3[] targetRotation)
    {



        float3[] torques = updateRule.GetJointForces(targetRotation);
        Debug.LogError("NEED TO APPLY THIS AS JOINT FORCES");

        /*
        for (int i = 0; i < _motors.Length; i++)
        {

            _motors[i].AddRelativeTorque(torques[i]);

        }
        */
    }

}