using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvironmentManager : MonoBehaviour
{
    [SerializeField]
    List<HandledEvent> handledEvents;

    private void Awake()
    {
        foreach (HandledEvent handledEvent in handledEvents)
        {
            //handledEvent.Activate();
        }
    }

}

[Serializable]
public class HandledEvent
{
    [SerializeField]
    TrainingEvent trainingEvent;

    [SerializeField]
    TrainingEventHandler trainingEventHandler;
/*
    public void Activate()
    {
        trainingEvent.SubscribeHandler(trainingEventHandler.Handler);
    }

    public void Deactivate()
    {
        trainingEvent.UnsubscribeHandler(trainingEventHandler.Handler);
    }*/


}
