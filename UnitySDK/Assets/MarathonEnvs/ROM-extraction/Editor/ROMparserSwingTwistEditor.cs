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



    ArticulationBody[] articulationBodies;

     public override void OnInspectorGUI()
    {
        serializedObject.Update();


        base.OnInspectorGUI();


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

        Transform targetRoot = t.targetRoot;
        
       
        
        Transform[] joints = t.targetJoints;
        //we want them all:
        if (joints.Length == 0)
            joints = targetRoot.GetComponentsInChildren<Transform>();


        List<ArticulationBody> temp = new List<ArticulationBody>();
        for (int i = 0; i < joints.Length; i++)
        {
            ArticulationBody a = joints[i].GetComponent<ArticulationBody>();
            if (a != null)
                temp.Add(a);
        }

        articulationBodies = temp.ToArray();






        List<string> jNames = new List<string>(infoStored.jointNames);

        for (int i = 0; i < articulationBodies.Length; i++)
        {
            string s = articulationBodies[i].name;
            string[] parts = s.Split(':');
            //we assume the articulationBodies have a name structure of hte form ANYNAME:something-in-the-targeted-joint

            int index = -1;

            index = jNames.FindIndex(x => x.Contains(parts[1]));

            if (index < 0)
                Debug.Log("Could not find a joint name matching " + s + "and specifically: " + parts[1]);
            else
            {

                //The swing in one axis:
                ArticulationDrive yboundaries = new ArticulationDrive();
                yboundaries.upperLimit = infoStored.maxRotations[index].y;
                yboundaries.lowerLimit = infoStored.minRotations[index].y;
                articulationBodies[i].yDrive = yboundaries;


                //The twist in the second axis:
                ArticulationDrive zboundaries = new ArticulationDrive();
                zboundaries.upperLimit = infoStored.maxRotations[index].z;
                zboundaries.lowerLimit = infoStored.minRotations[index].z;
                articulationBodies[i].zDrive = zboundaries;


                //The twist:
                ArticulationDrive xboundaries = new ArticulationDrive();
                xboundaries.upperLimit = infoStored.maxRotations[index].x;
                xboundaries.lowerLimit = infoStored.minRotations[index].x;
                articulationBodies[i].xDrive = xboundaries;



            }






        }


        // Set the path,
        // and name it as the GameObject's name with the .Prefab format
        string localPath = "Assets/ROM-Example/" + targetRoot.name + "Constrained.prefab";

        // Make sure the file name is unique, in case an existing Prefab has the same name.
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAssetAndConnect(targetRoot.gameObject, localPath, InteractionMode.UserAction);





    }


}
