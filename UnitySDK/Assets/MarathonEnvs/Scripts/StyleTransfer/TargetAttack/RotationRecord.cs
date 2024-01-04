using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RotationRecord
{
    public int recordPoints;
    public float rotationRecord;

    public RotationRecord(int recordPoints, float rotationRecord)
    {
        this.recordPoints = recordPoints;
        this.rotationRecord = rotationRecord;
    }
}
