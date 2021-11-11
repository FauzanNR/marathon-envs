using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using ManyWorlds;
using UnityEngine.Assertions;

using System;

namespace DReCon
{
    [RequireComponent(typeof(Muscles))]
    public class DReConAgent : Agent, IRememberPreviousActions, IEventsAgent
    {
        [Header("Settings")]
        [SerializeField]
        private float fixedDeltaTime = 1f / 60f;
        [SerializeField]
        private float actionSmoothingBeta = 0.2f;

        [SerializeField]
        KinematicRig kinematicRig;

        [SerializeField]
        ObservationSignal observationSignal;

        [SerializeField]
        RewardSignal rewardSignal;

        [SerializeField]
        Muscles ragDollMuscles;

        [SerializeField]
        Transform motorsRoot;
        List<ArticulationBody> motors;

        DecisionRequester decisionRequester;

        float[] previousActions;
        public float[] PreviousActions { get => previousActions;}


        bool hasLazyInitialized;

        public event EventHandler<AgentEventArgs> onActionHandler;


        public float ObservationTimeDelta
        {
            get => Time.fixedDeltaTime * decisionRequester.DecisionPeriod;
        }

        public float ActionTimeDelta
        {
            get => decisionRequester.TakeActionsBetweenDecisions ? Time.fixedDeltaTime : Time.fixedDeltaTime * decisionRequester.DecisionPeriod;
        }

        public override void Initialize()
        {
            Assert.IsFalse(hasLazyInitialized);
            hasLazyInitialized = true;
            Time.fixedDeltaTime = fixedDeltaTime;

            decisionRequester = GetComponent<DecisionRequester>();
            ragDollMuscles = GetComponent<Muscles>();

            motors = motorsRoot.GetComponentsInChildren<ArticulationBody>()
                .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
                .Where(x => !x.isRoot)
                .Distinct()
                .ToList();

            previousActions = GetActionsFromRagdollState();

            ragDollMuscles.SetKinematicReference(kinematicRig);

            rewardSignal.OnAgentInitialize();
            observationSignal.OnAgentInitialize();

            hasLazyInitialized = true;
        }

        override public void CollectObservations(VectorSensor sensor)
        {
            Assert.IsTrue(hasLazyInitialized);

            kinematicRig.TrackKinematics();
            observationSignal.PopulateObservations(sensor);
        }
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            Assert.IsTrue(hasLazyInitialized);
            float[] vectorAction = actionBuffers.ContinuousActions.ToArray();
            vectorAction = SmoothActions(vectorAction);

            kinematicRig.TrackKinematics();

            int i = 0;
            foreach (var m in motors)
            {
                Vector3 targetNormalizedRotation = Vector3.zero;
                if (m.jointType != ArticulationJointType.SphericalJoint)
                    continue;
                if (m.twistLock == ArticulationDofLock.LimitedMotion)
                    targetNormalizedRotation.x = vectorAction[i++];
                if (m.swingYLock == ArticulationDofLock.LimitedMotion)
                    targetNormalizedRotation.y = vectorAction[i++];
                if (m.swingZLock == ArticulationDofLock.LimitedMotion)
                    targetNormalizedRotation.z = vectorAction[i++];

                ragDollMuscles.UpdateMotor(m, targetNormalizedRotation, ActionTimeDelta);
            }

            previousActions = vectorAction;

            float currentReward = rewardSignal.Reward;
            AddReward(currentReward);

            onActionHandler?.Invoke(this, new AgentEventArgs(vectorAction, currentReward));
        }
        public override void OnEpisodeBegin()
        {

            Vector3 resetVelocity = Vector3.zero;

            throw new NotImplementedException(); // Might need a separate class to handle setting up and resetting environment (i.e., the character)

            previousActions = GetActionsFromRagdollState();
        }

        float[] GetActionsFromRagdollState()
        {
            var vectorActions = new List<float>();
            foreach (var m in motors)
            {
                if (m.isRoot)
                    continue;
                int i = 0;
                if (m.jointType != ArticulationJointType.SphericalJoint)
                    continue;
                if (m.twistLock == ArticulationDofLock.LimitedMotion)
                {
                    var drive = m.xDrive;
                    var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                    var midpoint = drive.lowerLimit + scale;
                    var deg = m.jointPosition[i++] * Mathf.Rad2Deg;
                    var target = (deg - midpoint) / scale;
                    vectorActions.Add(target);
                }
                if (m.swingYLock == ArticulationDofLock.LimitedMotion)
                {
                    var drive = m.yDrive;
                    var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                    var midpoint = drive.lowerLimit + scale;
                    var deg = m.jointPosition[i++] * Mathf.Rad2Deg;
                    var target = (deg - midpoint) / scale;
                    vectorActions.Add(target);
                }
                if (m.swingZLock == ArticulationDofLock.LimitedMotion)
                {
                    var drive = m.zDrive;
                    var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                    var midpoint = drive.lowerLimit + scale;
                    var deg = m.jointPosition[i++] * Mathf.Rad2Deg;
                    var target = (deg - midpoint) / scale;
                    vectorActions.Add(target);
                }
            }
            return vectorActions.ToArray();
        }

        float[] SmoothActions(float[] vectorAction)
        {
            var smoothedActions = vectorAction
                .Zip(PreviousActions, (a, y) => actionSmoothingBeta * a + (1f - actionSmoothingBeta) * y)
                .ToArray();
            return smoothedActions;
        }
    }

    public interface IRememberPreviousActions
    {
        public float[] PreviousActions { get; }
    }

    public interface IEventsAgent
    {
        public event EventHandler<AgentEventArgs> onActionHandler;
    }

    public class AgentEventArgs: EventArgs
    {
        public float[] actions;
        public float reward;

        public AgentEventArgs(float[] actions, float reward)
        {
            this.actions = actions;
            this.reward = reward;
        }
    }

}