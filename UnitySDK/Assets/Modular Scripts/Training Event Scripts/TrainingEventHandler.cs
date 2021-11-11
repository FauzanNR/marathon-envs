using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A flexible base class that can supply a handling function when paired up with a TrainingEvent
/// </summary>
public abstract class TrainingEventHandler : MonoBehaviour
{
    public abstract EventHandler Handler { get; }
}
