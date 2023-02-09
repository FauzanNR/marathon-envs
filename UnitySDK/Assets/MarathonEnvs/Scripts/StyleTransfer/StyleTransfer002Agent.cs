﻿// Implmentation of an Agent. Agent reads observations relevant to the reinforcement
// learning task at hand, acts based on the observations, and receives a reward
// based on its performance. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using ManyWorlds;

public class StyleTransfer002Agent : Agent, IOnSensorCollision, IOnTerrainCollision
{

    public float FrameReward;
    public float AverageReward;
    public List<float> Rewards;
    public List<float> SensorIsInTouch;
    StyleTransfer002Master _master;
    public StyleTransfer002Animator _localStyleAnimator;
    StyleTransfer002Animator _styleAnimator;
    DecisionRequester _decisionRequester;

    List<GameObject> _sensors;

    public bool ShowMonitor = false;

    static int _startCount;
    static ScoreHistogramData _scoreHistogramData;
    int _totalAnimFrames;
    bool _ignorScoreForThisFrame;
    bool _isDone;
    bool _hasLazyInitialized;

    // Use this for initialization
    void Start()
    {
        _master = GetComponent<StyleTransfer002Master>();
        _decisionRequester = GetComponent<DecisionRequester>();
        var spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _localStyleAnimator = spawnableEnv.gameObject.GetComponentInChildren<StyleTransfer002Animator>();
        _styleAnimator = _localStyleAnimator.GetFirstOfThisAnim();
        _startCount++;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Collect observations that are used by the Neural Network for training and inference.
    override public void CollectObservations(VectorSensor sensor)
    {
        if (!_hasLazyInitialized)
        {
            OnEpisodeBegin();
        }

        sensor.AddObservation(_master.ObsPhase);
        var a = 0;
        foreach (var bodyPart in _master.BodyParts)
        {
            a += 1;
            print(" act " + a);
            sensor.AddObservation(bodyPart.ObsLocalPosition);
            sensor.AddObservation(bodyPart.ObsRotation);
            sensor.AddObservation(bodyPart.ObsRotationVelocity);
            sensor.AddObservation(bodyPart.ObsVelocity);
        }

        foreach (var muscle in _master.Muscles)
        {

            if (muscle.ConfigurableJoint.angularXMotion != ConfigurableJointMotion.Locked)
            {

                sensor.AddObservation(muscle.TargetNormalizedRotationX);
            }
            if (muscle.ConfigurableJoint.angularYMotion != ConfigurableJointMotion.Locked)
            {
                // a += 1;
                // print(" actiooon" + muscle.ConfigurableJoint.gameObject.name);
                sensor.AddObservation(muscle.TargetNormalizedRotationY);
            }
            if (muscle.ConfigurableJoint.angularZMotion != ConfigurableJointMotion.Locked)
            {

                sensor.AddObservation(muscle.TargetNormalizedRotationZ);
            }
        }

        sensor.AddObservation(_master.ObsCenterOfMass);
        sensor.AddObservation(_master.ObsVelocity);
        sensor.AddObservation(_master.ObsAngularMoment);
        sensor.AddObservation(SensorIsInTouch);
        print(SensorIsInTouch.Count);
    }

    // A method that applies the vectorAction to the muscles, and calculates the rewards. 
    public override void OnActionReceived(ActionBuffers actions)
    {
        float[] vectorAction = actions.ContinuousActions.Select(x => x).ToArray();
        if (!_hasLazyInitialized)
        {
            return;
        }
        _isDone = false;
        if (_styleAnimator == _localStyleAnimator)
            _styleAnimator.OnAgentAction();
        _master.OnAgentAction();
        int i = 0;
        foreach (var muscle in _master.Muscles)
        {
            if (muscle.ConfigurableJoint.angularXMotion != ConfigurableJointMotion.Locked)
            {
                muscle.TargetNormalizedRotationX = vectorAction[i++];
            }
            if (muscle.ConfigurableJoint.angularYMotion != ConfigurableJointMotion.Locked)
            {
                muscle.TargetNormalizedRotationY = vectorAction[i++];
            }
            if (muscle.ConfigurableJoint.angularZMotion != ConfigurableJointMotion.Locked)
            {
                muscle.TargetNormalizedRotationZ = vectorAction[i++];
            }
        }
        // the scaler factors are picked empirically by calculating the MaxRotationDistance, MaxVelocityDistance achieved for an untrained agent. 
        var rotationDistance = _master.RotationDistance / 16f;
        var centerOfMassvelocityDistance = _master.CenterOfMassVelocityDistance / 6f;
        var endEffectorDistance = _master.EndEffectorDistance / 1f;
        var endEffectorVelocityDistance = _master.EndEffectorVelocityDistance / 170f;
        var jointAngularVelocityDistance = _master.JointAngularVelocityDistance / 7000f;
        var jointAngularVelocityDistanceWorld = _master.JointAngularVelocityDistanceWorld / 7000f;
        var centerOfMassDistance = _master.CenterOfMassDistance / 0.3f;
        var angularMomentDistance = _master.AngularMomentDistance / 150.0f;
        var sensorDistance = _master.SensorDistance / 1f;

        var rotationReward = 0.35f * Mathf.Exp(-rotationDistance);
        var centerOfMassVelocityReward = 0.1f * Mathf.Exp(-centerOfMassvelocityDistance);
        var endEffectorReward = 0.15f * Mathf.Exp(-endEffectorDistance);
        var endEffectorVelocityReward = 0.1f * Mathf.Exp(-endEffectorVelocityDistance);
        var jointAngularVelocityReward = 0.1f * Mathf.Exp(-jointAngularVelocityDistance);
        // var jointAngularVelocityRewardWorld = 0.0f * Mathf.Exp(-jointAngularVelocityDistanceWorld);
        var centerMassReward = 0.05f * Mathf.Exp(-centerOfMassDistance);
        var angularMomentReward = 0.15f * Mathf.Exp(-angularMomentDistance);
        // var sensorReward = 0.0f * Mathf.Exp(-sensorDistance);
        // var jointsNotAtLimitReward = 0.0f * Mathf.Exp(-JointsAtLimit());

        // Debug.Log("---------------");
        // Debug.Log("rotation reward: " + rotationReward);
        // Debug.Log("endEffectorReward: " + endEffectorReward);
        // Debug.Log("endEffectorVelocityReward: " + endEffectorVelocityReward);
        // Debug.Log("jointAngularVelocityReward: " + jointAngularVelocityReward);
        // Debug.Log("jointAngularVelocityRewardWorld: " + jointAngularVelocityRewardWorld);
        // Debug.Log("centerMassReward: " + centerMassReward);
        // Debug.Log("centerMassVelocityReward: " + centerOfMassVelocityReward);
        // Debug.Log("angularMomentReward: " + angularMomentReward);
        // Debug.Log("sensorReward: " + sensorReward);
        // Debug.Log("joints not at limit rewards:" + jointsNotAtLimitReward);

        float reward = rotationReward +
            centerOfMassVelocityReward +
            endEffectorReward +
            endEffectorVelocityReward +
            jointAngularVelocityReward +
            // jointAngularVelocityRewardWorld +
            centerMassReward +
            angularMomentReward;
        // sensorReward +
        // jointsNotAtLimitReward;

        if (!_master.IgnorRewardUntilObservation)
            AddReward(reward);

        if (reward < 0.5)
            EndEpisode();

        if (!_isDone)
        {
            if (_master.IsDone())
            {
                EndEpisode();
                if (_master.StartAnimationIndex > 0)
                    _master.StartAnimationIndex--;
            }
        }
        // print("StepCount " + StepCount);
        FrameReward = reward;
        var stepCount = StepCount > 0 ? StepCount : 1;
        AverageReward = GetCumulativeReward() / (float)stepCount;
    }

    // A helper function that calculates a fraction of joints at their limit positions
    float JointsAtLimit(string[] ignorJoints = null)
    {
        int atLimitCount = 0;
        int totalJoints = 0;
        foreach (var muscle in _master.Muscles)
        {
            if (muscle.Parent == null)
                continue;

            var name = muscle.Name;
            if (ignorJoints != null && ignorJoints.Contains(name))
                continue;
            if (Mathf.Abs(muscle.TargetNormalizedRotationX) >= 1f)
                atLimitCount++;
            if (Mathf.Abs(muscle.TargetNormalizedRotationY) >= 1f)
                atLimitCount++;
            if (Mathf.Abs(muscle.TargetNormalizedRotationZ) >= 1f)
                atLimitCount++;
            totalJoints++;
        }
        float fractionOfJointsAtLimit = (float)atLimitCount / (float)totalJoints;
        return fractionOfJointsAtLimit;
    }

    // Sets reward 
    public void SetTotalAnimFrames(int totalAnimFrames)
    {
        _totalAnimFrames = totalAnimFrames;
        if (_scoreHistogramData == null)
        {
            var columns = _totalAnimFrames;
            if (_decisionRequester?.DecisionPeriod > 1)
                columns /= _decisionRequester.DecisionPeriod;
            _scoreHistogramData = new ScoreHistogramData(columns, 30);
        }
        Rewards = _scoreHistogramData.GetAverages().Select(x => (float)x).ToList();
    }

    // Resets the agent. Initialize the style animator and master if not initialized. 
    public override void OnEpisodeBegin()
    {
        if (!_hasLazyInitialized)
        {
            _master = GetComponent<StyleTransfer002Master>();
            _master.BodyConfig = MarathonManAgent.BodyConfig;
            _decisionRequester = GetComponent<DecisionRequester>();
            var spawnableEnv = GetComponentInParent<SpawnableEnv>();
            _localStyleAnimator = spawnableEnv.gameObject.GetComponentInChildren<StyleTransfer002Animator>();
            _styleAnimator = _localStyleAnimator.GetFirstOfThisAnim();
            _styleAnimator.BodyConfig = MarathonManAgent.BodyConfig;

            _styleAnimator.OnInitializeAgent(this.gameObject);
            _master.OnInitializeAgent();

            _hasLazyInitialized = true;
            _localStyleAnimator.DestoryIfNotFirstAnim();
        }
        _isDone = true;
        _ignorScoreForThisFrame = true;
        _master.ResetPhase();
        _sensors = GetComponentsInChildren<SensorBehavior>()
            .Select(x => x.gameObject)
            .ToList();
        SensorIsInTouch = Enumerable.Range(0, _sensors.Count).Select(x => 0f).ToList();
        if (_scoreHistogramData != null)
        {
            var column = _master.StartAnimationIndex;
            if (_decisionRequester?.DecisionPeriod > 1)
                column /= _decisionRequester.DecisionPeriod;
            if (_ignorScoreForThisFrame)
                _ignorScoreForThisFrame = false;
            else
                _scoreHistogramData.SetItem(column, AverageReward);
        }
        print("EPisode begin");
    }

    // A method called on terrain collision. Used for early stopping an episode
    // on specific objects' collision with terrain. 
    public virtual void OnTerrainCollision(GameObject other, GameObject terrain)
    {
        if (string.Compare(terrain.name, "Terrain", true) != 0)
            return;
        if (!_styleAnimator.AnimationStepsReady)
            return;
        var bodyPart = _master.BodyParts.FirstOrDefault(x => x.Transform.gameObject == other);
        if (bodyPart == null)
            return;
        switch (bodyPart.Group)
        {
            case BodyHelper002.BodyPartGroup.None:
            case BodyHelper002.BodyPartGroup.Foot:
            case BodyHelper002.BodyPartGroup.LegUpper:
            case BodyHelper002.BodyPartGroup.LegLower:
            case BodyHelper002.BodyPartGroup.Hand:
            case BodyHelper002.BodyPartGroup.ArmLower:
            case BodyHelper002.BodyPartGroup.ArmUpper:
                break;
            default:
                EndEpisode();
                break;
        }
    }

    // Sets the a flag in Sensors In Touch array when an object enters collision with terrain
    public void OnSensorCollisionEnter(Collider sensorCollider, GameObject other)
    {
        if (string.Compare(other.name, "Terrain", true) != 0)
            return;
        if (_sensors == null || _sensors.Count == 0)
            return;
        var sensor = _sensors
            .FirstOrDefault(x => x == sensorCollider.gameObject);
        if (sensor != null)
        {
            var idx = _sensors.IndexOf(sensor);
            SensorIsInTouch[idx] = 1f;
        }
    }

    // Sets the a flag in Sensors In Touch array when an object stops colliding with terrain
    public void OnSensorCollisionExit(Collider sensorCollider, GameObject other)
    {
        if (string.Compare(other.gameObject.name, "Terrain", true) != 0)
            return;
        if (_sensors == null || _sensors.Count == 0)
            return;
        var sensor = _sensors
            .FirstOrDefault(x => x == sensorCollider.gameObject);
        if (sensor != null)
        {
            var idx = _sensors.IndexOf(sensor);
            SensorIsInTouch[idx] = 0f;
        }
    }

}
