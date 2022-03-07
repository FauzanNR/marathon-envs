using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MjKinematicRig))]
public class MjKinematicRigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        GUILayout.Label("");




        base.OnInspectorGUI();


        if (GUILayout.Button("Replace MjGeoms with Unity Colliders"))
        {
            MjKinematicRig t = target as MjKinematicRig;
            t.ReplaceGeoms();
        }



        serializedObject.ApplyModifiedProperties();

    }
}
