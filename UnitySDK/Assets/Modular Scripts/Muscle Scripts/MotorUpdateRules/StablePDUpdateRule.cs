using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "StablePD", menuName = "ScriptableObjects/StablePD", order = 1)]
    public class StablePDUpdateRule : MotorUpdateRule
    {
        public override float GetTorque(float[] curState, float[] targetState)
        {
            float res = 0;
            for (int i=0; i<2; i++)
            {
                res -= gains[i] * (curState[i] + dT*curState[i+1] - targetState[i]);
            }
            return res;
        }

        public override float GetTorque(IState curState, IState targetState)
        {
            Debug.Log($"The position is {curState.Position}\nThe target is {targetState.Position}\nThe velocity is {curState.Velocity}\nTherefore the error is {curState.Position + curState.Velocity * dT - targetState.Position}\n\nThe acceleration is {curState.Acceleration}\n\n-{gains[0]} * {(curState.Position + curState.Velocity*dT - targetState.Position)} - {gains[1]} * {(curState.Velocity + curState.Acceleration*dT - targetState.Velocity)}" );
            return -gains[0] * (curState.Position + curState.Velocity*dT - targetState.Position) - gains[1] * (curState.Velocity + curState.Acceleration*dT - targetState.Velocity);
        }
    }
}