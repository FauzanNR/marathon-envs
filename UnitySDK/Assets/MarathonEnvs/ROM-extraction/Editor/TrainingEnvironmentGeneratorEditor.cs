using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TrainingEnvironmentGenerator))]
public class TrainingEnvironmentGeneratorEditor : Editor
{



    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        GUILayout.Label("");

        base.OnInspectorGUI();

        if (GUILayout.Button("1.Generate training environment "))
        {
            TrainingEnvironmentGenerator t = target as TrainingEnvironmentGenerator;
            t.GenerateTrainingEnv();
        }


        if (GUILayout.Button("2.Store Env as Prefab"))
        {
            TrainingEnvironmentGenerator t = target as TrainingEnvironmentGenerator;

            //this stores them 
            storeEnvAsPrefab(t.Outcome);

            //once stored we can destroy them to keep the scene clean
            //DestroyImmediate(t.Outcome.gameObject);
          


        }


        //GUILayout.Label("How to use:");

        //GUILayout.TextArea(
        //    "Step 1: execute in play mode until the values in the Info2Store file do not change any more" +
        //    // " \n Step 2: click on button 1 to apply the constraints to check if the ragdoll looks reasonable" +
        //    // " \n Step 3: in edit mode, click on button 1, and then on button 2, to generate a new constrained ragdoll. If a template for a SpawnableEnv is provided, also a new environment for training");
        //    " \n Step 2: open the Ragdoll on which you want to apply the range of motion, and use the script ApplyRangeOfMotion004");


        serializedObject.ApplyModifiedProperties();

    }



    void storeEnvAsPrefab(ManyWorlds.SpawnableEnv env)
    {
       

        // Set the path,
        // and name it as the GameObject's name with the .Prefab format
        string localPath = "Assets/MarathonEnvs/Environments/" + env.name + ".prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        string uniqueLocalPath = AssetDatabase.GenerateUniqueAssetPath(localPath);


        if (PrefabUtility.IsAnyPrefabInstanceRoot(env.gameObject))
            //We want to store it independently from the current prefab. Therefore:
            PrefabUtility.UnpackPrefabInstance(env.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);


        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAsset(env.gameObject, uniqueLocalPath);

        Debug.Log("Saved new Training Environment at: " + uniqueLocalPath);


       
    }


}
