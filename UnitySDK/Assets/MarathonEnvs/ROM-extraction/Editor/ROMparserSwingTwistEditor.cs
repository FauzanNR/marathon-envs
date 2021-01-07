using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//This script assumes information was stored as SwingTwist
//This means the script goes together with ROMparserSwingTwist

//we assume the articulationBodies have a name structure of hte form ANYNAME:something-in-the-targeted-joint

[CustomEditor(typeof(ROMparserSwingTwist))]
public class ROMparserSwingTwistEditor: Editor
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


        if (GUILayout.Button("1.Apply ROM As Constraints"))
        {

            ROMparserSwingTwist t = target as ROMparserSwingTwist;
            Transform targetRoot = t.targetRagdollRoot;
            t.ApplyROMAsConstraints();
        }


        if (GUILayout.Button("2.Store Ragdoll and Env as Prefab"))
        {
            ROMparserSwingTwist t = target as ROMparserSwingTwist;
            Transform targetRoot = t.targetRagdollRoot;

            ManyWorlds.SpawnableEnv envPrefab = null;
            RagDollAgent rda = null;

            //this creates the needed objects and configures them
            t.Prepare4PrefabStorage(out rda, out envPrefab);
            
            //this stores them 
            storeNewPrefabWithROM(rda, envPrefab);

            //once stored we can destroy them to keep the scene clean
            DestroyImmediate(rda.gameObject);
            if (envPrefab != null)
                DestroyImmediate(envPrefab.gameObject);


        }

        GUILayout.Label("Prefab Keyword:");
        keyword4prefabs = GUILayout.TextField(keyword4prefabs);

        GUILayout.Label("How to use:");

        GUILayout.TextArea(
            "Step 1: execute in play mode until the values in the Info2Store file do not change any more" +
            " \n Step 2: click on button 1 to apply the constraints to check if the ragdoll looks reasonable" +
            " \n Step 3: in edit mode, click on button 1, and then on button 2, to generate a new constrained ragdoll. If a template for a SpawnableEnv is provided, also a new environment for training");



        serializedObject.ApplyModifiedProperties();

    }



    void storeNewPrefabWithROM(RagDollAgent rda, ManyWorlds.SpawnableEnv envPrefab = null)
    {
        ROMparserSwingTwist t = target as ROMparserSwingTwist;

        //ROMinfoCollector infoStored = t.info2store;

        Transform targetRoot = t.targetRagdollRoot;


        string add2prefabs = keyword4prefabs;

        

        // Set the path,
        // and name it as the GameObject's name with the .Prefab format
        string localPath = "Assets/MarathonEnvs/Agents/Characters/MarathonMan004/" + targetRoot.name + add2prefabs + ".prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        string uniqueLocalPath = AssetDatabase.GenerateUniqueAssetPath(localPath);


        if ( PrefabUtility.IsAnyPrefabInstanceRoot(targetRoot.gameObject)  )
        //We want to store it independently from the current prefab. Therefore:
            PrefabUtility.UnpackPrefabInstance(targetRoot.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);


        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAsset(targetRoot.gameObject, uniqueLocalPath);

        Debug.Log("Saved new CharacterPrefab at: " + uniqueLocalPath);


        if (envPrefab != null)
        {
            Transform targetEnv = envPrefab.transform;

            targetEnv.name = "ControllerMarathonManEnv" + add2prefabs;

            string localEnvPath = "Assets/MarathonEnvs/Environments/" + targetEnv.name + ".prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            string uniqueLocalEnvPath = AssetDatabase.GenerateUniqueAssetPath(localEnvPath);

            if (PrefabUtility.IsAnyPrefabInstanceRoot(targetEnv.gameObject))
                //We want to store it independently from the current prefab. Therefore:
                PrefabUtility.UnpackPrefabInstance(targetEnv.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);


            // Create the new Prefab.
            PrefabUtility.SaveAsPrefabAsset(targetEnv.gameObject, uniqueLocalEnvPath);

            Debug.Log("Saved new Environment Prefab at: " + uniqueLocalEnvPath);
           
        }



    }


}
