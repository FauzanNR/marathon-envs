using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct RangeOfMotionValue
{
    // [HideInInspector]
    public string name;
    public Vector3 lower;
    public Vector3 upper;
    public Vector3 rangeOfMotion;
}

[CreateAssetMenu(fileName = "RangeOfMotion", menuName = "Parser/CreateRangeOfMotionFile")]
public class RangeOfMotion004 : ScriptableObject
{
    public RangeOfMotionValue[] Values;
}

