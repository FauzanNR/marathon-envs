using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ManualEvent))]
public class TrainingEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        GUILayout.Label("");




        base.OnInspectorGUI();


        if (GUILayout.Button("Manually Trigger"))
        {
            TrainingEvent t = target as TrainingEvent;

            t.ManuallyTrigger(System.EventArgs.Empty);
        }



        serializedObject.ApplyModifiedProperties();

    }
}
