using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using System.Linq;

public class EarlyTerminationHandler : DelayableEventHandler
{
    [SerializeField]
    Agent agent;

    public override EventHandler Handler => Terminate;

    [SerializeField]
    public List<DelayableEventHandler> blockingEvents;

    private void Terminate(object sender, EventArgs args)
    {
        if (IsWaiting) return;
        if (blockingEvents.Count != 0 && blockingEvents.Any(e => e.IsWaiting)) return;

        if (framesToWait != 0)
        {
            StartCoroutine(DelayedExecution(sender, args));
            return;
        }

        agent.EndEpisode();
    }

    protected override IEnumerator DelayedExecution(object sender, EventArgs args)
    {
        IsWaiting = true;
        yield return WaitFrames();
        agent.EndEpisode();
        IsWaiting = false;
    }
}
