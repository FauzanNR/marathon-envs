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


        if (GUILayout.Button("2.Store ArtBod with ROM"))
        {
            ROMparserSwingTwist t = target as ROMparserSwingTwist;
            Transform targetRoot = t.targetRagdollRoot;
            bool prefab4env =t.Prepare4PrefabStorage();


            applyROM2NewPrefab(prefab4env);
   

        }


        serializedObject.ApplyModifiedProperties();

    }



    void applyROM2NewPrefab(bool prefab4env= false)
    {
        ROMparserSwingTwist t = target as ROMparserSwingTwist;

        //ROMinfoCollector infoStored = t.info2store;

        Transform targetRoot = t.targetRagdollRoot;


        
        // Set the path,
        // and name it as the GameObject's name with the .Prefab format
        string localPath = "Assets/MarathonEnvs/Agents/Characters/MarathonMan004/" + targetRoot.name + ".prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        string uniqueLocalPath = AssetDatabase.GenerateUniqueAssetPath(localPath);


        if ( PrefabUtility.IsAnyPrefabInstanceRoot(targetRoot.gameObject)  )
        //We want to store it independently from the current prefab. Therefore:
            PrefabUtility.UnpackPrefabInstance(targetRoot.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);


        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAsset(targetRoot.gameObject, uniqueLocalPath);//, InteractionMode.UserAction);

        Debug.Log("Saved new CharacterPrefab at: " + uniqueLocalPath);


        if (prefab4env)
        {
            Transform targetEnv = t.trainingEnv.transform;
            string localEnvPath = "Assets/MarathonEnvs/Environments/" + targetEnv.name + ".prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            string uniqueLocalEnvPath = AssetDatabase.GenerateUniqueAssetPath(localEnvPath);


            if (PrefabUtility.IsAnyPrefabInstanceRoot(targetEnv.gameObject))
                //We want to store it independently from the current prefab. Therefore:
                PrefabUtility.UnpackPrefabInstance(targetEnv.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);


            // Create the new Prefab.
            PrefabUtility.SaveAsPrefabAsset(targetEnv.gameObject, uniqueLocalPath);//, InteractionMode.UserAction);

            Debug.Log("Saved new Environment Prefab at: " + uniqueLocalPath);







        }



    }


}
