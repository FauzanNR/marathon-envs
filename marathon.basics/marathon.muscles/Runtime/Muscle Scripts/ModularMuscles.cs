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

  

    
    public abstract IKinematic[] GetMotors();

   

    public void ApplyRuleAsRelativeTorques( float3[] targetRotation)
    {



        float3[] torques = updateRule.GetJointForces(targetRotation);
        Debug.LogError("NEED TO APPLY THIS AS JOINT FORCES");

     
    }

}