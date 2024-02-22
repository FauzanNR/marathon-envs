// Implmentation of an Agent. Agent reads observations relevant to the reinforcement
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
using System;
using UnityEngine.Animations;
using System.Windows.Forms;
using UnityEditor;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.OleDb;


public class StyleTransfer002Agent : Agent, IOnSensorCollision, IOnTerrainCollision
{
    public float FrameReward;
    public float AverageReward;
    public List<float> Rewards;
    public List<float> SensorIsInTouch;
    private Transform targetAttackTransformContainer;
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


    public Transform torsoBodyAncor;
    public Transform targetAttackTransform;

    public RagdollManager ragdollManager;
    public HandTarget targetHand;
    public Transform agentHand;
    public Transform rightFoot;
    public Transform leftFoot;

    public bool handCollosion;
    private float distanceToTarget;
    private float faceDirectionReward;
    public float distanceReward;
    private float targetDistance = 1.02f;
    private float rewardScale50Percent = 0.5f;
    private float reward;
    private float handGripReward = 0f;
    public float[] actionsValue;
    public float timerGrip = 0f;
    public float gripDuration = 3f;
    public float gripForce;
    public bool isHandGrip;

    public List<RotationRecord> rotationRecords;
    private int recordPoint = 0;


    // Use this for initialization
    void Start()
    {
        rotationRecords = new List<RotationRecord>();
        targetAttackTransformContainer = targetAttackTransform;
        _master = GetComponent<StyleTransfer002Master>();
        _decisionRequester = GetComponent<DecisionRequester>();
        var spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _localStyleAnimator = spawnableEnv.gameObject.GetComponentInChildren<StyleTransfer002Animator>();
        _styleAnimator = _localStyleAnimator.GetFirstOfThisAnim();
        _startCount++;
        // print("starararat");
    }

    // Update is called once per frame
    void Update()
    {
        var forwardNormalize = torsoBodyAncor.TransformDirection(-Vector3.right) * 5f;
        Debug.DrawRay(torsoBodyAncor.position, forwardNormalize, Color.red);
    }

    // Collect observations that are used by the Neural Network for training and inference.
    override public void CollectObservations(VectorSensor sensor)
    {
        if (!_hasLazyInitialized)
        {
            OnEpisodeBegin();
        }
        //direction and distance to target observation
        //var faceDirection = (rewardScale50Percent * AngleTowardTarget()) + (rewardScale50Percent * FaceDirection);
        distanceToTarget = Vector3.Distance(transform.position, targetAttackTransform.position);
        var agentHandtoTargetHandDistance = Vector3.Distance(targetHand.transform.position, agentHand.position);

        sensor.AddObservation(agentHandtoTargetHandDistance);//1
        sensor.AddObservation(distanceToTarget);//1
        // leg distance observatoin
        var legDistance = Vector3.Distance(rightFoot.position, leftFoot.position);
        sensor.AddObservation(legDistance);//1
        sensor.AddObservation(rightFoot.position);//3
        sensor.AddObservation(leftFoot.position);//3

        //target position
        sensor.AddObservation(targetAttackTransform.position);//3
        sensor.AddObservation(targetHand.transform.position);//3

        //hand grip
        sensor.AddObservation(targetHand.isTouch);

        //animation phase
        sensor.AddObservation(_master.ObsPhase);//1
        // print("direction+distances+animation phase " + GetObservations().Count);

        var a = 0;
        foreach (var bodyPart in _master.BodyParts)
        {
            a += 1;
            sensor.AddObservation(bodyPart.ObsLocalPosition);//3 = 51
            sensor.AddObservation(bodyPart.ObsRotation);//4 = 68
            sensor.AddObservation(bodyPart.ObsRotationVelocity);//3 = 51
            sensor.AddObservation(bodyPart.ObsVelocity);//3 = 51
        }// times by 17 body part = 221
        // print("body part " + GetObservations().Count);

        foreach (var muscle in _master.Muscles)
        {
            if (muscle.ConfigurableJoint.angularXMotion != ConfigurableJointMotion.Locked)
            {

                sensor.AddObservation(muscle.TargetNormalizedRotationX);//1
            }
            if (muscle.ConfigurableJoint.angularYMotion != ConfigurableJointMotion.Locked)
            {
                // a += 1;
                // print(" actiooon" + muscle.ConfigurableJoint.gameObject.name);
                sensor.AddObservation(muscle.TargetNormalizedRotationY);//1
            }
            if (muscle.ConfigurableJoint.angularZMotion != ConfigurableJointMotion.Locked)
            {
                sensor.AddObservation(muscle.TargetNormalizedRotationZ);//1
            }
        }//21, the muscle is not the same like action value, but most of muscle are using action value except one, the handgrip.

        // print("body part muscles " + GetObservations().Count);
        sensor.AddObservation(_master.ObsCenterOfMass);//3
        sensor.AddObservation(_master.ObsVelocity);//3
        sensor.AddObservation(_master.ObsAngularMoment);//3
        sensor.AddObservation(SensorIsInTouch);//8

        // print("mass sensor, target " + GetObservations().Count);
        // data to text to be commented for a while
        // var rotationData = new RotationRecord(recordPoint, _master.AngularMomentDistance);
        // rotationRecords.Add(rotationData);
        // recordPoint++;
        // CreateDataRotation(rotationRecords, UnityEngine.Application.dataPath + "/Results/Rotation_record/data_rotation.txt");


        // data to tensorboard. Record the rotation differences between simulation and reference
        var statsRecorder = Academy.Instance.StatsRecorder;
        statsRecorder.Add("Rotation Different", _master.AngularMomentDistance);

    }


