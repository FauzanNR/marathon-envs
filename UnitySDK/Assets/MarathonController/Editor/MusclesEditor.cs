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

        /*
        serializedObject.Update();


        GUILayout.Label("");

        */


        base.OnInspectorGUI();

/*
        if (GUILayout.Button("SetupROM170"))
        {
            Muscles t = target as Muscles;
            t.SetDOFAsLargeROMArticulations();
        }


        if (GUILayout.Button("Update Prefab"))
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        */


        serializedObject.ApplyModifiedProperties();

    }





}
