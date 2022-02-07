using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionsDivergedEvent : TrainingEvent
{
    [SerializeField]
    private Transform positionA;

    [SerializeField]
    private Transform positionB;

    [SerializeField]
    private float maxDistance;

    private void Update()
    {
        if ((positionA.position - positionB.position).magnitude > maxDistance) OnTrainingEvent(EventArgs.Empty);
    }
}
