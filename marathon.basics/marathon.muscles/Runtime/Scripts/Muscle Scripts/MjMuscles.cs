using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System;

namespace Mujoco
{

    public class MjMuscles : Muscles
    {
        [SerializeField]
        protected Transform actuatorRoot;

        protected IReadOnlyList<MjActuator> actuators;

        public virtual IReadOnlyList<MjActuator> Actuators { get => actuatorRoot.GetComponentsInChildren<MjActuator>().ToList(); }

        private void Awake()
        {
            OnAgentInitialize();
        }

        public override int ActionSpaceSize => actuatorRoot.GetComponentsInChildren<MjActuator>().ToList().Count;

        //public override void ApplyActions(float[] actions, float actionTimeDelta)
        public override void ApplyActions(float[] actions)
        {
            foreach ((var action, var actuator) in actions.Zip(actuators, Tuple.Create))
            {
                actuator.Control = action;
            }
        }

        public override float[] GetActionsFromState()
        {
            return actuatorRoot.GetComponentsInChildren<MjActuator>().Select(a => a.Control).ToArray();
        }

        public override void OnAgentInitialize() { actuators = Actuators; }
    }
}