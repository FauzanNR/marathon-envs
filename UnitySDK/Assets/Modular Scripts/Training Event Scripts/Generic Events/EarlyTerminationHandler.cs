using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EarlyTerminationHandler : TrainingEventHandler
{
    [SerializeField]
    Agent agent;

    public override EventHandler Handler => throw new NotImplementedException();

    private void Terminate(object sender, EventArgs args)
    {
        agent.EndEpisode();
    }
}
