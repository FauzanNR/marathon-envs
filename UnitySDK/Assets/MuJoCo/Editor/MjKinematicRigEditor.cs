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


        if (GUILayout.Button("Fix Mocapbodies"))
        {
            MjKinematicRig t = target as MjKinematicRig;
            t.ReplaceMocapBodies();
        }



        serializedObject.ApplyModifiedProperties();

    }
}
