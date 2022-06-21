using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "Proportional", menuName = "ScriptableObjects/POnly", order = 1)]
    public class POnlyUpdateRule : MotorUpdateRule
    {
        [SerializeField]
        protected float dT = 1 / 60;
        public override float GetTorque(float[] curState, float[] targetState)
        {

            return -gains[0] * (curState[0] + dT * curState[1] - targetState[0]);

        }

        public override float GetTorque(IState curState, IState targetState)
        {
            return -gains[0] * (curState.Position + curState.Velocity*dT - targetState.Position);
        }
    }
}