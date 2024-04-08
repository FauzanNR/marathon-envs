using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BoxAgent : Agent
{
    public RagdollManager ragdollManager;
    public HandTarget handTarget;
    public Transform agentHand;
    public Transform agentHandPointer;
    public float targetDistance = 1.02f;
    public Transform target;
    public float gripDuration = 2f;
    public float gripForce = 15;
    private float timerGrip = 0f;
    float rewardScale50Perent = 0.5f;
    Rigidbody rBody;

    public CharacterJoint joint;

    private void Update()
    {
        var forward = transform.TransformDirection(Vector3.forward) * 5f;
        Debug.DrawRay(transform.position, forward, Color.red);

        if (handTarget.isTouch)
        {
            // if (agentHand.GetComponents<CharacterJoint>().Count() <= 1)
            // {
            //     print("is touch");
            //     joint = agentHandPointer.gameObject.AddComponent(typeof(CharacterJoint)) as CharacterJoint;
            //     // joint = agentHand.GetComponents<CharacterJoint>().Last();
            //     joint.connectedMassScale = 10000;
            //     joint.connectedBody = handTarget.getRigidBody;
            // }

            // timerGrip += Time.fixedDeltaTime;
            // if (timerGrip < gripDuration)
            // {
            // var gripDirection = (agentHand.position - handTarget.transform.position).normalized * gripForce;
            // handTarget.getRigidBody.AddForceAtPosition(gripDirection, agentHand.position, ForceMode.Force);
            // }
            // else
            // {
            //     timerGrip = 0f;
            // }
            handTarget.getRigidBody.isKinematic = true;

        }
        var foreceNeeded = agentHand.position.magnitude * ragdollManager.handRb.mass * 3f * 10f;
        Vector3 newPosition = Vector3.Lerp(handTarget.transform.position, agentHand.position, gripForce * Time.deltaTime);
        // gripForce = agentHand.position.magnitude;
        // Update the object's position
        handTarget.transform.position = newPosition;
        handTarget.getRigidBody.isKinematic = false;
    }

    public override void Initialize()
    {
        Debug.Log("Initialize");
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // transform.position = new Vector3(6.3f, 1.3f, 22.4f);
        // print("New epsd");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector3)transform.localPosition);
        sensor.AddObservation((Vector3)target.localPosition);
        sensor.AddObservation(DistanceToTarget);
        sensor.AddObservation(FaceDirection);

    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionZero = actions.ContinuousActions[0];
        var actionOne = actions.ContinuousActions[1];
        var actionTwo = actions.ContinuousActions[2];
        var actionForce = new Vector3(actionZero, actionOne, actionTwo);
        // rBody.AddForce(actionForce.normalized, ForceMode.VelocityChange);
        transform.localPosition += new Vector3(actionZero, actionOne, actionTwo);



        var faceDirectionToTarget = FaceDirectionTowardTarget();
        var faceToFaceDirection = FaceDirection;
        // var faceDirectionReward = rewardScale50Perent * (faceDirectionToTarget / faceToFaceDirection);
        var faceDirectionReward = 0.10f * ((rewardScale50Perent * faceDirectionToTarget) + (rewardScale50Perent * faceToFaceDirection));
        // var distanceReward = rewardScale50Perent * Mathf.Clamp(Mathf.Exp(-Mathf.Abs(DistanceToTarget - targetDistance)), 0.0f, 1.0f);
        var distanceReward = 0.05f * Mathf.Exp(-Mathf.Abs(DistanceToTarget - targetDistance));
        var rewardo = faceDirectionReward + distanceReward;

        AddReward(rewardo);
        if (rewardo < 0.4f)
            EndEpisode();

        // print("faceDirectionToTarget: " + faceDirectionToTarget);
        // print("faceToFaceDirection: " + faceToFaceDirection);
        // print("faceDirectionReward: " + faceDirectionReward);
        // print("distanceReward: " + distanceReward);
        // print("rewardo: " + rewardo);
    }

    float FaceDirectionTowardTarget()
    {
        var directionToTarget = target.localPosition - transform.localPosition;
        var angleDifference = Vector3.Angle(transform.forward, directionToTarget);
        return Mathf.Exp(-angleDifference / 45f);
    }
    float FaceDirection => Vector3.Dot(-target.forward, transform.forward);
    float DistanceToTarget => Vector3.Distance(target.localPosition, transform.localPosition);
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actionOutInput = actionsOut.ContinuousActions;
        actionOutInput[0] = Input.GetAxisRaw("Horizontal");
        actionOutInput[2] = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Space)) { actionOutInput[1] = 3f; print("space"); }

    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == target.gameObject.name)
        {
            AddReward(1);
            EndEpisode();
        }
        else if (other.gameObject.name == target.gameObject.name)
        {
            AddReward(-1);
            EndEpisode();
        }
    }
    #region unusedFunction

    public override string ToString()
    {
        return base.ToString();
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        base.WriteDiscreteActionMask(actionMask);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
    #endregion
}
