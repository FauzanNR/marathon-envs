using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RotationRecord
{
    public int recordPoints;
    public float rotationRecord;
    public float rewardRecord;

    public RotationRecord(int recordPoints, float rotationRecord, float rewardRecord)
    {
        this.recordPoints = recordPoints;
        this.rotationRecord = rotationRecord;
        this.rewardRecord = rewardRecord;
    }
}
