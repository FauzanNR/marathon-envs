using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ApplyRangeOfMotion004))]
public class ApplyRangeOfMotion004Editor : Editor
{
    [SerializeField]
    SerializedProperty ROMparserSwingTwist;

    [SerializeField]
    string keyword4prefabs = "Constrained-procedurally";


    void OnEnable()
    {
        ROMparserSwingTwist = serializedObject.FindProperty("ROMparserSwingTwist");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Label("");
        
        base.OnInspectorGUI();

        if (GUILayout.Button("1. Calculate Degrees of Freedom (motors)"))
        {
            ApplyRangeOfMotion004 t = target as ApplyRangeOfMotion004;
            t.CalculateDoF();
        }        

        if (GUILayout.Button("1. Apply to RagDoll"))
        {
            ApplyRangeOfMotion004 t = target as ApplyRangeOfMotion004;
            t.ApplyToRagDoll();
        }
    }
}
