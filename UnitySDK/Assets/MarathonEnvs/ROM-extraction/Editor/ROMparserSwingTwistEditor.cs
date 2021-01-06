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
            applyROM2NewPrefab(rda, envPrefab);

            //once stored we can destroy them to keep the scene clean
            //Destroy(envPrefab);
            //Destroy(rda);


        }

        GUILayout.Label("Prefab Keyword:");
        keyword4prefabs = GUILayout.TextField(keyword4prefabs);

        serializedObject.ApplyModifiedProperties();

    }



    void applyROM2NewPrefab(RagDollAgent rda, ManyWorlds.SpawnableEnv envPrefab = null)
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
