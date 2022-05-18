using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Unity.MLAgents;

public abstract class Muscles: MonoBehaviour
{
   

    public abstract int ActionSpaceSize { get; }

    //TODO: we shold remove actionTimeDelta from teh call Apply Actions in Muscles, this is only used inside motorupdaterule
    public abstract void ApplyActions(float[] actions, float actionTimeDelta);

    public abstract float[] GetActionsFromState();

    public virtual void OnAgentInitialize() { }
}
