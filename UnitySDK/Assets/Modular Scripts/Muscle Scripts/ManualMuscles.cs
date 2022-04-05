using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotorUpdate;
using Mujoco;
using System.Linq;
using System;

public class ManualMuscles : Muscles
{
    [SerializeField]
    private List<GameObject> actuatorGameObjects;

    [SerializeField]
    List<float> targets;

    [SerializeField]
    MotorUpdateRule updateRule;

    private List<IState> actuatorStates;
    private List<MjActuator> actuators;

    [DebugGUIGraph]
    public float acc;

    public override int ActionSpaceSize => actuators.Count;


    public override void ApplyActions(float[] actions, float actionTimeDelta)
    {
        foreach((var motor, var action) in actuators.Zip(actions, Tuple.Create))
        {
            motor.Control = action;
        }
    }

    public override float[] GetActionsFromState()
    {
        return actuators.Select(a => a.Control).ToArray();
    }

    private void Awake()
    {
        actuators = actuatorGameObjects.Select(ago => ago.GetComponent<MjActuator>()).ToList();
        actuatorStates = actuators.Select(a => (IState)new MjActuatorState(a)).ToList();

    }

    private void Start()
    {

    }

    private unsafe void Sync()
    {
        foreach(var a in actuators)
        {
            a.OnSyncState(MjScene.Instance.Data);
            a.Joint.OnSyncState(MjScene.Instance.Data);
        }
    }

    private unsafe void FixedUpdate()
    {
        if (MjScene.Instance.Data == null) return;

        MjScene.Instance.SyncUnityToMjState();
        Debug.Log($"nv: {MjScene.Instance.Model->nv}\nJoint mjId:{actuators[0].Joint.MujocoId}\nJoint QposADdress: {actuators[0].Joint.QposAddress}\nJoint DofAddress: {actuators[0].Joint.GetDoFAddress()}\nAct mjId:{actuators[0].MujocoId}");
        Debug.Log(string.Join(", ", Enumerable.Range(0, MjScene.Instance.Model->nv * 20).Select(x => (MjScene.Instance.Data->qacc[x]))));
        //MujocoLib.mj_kinematics(MjScene.Instance.Model, MjScene.Instance.Data);
        //float[] curActions = actuators.Zip(actuatorStates, Tuple.Create).Zip(targets, (a, t) => updateRule.GetTorque(a.Item2, new StaticState(a.Item1.Deg2Length(t), 0f, 0f))).ToArray();
        //ApplyActions(curActions, Time.fixedDeltaTime);
        MjScene.Instance.SyncUnityToMjState();
        //acc = actuators[0].GetAcceleration();
    }
}
