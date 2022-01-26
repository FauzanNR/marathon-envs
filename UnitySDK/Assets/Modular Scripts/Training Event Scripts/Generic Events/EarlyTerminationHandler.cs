using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EarlyTerminationHandler : TrainingEventHandler
{
    [SerializeField]
    Agent agent;

    public override EventHandler Handler => Terminate;

    [SerializeField]
    public BasicSetupHandler setupper;

    private void Terminate(object sender, EventArgs args)
    {
        if (setupper != null && setupper.IsWaiting) return;
        agent.EndEpisode();
    }
}
