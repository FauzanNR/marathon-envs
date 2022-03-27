using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MjRagdollTweaker))]
public class MjRagdollTweakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();



        if (GUILayout.Button("Update Gains"))
        {
 
        }

        



        serializedObject.ApplyModifiedProperties();

    }

   
}


