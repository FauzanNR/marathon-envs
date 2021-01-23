using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Assertions;
using ManyWorlds;

public class DReConRewards : MonoBehaviour
{
    [Header("Reward")]
    public float SumOfSubRewards;
    public float Reward;

    [Header("Position Reward")]
    public float SumOfDistances;
    public float SumOfSqrDistances;
    public float PositionReward;

    [Header("Velocity Reward")]
    public float PointsVelocityDifferenceSquared;
    public float PointsVelocityReward;

    [Header("Local Pose Reward")]
    public List<float> RotationDifferences;
    public float SumOfRotationDifferences;
    public float SumOfRotationSqrDifferences;
    public float LocalPoseReward;

    [Header("Center of Mass Velocity Reward")]
    public Vector3 MocapCOMVelocity;
    public Vector3 RagDollCOMVelocity;

    public float COMVelocityDifference;
    public float ComVelocityReward;

    [Header("Center of Mass Direction Reward")]
    public float ComDirectionDistance;
    public float ComDirectionReward;

    [Header("Center of Mass Position Reward")]
    public float ComDistance;
    public float DistanceFactor;
    public float ComPositionReward;

    // [Header("Direction Factor")]
    // public float DirectionDistance;
    // public float DirectionFactor;


    [Header("Misc")]
    public float HeadHeightDistance;

    [Header("Gizmos")]
    public int ObjectForPointDistancesGizmo;

    SpawnableEnv _spawnableEnv;
    MocapControllerArtanim _mocap;
    GameObject _ragDoll;
    InputController _inputController;

    internal DReConRewardStats _mocapBodyStats;
    internal DReConRewardStats _ragDollBodyStats;

    // List<ArticulationBody> _mocapBodyParts;
    // List<ArticulationBody> _ragDollBodyParts;
    Transform _mocapHead;
    Transform _ragDollHead;

    bool _hasLazyInitialized;

    public void OnAgentInitialize()
    {
        Assert.IsFalse(_hasLazyInitialized);

        _hasLazyInitialized = true;

        _spawnableEnv = GetComponentInParent<SpawnableEnv>();
        Assert.IsNotNull(_spawnableEnv);

        _mocap = _spawnableEnv.GetComponentInChildren<MocapControllerArtanim>();

        _ragDoll = _spawnableEnv.GetComponentInChildren<RagDollAgent>().gameObject;
        Assert.IsNotNull(_mocap);
        Assert.IsNotNull(_ragDoll);
        _inputController = _spawnableEnv.GetComponentInChildren<InputController>();
        // _mocapBodyParts = _mocap.GetComponentsInChildren<ArticulationBody>().ToList();
        // _ragDollBodyParts = _ragDoll.GetComponentsInChildren<ArticulationBody>().ToList();
        // Assert.AreEqual(_mocapBodyParts.Count, _ragDollBodyParts.Count);
        _mocapHead = _mocap
            .GetComponentsInChildren<Transform>()
            .First(x => x.name == "head");
        _ragDollHead = _ragDoll
            .GetComponentsInChildren<Transform>()
            .First(x => x.name == "head");
        _mocapBodyStats = new GameObject("MocapDReConRewardStats").AddComponent<DReConRewardStats>();

        _mocapBodyStats.ObjectToTrack = _mocap;

        _mocapBodyStats.transform.SetParent(_spawnableEnv.transform);
        _mocapBodyStats.OnAgentInitialize(_mocapBodyStats.ObjectToTrack.transform);

        _ragDollBodyStats= new GameObject("RagDollDReConRewardStats").AddComponent<DReConRewardStats>();
        _ragDollBodyStats.ObjectToTrack = this;
        _ragDollBodyStats.transform.SetParent(_spawnableEnv.transform);
        _ragDollBodyStats.OnAgentInitialize(transform, _mocapBodyStats);      

        _mocapBodyStats.AssertIsCompatible(_ragDollBodyStats);    
    }

