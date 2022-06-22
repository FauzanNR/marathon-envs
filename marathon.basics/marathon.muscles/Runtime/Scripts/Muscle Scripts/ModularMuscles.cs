using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotorUpdate;

using Unity.MLAgents;

public abstract class ModularMuscles : Muscles
{



    [SerializeField]
    protected MotorUpdateRule updateRule;

    protected float _deltaTime;

    public override void OnAgentInitialize()
    {
        if (updateRule != null)
        {
            _deltaTime = Utils.GetActionTimeDelta(gameObject.GetComponent<DecisionRequester>());
            updateRule.Initialize(this, _deltaTime);
        }

        else
            Debug.LogError("there is no motor update rule");


    }






}