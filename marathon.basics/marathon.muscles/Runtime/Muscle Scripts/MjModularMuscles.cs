using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;



using MotorUpdate;

namespace Mujoco
{
    public class MjModularMuscles : ModularMuscles
    {
        [SerializeField]
        protected Transform actuatorRoot;

        [SerializeField]
        protected List<MjActuator> actuatorSubset;

        protected IReadOnlyList<ActuatorReferencePair> actuatorPairs;

        public virtual IReadOnlyList<MjActuator> Actuators { get => actuatorRoot.GetComponentsInChildren<MjActuator>().ToList(); }

     
        [SerializeField]
        Transform kinematicRef;

        [SerializeField]
        bool trackPosition;
        [SerializeField]
        bool trackVelocity;

        private bool IsSubsetDefined { get => (actuatorSubset != null && actuatorSubset.Count > 0); }

        public override int ActionSpaceSize => actuatorPairs.Where(arp=>arp.active).Count();

        float[] nextActions;

        unsafe private void UpdateTorque(object sender, MjStepArgs e)
        {
            UpdateTorques(nextActions, actuatorPairs.Where(arp => arp.active), e);
            UpdateTorques(Enumerable.Repeat(0f, actuatorPairs.Count-nextActions.Length).ToArray(), actuatorPairs.Where(arp => !arp.active), e);

        }

        unsafe private void UpdateTorques(float[] actions, IEnumerable<ActuatorReferencePair> arps, MjStepArgs e)
        {
            List<IState> currentStates = new List<IState>();
            List<IState> ITargetState = new List<IState>();

            foreach ((var action, ActuatorReferencePair arp) in nextActions.Zip(arps, Tuple.Create))
            {


                currentStates.Add(new StaticState((float)e.data->qpos[arp.act.Joint.QposAddress],
                                             (float)e.data->qvel[arp.act.Joint.DofAddress],
                                             (float)e.data->qacc[arp.act.Joint.DofAddress]));

                var targetState = trackPosition ? new float[] { (float)e.data->qpos[arp.reference.QposAddress]+action,
                                                                    trackVelocity? (float)e.data->qvel[arp.reference.DofAddress] : 0f, 0f} : new float[] { action, 0f, 0f };

                ITargetState.Add(new StaticState(targetState[0], targetState[1], targetState[2]));
            }

            float[] torques = updateRule.GetJointForces(currentStates.ToArray(), ITargetState.ToArray());

            foreach ((var torque,  ActuatorReferencePair arp) in torques.Zip(arps, Tuple.Create))
            {
                e.data->ctrl[arp.act.MujocoId] = torque;
                arp.act.Control = torque;
            }

        }

        public override void ApplyActions(float[] actions)
        {
            nextActions = actions;
            
        }

        public override float[] GetActionsFromState()
        {
            if (trackPosition) return Enumerable.Repeat(0f, ActionSpaceSize).ToArray();
            if (kinematicRef) return actuatorPairs.Where(arp=>arp.active).Select(arp => Mathf.Deg2Rad * arp.reference.Configuration).ToArray();
            return actuatorPairs.Select(a => a.act.Control).ToArray();
        }

        public override void OnAgentInitialize() 
        {
          
            MjScene.Instance.ctrlCallback += UpdateTorque;
            IReadOnlyList<MjActuator>  actuators = Actuators;
            IReadOnlyList<MjActuator> subset = actuatorSubset == null ? new List<MjActuator> { } : actuatorSubset;

            if (IsSubsetDefined && kinematicRef && trackPosition)
            {
                actuatorPairs = actuators.Select(a => new ActuatorReferencePair(a, FindReference(a), subset.Contains(a))).ToList();
                return;
            }

            actuatorPairs = actuators.Select(a => new ActuatorReferencePair(a, FindReference(a), true)).ToList();
        }

        private void OnDisable()
        {

            if (MjScene.InstanceExists) MjScene.Instance.ctrlCallback -= UpdateTorque;
        }

        private MjHingeJoint FindReference(MjActuator act)
        {
            return kinematicRef? kinematicRef.GetComponentsInChildren<MjHingeJoint>().First(rj => rj.name.Contains(act.Joint.name)) : null;
        }

        protected class ActuatorReferencePair
        {
            public readonly MjActuator act;
            public readonly MjHingeJoint reference;
            public readonly bool active;

            public ActuatorReferencePair(MjActuator act, MjHingeJoint reference, bool active)
            {
                this.act = act;
                this.reference = reference;
                this.active = active;
            }
        }
    }
}