    // Update is called once per frame
    public void OnStep(float timeDelta)
    {
        _mocapBodyStats.SetStatusForStep(timeDelta);
        _ragDollBodyStats.SetStatusForStep(timeDelta);

        // position reward
        List<float> distances = _mocapBodyStats.GetPointDistancesFrom(_ragDollBodyStats);
        PositionReward = -7.37f/(distances.Count/6f);
        List<float> sqrDistances = distances.Select(x=> x*x).ToList();
        SumOfDistances = distances.Sum();
        SumOfSqrDistances = sqrDistances.Sum();
        PositionReward *= SumOfSqrDistances;
        PositionReward = Mathf.Exp(PositionReward);

        // center of mass velocity reward
        MocapCOMVelocity = _mocapBodyStats.CenterOfMassVelocity;
        RagDollCOMVelocity = _ragDollBodyStats.CenterOfMassVelocity;
        COMVelocityDifference = (MocapCOMVelocity-RagDollCOMVelocity).magnitude;
        ComVelocityReward = -Mathf.Pow(COMVelocityDifference,2);
        ComVelocityReward = Mathf.Exp(ComVelocityReward);

        // points velocity
        List<float> velocityDistances = _mocapBodyStats.GetPointVelocityDistancesFrom(_ragDollBodyStats);
        List<float> sqrVelocityDistances = velocityDistances.Select(x=> x*x).ToList();
        PointsVelocityDifferenceSquared = sqrVelocityDistances.Sum();
        PointsVelocityReward = (-1f/_mocapBodyStats.PointVelocity.Length) * PointsVelocityDifferenceSquared;
        PointsVelocityReward = Mathf.Exp(PointsVelocityReward);

        // local pose reward
        if (RotationDifferences == null || RotationDifferences.Count < _mocapBodyStats.Rotations.Count)
            RotationDifferences = Enumerable.Range(0,_mocapBodyStats.Rotations.Count)
            .Select(x=>0f)
            .ToList();
        SumOfRotationDifferences = 0f;
        SumOfRotationSqrDifferences = 0f;
        for (int i = 0; i < _mocapBodyStats.Rotations.Count; i++)
        { 
            var angle = Quaternion.Angle(_mocapBodyStats.Rotations[i], _ragDollBodyStats.Rotations[i]);
            Assert.IsTrue(angle <= 180f);
            angle = DReConObservationStats.NormalizedAngle(angle);
            var sqrAngle = angle * angle;
            RotationDifferences[i] = angle;
            SumOfRotationDifferences += angle;
            SumOfRotationSqrDifferences += sqrAngle;
        }
        LocalPoseReward = -6.5f/RotationDifferences.Count;
        LocalPoseReward *= SumOfRotationSqrDifferences;
        LocalPoseReward = Mathf.Exp(LocalPoseReward);

        // distance factor
        ComDistance = (_mocapBodyStats.transform.position - _ragDollBodyStats.transform.position).magnitude;
        DistanceFactor = Mathf.Pow(ComDistance,2);
        DistanceFactor = 1.4f*DistanceFactor;
        DistanceFactor = 1.01f-DistanceFactor;
        DistanceFactor = Mathf.Clamp(DistanceFactor, 0f, 1f);
        // ComPositionReward = Mathf.Exp(-2f * Mathf.Pow(ComDistance,2));
        ComPositionReward = 1.3f*ComDistance;
        ComPositionReward = 1.01f-ComPositionReward;
        ComPositionReward = Mathf.Clamp(ComPositionReward, 0f, 1f);
        ComPositionReward = Mathf.Pow(ComPositionReward,2);

        // center of mass direction reward
        ComDirectionDistance = Vector3.Dot( 
            _mocapBodyStats.transform.forward, 
            _ragDollBodyStats.transform.forward);
        ComDirectionReward = (ComDirectionDistance + 1f)/2f;
        ComDirectionReward = Mathf.Pow(ComDirectionReward,2);

        // misc
        HeadHeightDistance = (_mocapHead.position.y - _ragDollHead.position.y);
        HeadHeightDistance = Mathf.Abs(HeadHeightDistance);

        // reward
        SumOfSubRewards = ComPositionReward+ComVelocityReward+ComDirectionReward+PositionReward+LocalPoseReward+PointsVelocityReward;
        Reward = (ComPositionReward * 0.3f) +
                    (ComVelocityReward * 0.1f) +
                    // (ComDirectionReward * 0.2f) +
                    (PositionReward * 0.1f) +
                    (LocalPoseReward * 0.4f) +
                    (PointsVelocityReward * 0.1f);
        Reward *= ComDirectionReward;
    }
    public void OnReset()
    {
        Assert.IsTrue(_hasLazyInitialized);
       
        _mocapBodyStats.OnReset();
        _ragDollBodyStats.OnReset();
        _ragDollBodyStats.transform.position = _mocapBodyStats.transform.position;
        _ragDollBodyStats.transform.rotation = _mocapBodyStats.transform.rotation;
    }
    void OnDrawGizmos()
    {
        if (_ragDollBodyStats == null)
            return;
        var max = (_ragDollBodyStats.Points.Length/6)-1;
        ObjectForPointDistancesGizmo = Mathf.Clamp(ObjectForPointDistancesGizmo, -1, max);
        // _mocapBodyStats.DrawPointDistancesFrom(_ragDollBodyStats, ObjectForPointDistancesGizmo);
        _ragDollBodyStats.DrawPointDistancesFrom(_mocapBodyStats, ObjectForPointDistancesGizmo);
    }
}
