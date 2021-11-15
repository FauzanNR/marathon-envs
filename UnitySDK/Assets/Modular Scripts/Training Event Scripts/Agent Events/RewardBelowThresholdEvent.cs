using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using DReCon;

public class RewardBelowThresholdEvent : TrainingEvent
{
    [SerializeField]
    Agent trackedAgent;

    [SerializeField]
    float rewardThreshold;

    private void Awake()
    {
        IEventsAgent eventsAgent = trackedAgent as IEventsAgent;
        if (eventsAgent == null)
        {
            throw new InvalidCastException("Agent should implement IEventsAgent");
        }

        eventsAgent.onActionHandler += ThresholdWrapper;
    }

    private void ThresholdWrapper(object sender, AgentEventArgs eventArgs)
    {
        Debug.Log($"The current reward is: {eventArgs.reward}");
        if (eventArgs.reward >= rewardThreshold) return;

        OnTrainingEvent(eventArgs);
    }

}
