using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            if (gains.Length == 0)
                Debug.LogWarning("I am not applying any torque because the gains are null");
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


        public override List<float> GetJointForces(IState[] currentState, IState[] targetState)
        {

            float[] res = new float[currentState.Length];
            for (int i = 0; i < currentState.Length; i++)
            {
                res[i] = GetTorque(currentState[i].stateVector, targetState[i].stateVector);
            }
            return new List<float>(res);


        }





    }
}