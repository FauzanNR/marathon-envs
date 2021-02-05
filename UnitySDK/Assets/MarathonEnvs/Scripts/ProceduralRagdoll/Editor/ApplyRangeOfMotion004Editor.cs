using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ApplyRangeOfMotion004))]
public class ApplyRangeOfMotion004Editor : Editor
{
    [SerializeField]
    SerializedProperty ROMparserSwingTwist;

    //[SerializeField]
    //string keyword4prefabs = "Constrained-procedurally";


    void OnEnable()
    {
        ROMparserSwingTwist = serializedObject.FindProperty("ROMparserSwingTwist");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        base.OnInspectorGUI();



        /*

        //Moved to TrainingEnvironmentGeneratorEditor

        GUILayout.Label("Only IF Working With Prefab:");


        if (GUILayout.Button("1. Calculate Degrees of Freedom (motors)"))
        {
            ApplyRangeOfMotion004 t = target as ApplyRangeOfMotion004;
            t.CalculateDoF();
        }        

        if (GUILayout.Button("2. Apply ROM to RagDoll Prefab"))
        {
            ApplyRangeOfMotion004 t = target as ApplyRangeOfMotion004;
            t.ApplyRangeOfMotionToRagDoll();

            PrefabUtility.ApplyPrefabInstance( t.gameObject, InteractionMode.AutomatedAction);

        }

        if (GUILayout.Button("3. Apply Degrees of Freedom to Behavior Parameters"))
        {
            ApplyRangeOfMotion004 t = target as ApplyRangeOfMotion004;
            t.applyDoFOnBehaviorParameters();

            PrefabUtility.ApplyPrefabInstance(t.gameObject, InteractionMode.AutomatedAction);

        }
        */





    }
}
