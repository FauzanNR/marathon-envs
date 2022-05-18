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


        protected IReadOnlyList<Tuple<MjActuator, MjHingeJoint>> activeActRefPairs;
        protected IReadOnlyList<Tuple<MjActuator, MjHingeJoint>> passiveActRefPairs;

        private IReadOnlyList<MjActuator> actuators;
        public virtual IReadOnlyList<MjActuator> Actuators { get => actuatorRoot.GetComponentsInChildren<MjActuator>().ToList(); }

     
        [SerializeField]
        Transform kinematicRef;

        [SerializeField]
        bool trackPosition;
        [SerializeField]
        bool trackVelocity;

        private bool IsSubsetDefined { get => (actuatorSubset != null && actuatorSubset.Count > 0); }

        public override int ActionSpaceSize => IsSubsetDefined && kinematicRef && trackPosition ? actuatorSubset.Count : actuatorRoot.GetComponentsInChildren<MjActuator>().ToList().Count;

        float[] nextActions;

        unsafe private void UpdateTorque(object sender, MjStepArgs e)
        {
            List<IState> ICurrentState = new List<IState>();
            List<IState> ITargetState = new List<IState>();



            foreach ((var action, (var actuator, var reference)) in nextActions.Zip(activeActRefPairs, Tuple.Create))
            {
                /*
                var curState = new float[] { (float)e.data->qpos[actuator.Joint.QposAddress],
                                             (float)e.data->qvel[actuator.Joint.DofAddress],
                                             (float)e.data->qacc[actuator.Joint.DofAddress]};
                */


                ICurrentState.Add(new StaticState((float)e.data->qpos[actuator.Joint.QposAddress],
                                             (float)e.data->qvel[actuator.Joint.DofAddress],
                                             (float)e.data->qacc[actuator.Joint.DofAddress]));

                var targetState = trackPosition ? new float[] { (float)e.data->qpos[reference.QposAddress]+action,
                                                                    trackVelocity? (float)e.data->qvel[reference.DofAddress] : 0f} : new float[] { action, 0f };

                ITargetState.Add(new StaticState(targetState[0], targetState[1], targetState[2]));

                // float torque = updateRule.GetTorque(curState, targetState);
              
               // e.data->ctrl[actuator.MujocoId] = torque;
               // actuator.Control = torque;
            }
            float[] torques= updateRule.GetJointForces(ICurrentState.ToArray(), ITargetState.ToArray());
            Debug.LogError("TODO: finish updating the actuator with these torque values ");

            foreach ((var actuator, var reference) in passiveActRefPairs)
            {

                var curState = new float[] { (float)e.data->qpos[actuator.Joint.QposAddress],
                                             (float)e.data->qvel[actuator.Joint.DofAddress],
                                             (float)e.data->qacc[actuator.Joint.DofAddress]};
                var targetState = new float[] { (float)e.data->qpos[reference.QposAddress],
                                                 trackVelocity? (float)e.data->qvel[reference.DofAddress] : 0f};
                Debug.LogError("the calls in MjModularMuscles need to remove  the curState in the update call, and add it in the initialization phase");
                //float torque = updateRule.GetTorque(curState, targetState);
                //e.data->ctrl[actuator.MujocoId] = torque;
                //actuator.Control = torque;
            }
        }

        public override void ApplyActions(float[] actions, float actionTimeDelta)
        {
            nextActions = actions;
            
        }

        public override float[] GetActionsFromState()
        {
            if (trackPosition) return Enumerable.Repeat(0f, ActionSpaceSize).ToArray();
            if (kinematicRef) return activeActRefPairs.Select(a => Mathf.Deg2Rad * a.Item2.Configuration).ToArray();
            return activeActRefPairs.Select(a => a.Item1.Control).ToArray();
        }

        public override void OnAgentInitialize() 
        {
          
            MjScene.Instance.ctrlCallback += UpdateTorque;
            actuators = Actuators;

            if (IsSubsetDefined && kinematicRef && trackPosition)
            {
                var passiveActs = actuators.Where(a => !actuatorSubset.Contains(a));
                activeActRefPairs = actuatorSubset.Select(a => Tuple.Create(a, FindReference(a))).ToList();
                passiveActRefPairs = passiveActs.Select(a => Tuple.Create(a, FindReference(a))).ToList();
                return;
            }

            activeActRefPairs = actuators.Select(a => Tuple.Create(a, FindReference(a))).ToList();
            passiveActRefPairs = new List<Tuple<MjActuator, MjHingeJoint>>();
        }

        private void OnDisable()
        {

            if (MjScene.InstanceExists) MjScene.Instance.ctrlCallback -= UpdateTorque;
        }

        private MjHingeJoint FindReference(MjActuator act)
        {
            return kinematicRef? kinematicRef.GetComponentsInChildren<MjHingeJoint>().First(rj => rj.name.Contains(act.Joint.name)) : null;
        }
    }
}
