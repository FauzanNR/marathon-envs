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


        public override float[] GetJointForces(IState[] currentState, IState[] targetState) {

            float[] res = new float[currentState.Length];
            for (int i = 0; i < currentState.Length; i++)
            {
                res[i] = GetTorque(currentState[i].stateVector , targetState[i].stateVector);
            }
            return res;


        }






    }
}