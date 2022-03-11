using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System.Linq;
using System;

public class MjMuscles : Muscles
{
    [SerializeField]
    Transform actuatorRoot;

    private IReadOnlyList<MjActuator> actuators;

    private void Awake()
    {
        actuators = actuatorRoot.GetComponentsInChildren<MjActuator>().ToList().AsReadOnly();
    }

    public override int ActionSpaceSize => actuatorRoot.GetComponentsInChildren<MjActuator>().ToList().Count;

    public override void ApplyActions(float[] actions, float actionTimeDelta)
    {
        foreach((var action, var actuator) in actions.Zip(actuators, Tuple.Create))
        {
            actuator.Control = action;
        }
    }

    public override float[] GetActionsFromRagdollState()
    {
        return actuators.Select(a => a.Control).ToArray();
    }
}
