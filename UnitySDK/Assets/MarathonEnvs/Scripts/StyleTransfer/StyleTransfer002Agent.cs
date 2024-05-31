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
using System.Web.UI.WebControls.Expressions;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;
using static TaskSwitcher;


public class StyleTransfer002Agent : Agent, IOnSensorCollision, IOnTerrainCollision
{
    public float FrameReward;
    public float AverageReward;
    public int TouchFrequence = 0;
    public int TaskState = 0;
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
    private HandTarget targetHand;
    public HandTarget targetHandDummy;
    public HandTarget targetHandRagdoll;
    public Transform agentHand;
    public Transform rightFoot;
    public Transform leftFoot;
    public FixedJoint joint;
    public bool handCollosion;
    private float distanceToTarget;
    private float faceDirectionReward;
    public float distanceReward;
    private float targetDistance = 0.7f;
    private float rewardScale50Percent = 0.5f;
    private float reward;
    private float handGripReward = 0f;
    public float timerGrip = 0f;
    public float gripDuration = 3f;
    public float gripForce = 50;
    public bool targetIsTouched;
    public SphereCollider areaTarget;
    public List<RotationRecord> rotationRecords;
    private int recordPoint = 0;
    private TaskSwitcher taskSwitcher;
    private float endEffectorVelocityWeight = 0.15f;
    public int breakPoint;

    public List<TargetTask> targetTasks = new List<TargetTask>();


    // Use this for initialization
    void Start()
    {
        targetHand = targetHandDummy;
        rotationRecords = new List<RotationRecord>();
        targetAttackTransformContainer = targetAttackTransform;
        _master = GetComponent<StyleTransfer002Master>();
        _decisionRequester = GetComponent<DecisionRequester>();
        var spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _localStyleAnimator = spawnableEnv.gameObject.GetComponentInChildren<StyleTransfer002Animator>();
        _styleAnimator = _localStyleAnimator.GetFirstOfThisAnim();
        _startCount++;

        taskSwitcher = new TaskSwitcher(targetTasks);

        // print("starararat");
    }

    private void FixedUpdate()
    {
        // var forwardNormalize = torsoBodyAncor.TransformDirection(-Vector3.right) * 5f;
        // Debug.DrawRay(torsoBodyAncor.position, forwardNormalize, Color.red);
        TaskState = taskSwitcher.taskState;
    }

