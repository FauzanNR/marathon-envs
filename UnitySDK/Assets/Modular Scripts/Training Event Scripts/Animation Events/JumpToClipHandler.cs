using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpToClipHandler : TrainingEventHandler
{
    [SerializeField]
    Animator animator;
    [SerializeField]
    string clipName;
    public override EventHandler Handler => JumpToClip;

    void JumpToClip(object sender, EventArgs args)
    {
        animator.Play(stateName:clipName, layer: 0, normalizedTime: 0f);
    }
}
