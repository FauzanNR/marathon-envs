using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RotationRecord
{
    public int recordPoints;
    public float rotationDifferenceRecord;
    public float animatorBodyRotation;
    public float agentBodyRotation;
    public float rewardRecord;

    public RotationRecord(int recordPoints, float rotationDifferenceRecord, float animatorBodyRotation, float agentBodyRotation, float rewardRecord)
    {
        this.recordPoints = recordPoints;
        this.rotationDifferenceRecord = rotationDifferenceRecord;
        this.animatorBodyRotation = animatorBodyRotation;
        this.agentBodyRotation = agentBodyRotation;
        this.rewardRecord = rewardRecord;
    }
}