    // Collect observations that are used by the Neural Network for training and inference.
    override public void CollectObservations(VectorSensor sensor)
    {
        if (!_hasLazyInitialized)
        {
            OnEpisodeBegin();
        }

        // leg distance observatoin
        var legDistance = Vector3.Distance(rightFoot.position, leftFoot.position);
        sensor.AddObservation(legDistance);//1
        sensor.AddObservation(rightFoot.position);//3
        sensor.AddObservation(leftFoot.position);//3

        // var targetHandDistanceFromOrigin = Vector3.Distance(ragdollManager.handDefaultPosition, targetHand.transform.position);
        // if (targetHandDistanceFromOrigin > 50.0f)
        // {
        //     targetHand.transform.position = ragdollManager.handDefaultPosition * 50;

        // }

        //direction and distance to target observation
        //var faceDirection = (rewardScale50Percent * AngleTowardTarget()) + (rewardScale50Percent * FaceDirection);
        distanceToTarget = Vector3.Distance(transform.position, targetAttackTransform.position);
        var agentHandtoTargetHandDistance = Vector3.Distance(targetHand.transform.position, agentHand.position);

        sensor.AddObservation(agentHandtoTargetHandDistance);//1
        // sensor.AddObservation(distanceToTarget);//1
        //target position
        // sensor.AddObservation(targetAttackTransform.position);//3
        sensor.AddObservation(targetHand.transform.position);//3
        //hand grip
        sensor.AddObservation(targetHand.isTouch);//1

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


        if (targetHand.isTouch)
        {
            TouchFrequence++;
            targetIsTouched = true;
            handGripReward = 0.20f;
            /** El Importante........................
            // it was...
            // using Old  way to get the specific amount of holding time reward. 
            // and then, I move to the generative way to generate adaptive reward based on 
            // how long the agent holding the target hand with exponential decay.
            //
            //Update...
            // using ForceMode seem imposible right now, because we have alot of moving part in agent properties
            // Then I change it to using Character joint component, 
            //then I change it again to FixedJoint component.
            ***/
        }
        else
        {
            timerGrip = 0f;
            handGripReward = 0f;
        }
        //Hand grip action and reward 
        SwitchTask();
        //Reward factor for agent to get closer to target
        var faceDirectionToTarget = AngleTowardTarget();
        var faceToFaceDirection = FaceDirection < 0 ? 0 : FaceDirection;//pluging to variable with minus prevention
        faceDirectionReward = 0.05f * ((rewardScale50Percent * faceDirectionToTarget) + (rewardScale50Percent * faceToFaceDirection));
        //Distance reward factor
        distanceReward = 0.20f * Mathf.Exp(-Mathf.Abs(DistanceToTarget - targetDistance));// target distance, zeroing distance before its actual zero
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

        var rotationReward = 0.10f * Mathf.Exp(-rotationDistance);
        var centerOfMassVelocityReward = 0.10f * Mathf.Exp(-centerOfMassvelocityDistance);
        var endEffectorReward = 0.10f * Mathf.Exp(-endEffectorDistance);
        var endEffectorVelocityReward = endEffectorVelocityWeight * Mathf.Exp(-endEffectorVelocityDistance);
        var jointAngularVelocityReward = 0.10f * Mathf.Exp(-jointAngularVelocityDistance);
        var jointAngularVelocityRewardWorld = 0.0f * Mathf.Exp(-jointAngularVelocityDistanceWorld);
        var centerMassReward = 0.05f * Mathf.Exp(-centerOfMassDistance);
        var angularMomentReward = 0.10f * Mathf.Exp(-angularMomentDistance);
        var sensorReward = 0.05f * Mathf.Exp(-sensorDistance);
        var jointsNotAtLimitReward = 0.05f * Mathf.Exp(-JointsAtLimit());
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
            distanceReward +//20% distance to target
                            // faceDirectionReward + //5% face direction
            handGripReward + // 30% grip opponent hand || PERCENTAGE DOES NOT MATTER
            endEffectorVelocityReward +//20% effector velocity  
            rotationReward +//10% joint rotation 
            centerOfMassVelocityReward +//10% center of mass velocity
            endEffectorReward +//10% effector
            jointAngularVelocityReward +//10% each joint Velocity 
            angularMomentReward +//15%
            centerMassReward + //5% 
                              sensorReward +
                              jointsNotAtLimitReward +
                              jointAngularVelocityRewardWorld;

        //     reward = (float)(distanceReward * 0.20 +  //Encourage approaching the ball
        // handGripReward * 0.40 +  //Prioritize gripping the ball
        // endEffectorVelocityReward * endEffectorVelocityWeight +  // Ensure velocity for throwing
        // rotationReward * 0.10 +  //Correct joint rotations while grabbing
        // centerOfMassVelocityReward * 0.10 + //Body motion for throwing
        // endEffectorReward * 0.10 +  //Use correct end effector for touching and throwing
        // jointAngularVelocityReward * 0.10 +  //Smooth and controlled joints
        // angularMomentReward * 0.10 +  //Correct rotational movement
        // centerMassReward * 0.05 +  //Maintain balance
        // sensorReward * 0.05 +  //Proper sensor usage
        // jointsNotAtLimitReward * 0.05);  //Prevent joints from reaching limits


        if (!_master.IgnorRewardUntilObservation)
        {
            AddReward(reward);
        }

        if (reward < 0.5)
        {
            EndEpisode();
        }

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
                EndEpisode();
                if (_master.StartAnimationIndex > 0)
                    _master.StartAnimationIndex--;
            }
            FrameReward = reward;
            var stepCount = StepCount > 0 ? StepCount : 1;
            AverageReward = GetCumulativeReward() / (float)stepCount;
        }
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

        SwitchTask();
        TouchFrequence = 0;
        if (TaskState == 0)
        {
            CatchTheBallInRandomPositionSphere();
        }
    }


    void HandGripWithInterpolation() //USING LINEAR INTERPOLATION AS THE HAND GRIP
    {
        var foreceNeeded = agentHand.position.magnitude * ragdollManager.handRb.mass * 10f;
        targetHand.getRigidBody.isKinematic = true;
        Vector3 newPosition = Vector3.Lerp(targetHand.transform.position, agentHand.position, foreceNeeded * Time.deltaTime);
        targetHand.transform.position = newPosition;
        gripForce = foreceNeeded;//debug
    }
    // USING FORCE AS THE HAND GRIP
    void HandGripWithForce()
    {
        timerGrip += Time.deltaTime;
        //  seconds = Mathf.FloorToInt(timerGrip % 60); get the exact seconds
        if (timerGrip < gripDuration)
        {
            var gripDurationDIfferent = gripDuration - timerGrip;
            handGripReward = 0.15f * Mathf.Exp(-Mathf.Abs(gripDurationDIfferent));
            // print("Start griping");
            // CURRENT..................TEMPORARY
            var handGripDirection = (agentHand.position - targetHand.transform.position).normalized * gripForce;//used to be with (vectorAction[0] + 150)
            targetHand.getRigidBody.AddForceAtPosition(handGripDirection, agentHand.position, ForceMode.Force);

            ragdollManager.gripForce = 50;
        }
        else
            timerGrip = 0f;

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

    public void SwitchTask()
    {
        switch (taskSwitcher.taskState)
        {
            case 0:
                endEffectorVelocityWeight = 0.15f;

                ragdollManager.gameObject.SetActive(false);
                areaTarget.gameObject.SetActive(true);
                targetHandDummy.gameObject.SetActive(true);
                targetHand = targetHandDummy;
                CatchTheBallInRandomPositionSphere();
                break;
            case 1:
                endEffectorVelocityWeight = 0.20f;
                targetHand = targetHandDummy;
                ThrowTheBall();
                break;
            case 2:
                endEffectorVelocityWeight = 0.15f;
                targetHand = targetHandRagdoll;
                ThrowTheRagdoll();
                break;
        }
    }

    /**
    transisi menuju state 3 harus bersih dari connected/assigned FIxedJoint remove saat transisi
    cek the ragdoll apakah memiliki fixedjoint meski tidak touched dengan hand dan tidak ragdolled 
    **/
    FixedJoint fixedJoint = null;
    public void ThrowTheRagdoll()
    {

        if (targetHand.isTouch && !ragdollManager.isRagdolled && fixedJoint == null)
        {//attach Fixed joint to target hand
            // print("!ragdoled, hasn't fixedJ");
            fixedJoint = AddFixedJoint();
        }

        if (ragdollManager.isRagdolled && targetHand.jointBroke && fixedJoint == null)
        {//reset when task complete
            // print("Ragdoled");
            handGripReward = 0.40f;
            ragdollManager.resetRadoll2();
            targetHand.jointBroke = false;
        }
        else if (!ragdollManager.isRagdolled && fixedJoint != null && _isDone)
        {//reset when episode end but won't able to lift the target to target velocity
            // print("!ragdoled, has fixedJ");
            targetHand.jointBroke = false;
            Destroy(fixedJoint);
            ragdollManager.resetRadoll2();
        }

        if (ragdollManager.isRagdolled && _isDone)
        {//reset when ragdolled and episode begin
            ragdollManager.resetRadoll2();
        }
    }

    public FixedJoint AddFixedJoint()
    {
        var target = targetHand.getRigidBody;
        var fixedJoint = targetHand.GetComponent<FixedJoint>();
        if (fixedJoint != null)
        {
            fixedJoint.connectedBody = agentHand.GetComponent<Rigidbody>();
            fixedJoint.enableCollision = false;
            fixedJoint.breakForce = breakPoint;
            fixedJoint.breakTorque = breakPoint;
            targetHand.jointBroke = false;
        }
        else
        {
            fixedJoint = targetHand.gameObject.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = agentHand.GetComponent<Rigidbody>();
            fixedJoint.enableCollision = false;
            fixedJoint.breakForce = breakPoint;
            fixedJoint.breakTorque = breakPoint;
            targetHand.jointBroke = false;
        }
        target.isKinematic = false;
        target.useGravity = true;
        return fixedJoint;
    }

    public void ThrowTheBall()  // USING CHARACTER JOINT AS THE CONNECTOR (HAND GRIP)
    {
        if (targetHand.isTouch)
        {
            fixedJoint = AddFixedJoint();
            targetHand.getCollider.isTrigger = false;
        }
        if (!targetHand.isTouch && targetHand.jointBroke && fixedJoint == null && !_isDone)//reset when able to perform the task
        {
            targetHand.jointBroke = false;
            handGripReward = 0.30f;
        }

        if ((_isDone && fixedJoint != null) || targetHand.isGround)
        {
            // print("DOne but fxjn still there");
            targetHand.jointBroke = false;
            Destroy(fixedJoint);
        }
        CatchTheBallInRandomPositionSphere();

        if (taskSwitcher.ReportTask(reward, TouchFrequence))
        {
            taskSwitcher.UpdateTask();
            ragdollManager.gameObject.SetActive(true);
            areaTarget.gameObject.SetActive(false);
            targetHandDummy.gameObject.SetActive(false);
        }
    }

    public void CatchTheBallInRandomPositionSphere()
    {
        if ((targetIsTouched && _isDone) || targetHand.isGround)
        {
            targetHand.getCollider.isTrigger = true;
            var coliderRadius = areaTarget.radius;
            var spawnPosition = new Vector3(Random.insideUnitSphere.x,
                                            Random.insideUnitSphere.y,
                                            Random.insideUnitSphere.z);
            spawnPosition = spawnPosition * coliderRadius;
            spawnPosition += areaTarget.transform.position;
            // spawnPosition = spawnPosition.normalized;
            targetHand.transform.position = spawnPosition;
            targetHand.getRigidBody.isKinematic = true;
            targetHand.getRigidBody.useGravity = false;
            targetIsTouched = false;
        }
        if (taskSwitcher.ReportTask(reward, TouchFrequence) && TaskState == 0)
        {
            taskSwitcher.UpdateTask();
        }
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