    //Write data rotation
    void CreateDataRotation(List<RotationRecord> rotationRecords, string dataPath)
    {
        try
        {
            using (StreamWriter dataWriter = new StreamWriter(dataPath))
            {
                foreach (var data in rotationRecords)
                {
                    dataWriter.WriteLine($"{data.recordPoints}, {data.rotationRecord}");
                }
            }
        }
        catch (System.Exception error)
        {
            Debug.Log("Errorr to write file " + error.Message);
        }
    }


    //Method to calculate face direction
    float AngleTowardTarget()
    {
        //Get target direction
        var directionToTarget = targetAttackTransform.position - torsoBodyAncor.position;
        // Calculate the angular difference in degrees
        var angleDifference = Vector3.Angle(-transform.right, directionToTarget);
        return Mathf.Exp(-angleDifference / 45f);
    }

    //DOT Product to observe the agent-target direction in space. We use agent(torsoBodyAncor) -Right Direction(equal to Left direction) is because the agent orientation is not common
    float FaceDirection => Vector3.Dot(-targetAttackTransform.forward, -torsoBodyAncor.right);
    float DistanceToTarget => Vector3.Distance(targetAttackTransform.position, torsoBodyAncor.position);

    // A method that applies the vectorAction to the muscles, and calculates the rewards. 
    public override void OnActionReceived(ActionBuffers actions)
    {
        float[] vectorAction = actions.ContinuousActions.Select(x => x).ToArray();

        // var a = 0;
        // foreach(var action in vectorAction){
        //     print("an action with value "+ a +" "+action);
        //     a++;
        // }

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
                // print(i + " "+muscle.Name);
            }
            if (muscle.ConfigurableJoint.angularYMotion != ConfigurableJointMotion.Locked)
            {
                muscle.TargetNormalizedRotationY = vectorAction[i++];
                //   print(i + " "+muscle.Name);
            }
            if (muscle.ConfigurableJoint.angularZMotion != ConfigurableJointMotion.Locked)
            {
                muscle.TargetNormalizedRotationZ = vectorAction[i++];
                //   print(i + " "+muscle.Name);
            }
        }


        //Hand grip action and reward 
        // if (DifferenceReward > 0.13f)
        // {
        actionsValue = vectorAction;
        isHandGrip = targetHand.isTouch;
        if (targetHand.isTouch)
        {
            // handGripReward = 0.06f;
            timerGrip += Time.deltaTime;
            // seconds = Mathf.FloorToInt(timerGrip % 60); get the exact seconds
            // if (timerGrip < gripDuration)
            // {
            var gripDurationDIfferent = gripDuration - timerGrip;
            handGripReward = 0.2f * Mathf.Exp(-Mathf.Abs(gripDurationDIfferent));
            // print("Start griping");
            var handGripDirection = (agentHand.position - targetHand.transform.position).normalized * vectorAction[0];
            targetHand.getRigidBody.AddForceAtPosition(handGripDirection, agentHand.position, ForceMode.Force);
            ragdollManager.gripForce = vectorAction[0];
            // if (timerGrip >= gripDuration)
            //     handGripReward = 0.1f;
            // }
            // else
            //     timerGrip = 0f;

            // it was...
            // using Old  way to get the specific amount of holding time reward. 
            // and then, I move to the generative way to generate adaptive reward based on 
            // how long the agent holding the target hand with exponential decay.
        }
        else
        {
            timerGrip = 0f;
            handGripReward = 0f;
        }
        // }

        //Reward factor for agent to get closer to target
        var faceDirectionToTarget = AngleTowardTarget();
        var faceToFaceDirection = FaceDirection < 0 ? 0 : FaceDirection;//pluging to variable with minus prevention
        faceDirectionReward = 0.05f * ((rewardScale50Percent * faceDirectionToTarget) + (rewardScale50Percent * faceToFaceDirection));
        //Distance reward factor
        distanceReward = 0.05f * Mathf.Exp(-Mathf.Abs(DistanceToTarget - targetDistance));// target distance, zeroing distance before its actual zero
        var DifferenceReward = faceDirectionReward + distanceReward;
        // print("Distance debug: " + DifferenceReward);
        // print("faceDirectionReward: " + faceDirectionReward);
        // print("distanceReward: " + distanceReward);

        // print("Total action " + vectorAction.Length);

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
        var centerOfMassVelocityReward = 0.05f * Mathf.Exp(-centerOfMassvelocityDistance);
        var endEffectorReward = 0.05f * Mathf.Exp(-endEffectorDistance);
        var endEffectorVelocityReward = 0.05f * Mathf.Exp(-endEffectorVelocityDistance);
        var jointAngularVelocityReward = 0.1f * Mathf.Exp(-jointAngularVelocityDistance);
        var jointAngularVelocityRewardWorld = 0.0f * Mathf.Exp(-jointAngularVelocityDistanceWorld);
        var centerMassReward = 0.05f * Mathf.Exp(-centerOfMassDistance);
        var angularMomentReward = 0.15f * Mathf.Exp(-angularMomentDistance);
        var sensorReward = 0.0f * Mathf.Exp(-sensorDistance);
        var jointsNotAtLimitReward = 0.0f * Mathf.Exp(-JointsAtLimit());
        #region 
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
        #endregion

        //force the agent to align with the opponent body and give the reward down the training
        // if (DifferenceReward > 0.425f){

        //tune the reward amount above
        reward =
            // distanceReward +//5% distance to target
            // faceDirectionReward + //5% face direction
            handGripReward + // 20% grip opponent hand
            endEffectorVelocityReward +//5% effector velocity  
            rotationReward +//35% joint rotation 
            centerOfMassVelocityReward +//10% center of mass velocity
            endEffectorReward +//5% effector
            jointAngularVelocityReward +//10% each joint Velocity 
            angularMomentReward +//15%
            centerMassReward + //5% 
                              sensorReward +
                              jointsNotAtLimitReward +
                              jointAngularVelocityRewardWorld;
        // if (!_master.IgnorRewardUntilObservation)
        // {
        //     AddReward(reward);
        // }
        // else
        // {
        //     AddReward(-0.1f);
        // }

        // if (reward < 0.15)
        // {
        //     AddReward(0);
        //     EndEpisode();
        //     // print("End of bellow reward standar");
        // }

        if (!_master.IgnorRewardUntilObservation)
            AddReward(reward);

        if (reward < 0.4)
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

        if (!_isDone)
        {
            if (_master.IsDone())
            {
                // print("Master Done");
                EndEpisode();
                if (_master.StartAnimationIndex > 0)
                    _master.StartAnimationIndex--;
            }
            FrameReward = reward;
            var stepCount = StepCount > 0 ? StepCount : 1;
            AverageReward = GetCumulativeReward() / (float)stepCount;
        }
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

        //random spawn agent position
        // targetAttackTransform = targetAttackTransformContainer;
        // var randomSphere = UnityEngine.Random.insideUnitSphere.normalized;
        // var randomDirection = new Vector3(randomSphere.x, 0, randomSphere.z).normalized;
        // var randomSpawn = targetAttackTransform.position + randomDirection * 3f;
        // transform.position = randomSpawn;


        //set target ragdoll to kinematic and default position
        ragdollManager.spineRb.useGravity = false;
        ragdollManager.spineRb.isKinematic = true;
        ragdollManager.spineRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        ragdollManager.hipsRbTr.position = ragdollManager.defaultPosition;
        ragdollManager.hipsRbTr.rotation = ragdollManager.defaultRotation;



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
        // print("Body part " + other.name);
        switch (bodyPart.Group)
        {
            case BodyHelper002.BodyPartGroup.None:
            case BodyHelper002.BodyPartGroup.Foot:
                // case BodyHelper002.BodyPartGroup.LegUpper:
                // case BodyHelper002.BodyPartGroup.LegLower:
                // case BodyHelper002.BodyPartGroup.Hand:
                // case BodyHelper002.BodyPartGroup.ArmLower:
                // case BodyHelper002.BodyPartGroup.ArmUpper:
                break;
            default:
                {
                    // print("part end " + bodyPart.Group);
                    EndEpisode();
                }
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
            // print("Sensorrr " + sensor.name);
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
