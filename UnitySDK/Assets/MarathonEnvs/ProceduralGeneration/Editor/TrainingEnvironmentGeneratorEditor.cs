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
            t.GenerateRagdollForMocap();
        }





        if (GUILayout.Button("2 (optional) Generate ROM values"))
        {
            TrainingEnvironmentGenerator t = target as TrainingEnvironmentGenerator;
            t.GenerateRangeOfMotionParser();

            t.Prepare4RangeOfMotionParsing();



        }
        GUILayout.Label("If (2), press play until the values in the ROM file do not change. Then press stop.");

        if (GUILayout.Button("3.Configure and Store")) {
            TrainingEnvironmentGenerator t = target as TrainingEnvironmentGenerator;

            //"3.We Configure Ragdoll and learning agent"
            t.Prepare4EnvironmentStorage();

            t.ApplyROMasConstraintsAndConfigure();



            //instructions below stores them, it can only be done in an editor script

            storeEnvAsPrefab(t.Outcome);
            //once stored we can destroy them to keep the scene clean
            //DestroyImmediate(t.Outcome.gameObject);


            EditorUtility.SetDirty(t.info2store);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
        //GUILayout.Label("After (3), activate the Ragdoll game object within the hierarchy of the training environment generated.");






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
