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


        if (GUILayout.Button("Apply ROM As Constraints"))
        {

            ROMparserSwingTwist t = target as ROMparserSwingTwist;
            Transform targetRoot = t.targetRagdollRoot;
            t.ApplyROMAsConstraints();
        }


        if (GUILayout.Button("Store ArtBod with ROM"))
        {

            applyROM2NewPrefab();
   

        }


        serializedObject.ApplyModifiedProperties();

    }



    void applyROM2NewPrefab()
    {
        ROMparserSwingTwist t = target as ROMparserSwingTwist;

        ROMinfoCollector infoStored = t.info2store;

        Transform targetRoot = t.targetRagdollRoot;





        // t.ApplyROMAsConstraints();



        // Set the path,
        // and name it as the GameObject's name with the .Prefab format
        string localPath = "Assets/MarathonEnvs/Agents/Characters/MarathonMan004/" + targetRoot.name + "Constrained.prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        string uniqueLocalPath = AssetDatabase.GenerateUniqueAssetPath(localPath);


        if ( PrefabUtility.IsAnyPrefabInstanceRoot(targetRoot.gameObject)  )
        //We want to store it independently from the current prefab. Therefore:
            PrefabUtility.UnpackPrefabInstance(targetRoot.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);


        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAsset(targetRoot.gameObject, uniqueLocalPath);//, InteractionMode.UserAction);

        Debug.Log("Saved new prefab at: " + uniqueLocalPath);



    }


}
