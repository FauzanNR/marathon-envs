using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(Muscles))]
public class MusclesEditor : Editor
{

    [SerializeField]
    SerializedProperty Muscles;

    void OnEnable()
    {
        Muscles = serializedObject.FindProperty("Muscles");

    }




    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        GUILayout.Label("");




        base.OnInspectorGUI();

        
        if (GUILayout.Button("Setup4-1D-Debug"))
        {
            Muscles t = target as Muscles;
            t.Set1DRotations4Debug();
        }



        serializedObject.ApplyModifiedProperties();

    }





}
