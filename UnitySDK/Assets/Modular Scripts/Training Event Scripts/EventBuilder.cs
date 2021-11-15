using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EventBuilder : MonoBehaviour
{
    [SerializeField]
    List<HandledEvent> handledEvents;

    private void Awake()
    {
        foreach (HandledEvent handledEvent in handledEvents)
        {
            handledEvent.Activate();
        }
    }

}

[Serializable]
public class HandledEvent
{
    [SerializeField]
    TrainingEvent trainingEvent;

    [SerializeField]
    TrainingEventHandler effect;

    public void Activate()
    {
        trainingEvent.SubscribeHandler(effect.Handler);
    }

    public void Deactivate()
    {
        trainingEvent.UnsubscribeHandler(effect.Handler);
    }


}
