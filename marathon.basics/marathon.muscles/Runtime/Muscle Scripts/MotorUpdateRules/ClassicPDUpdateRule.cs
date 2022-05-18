using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using Unity.MLAgents;
using Kinematic;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "ClassicPD", menuName = "ScriptableObjects/ClassicPD", order = 1)]
    public class ClassicPDUpdateRule : MotorUpdateRule
    {
              

    
      
        public float DampingRatio = 1.0f;
        public float NaturalFrequency = 40f;

      



        float3 GetRelativeTorque(IKinematic joint, float3 targetRotation)
        {
            //for AddRelativeTorque
            var m = joint.Mass;
          
            var n = NaturalFrequency; // n should be in the range 1..20
            var stiffness = Mathf.Pow(n, 2) * m;
            var damping = DampingRatio * (2 * Mathf.Sqrt(stiffness * m));
            

            float3 targetVel = Utils.AngularVelocityInReducedCoordinates(joint.JointPosition, targetRotation, dT);

            //we calculate the force:
            float3 torque = stiffness * (joint.JointPosition - targetRotation)*Mathf.Deg2Rad - damping * (joint.JointVelocity - targetVel) * Mathf.Deg2Rad;
            return torque;



        }

        public override float[] GetJointForces(IState[] currentState, IState[] targetState) {
            Debug.LogError("Need to implmeent this");
            return new float[1] { 0.0f };
        
        }
       




    }
}