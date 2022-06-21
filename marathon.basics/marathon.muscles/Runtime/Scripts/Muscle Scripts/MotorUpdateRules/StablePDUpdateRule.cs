using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "StablePD", menuName = "ScriptableObjects/StablePD", order = 1)]
    public class StablePDUpdateRule : MotorUpdateRule
    {
        [SerializeField]
        protected float dT = 1 / 60;

        public override float GetTorque(float[] curState, float[] targetState)
        {

            return -gains[0] * (curState[0] + dT * curState[1] - targetState[0]) - gains[1] * (curState[1] + dT * curState[2] - targetState[1]);

        }

        public override float GetTorque(IState curState, IState targetState)
        {
            return -gains[0] * (curState.Position + curState.Velocity * dT - targetState.Position) - gains[1] * (curState.Velocity + curState.Acceleration * dT - targetState.Velocity);
        }
    }
}