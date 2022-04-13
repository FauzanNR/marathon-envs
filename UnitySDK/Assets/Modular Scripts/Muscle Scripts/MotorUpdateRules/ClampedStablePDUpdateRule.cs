using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "ClampedStablePD", menuName = "ScriptableObjects/ClampedStablePD", order = 1)]
    public class ClampedStablePDUpdateRule : MotorUpdateRule
    {
        [SerializeField]
        float velClamp = 100f;

        [SerializeField]
        float accClamp = 100f;

        [SerializeField]
        protected float dT = 1 / 60;

        public override float GetTorque(float[] curState, float[] targetState)
        {
 
            return  -gains[0] * (curState[0] + dT* Mathf.Clamp(curState[1], -velClamp, velClamp) - targetState[0]) - gains[1] * (Mathf.Clamp(curState[1], -velClamp, velClamp) + dT * Mathf.Clamp(curState[2], -accClamp, accClamp) - Mathf.Clamp(targetState[1], -velClamp, velClamp));

        }

        public override float GetTorque(IState curState, IState targetState)
        {
            Debug.Log($"The position is {curState.Position}\nThe target is {targetState.Position}\nThe velocity is {curState.Velocity}\nTherefore the error is {curState.Position + curState.Velocity * dT - targetState.Position}\n\nThe acceleration is {curState.Acceleration}\n\n-{gains[0]} * {(curState.Position + curState.Velocity*dT - targetState.Position)} - {gains[1]} * {(curState.Velocity + curState.Acceleration*dT - targetState.Velocity)}" );
            return -gains[0] * (curState.Position + dT* Mathf.Clamp(curState.Velocity, -velClamp, velClamp) - targetState.Position) - gains[1] * (Mathf.Clamp(curState.Velocity, -velClamp, velClamp) + dT* Mathf.Clamp(curState.Acceleration, -accClamp, accClamp) - Mathf.Clamp(targetState.Velocity, -velClamp, velClamp));
        }
    }
}