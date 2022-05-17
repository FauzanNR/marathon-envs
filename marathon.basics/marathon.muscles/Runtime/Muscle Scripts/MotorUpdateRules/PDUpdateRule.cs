using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "PD", menuName = "ScriptableObjects/PD", order = 1)]
    public class PDUpdateRule : MotorUpdateRule
    {


        [SerializeField]
        protected float[] gains;


        float GetTorque(float[] curState, float[] targetState)
        {
            float res = 0;
            for (int i = 0; i < gains.Length; i++)
            {
                res -= gains[i] * (curState[i] - targetState[i]);
            }
            return res;
        }

         float GetTorque(IState curState, IState targetState)
        {
            return GetTorque(curState.stateVector, targetState.stateVector);
        }


        public override  float3[] GetJointForces(float3[] targetRotation)
        {
            float3[] result = new float3[_motors.Length];

            for (int i = 0; i < _motors.Length; i++)
            {

                Debug.LogError("TODO: convert the 3D target rotations to Istates (which are unidimensionals");
               // result[i] = GetTorque(_motors[i], targetRotation[i]);

              //  IState()
            }


            return result;

        }

     

    }
}