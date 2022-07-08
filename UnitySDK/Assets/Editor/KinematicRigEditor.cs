using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KinematicRig))]
public class KinematicRigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();





        base.OnInspectorGUI();


        if (GUILayout.Button("Create Rigidbody Ragdoll from ArticulationBody Ragdoll"))
        {
            KinematicRig r = target as KinematicRig;

            var copy = Instantiate(r.TrackedTransformRoot);
            foreach(var ab in copy.GetComponentsInChildren<ArticulationBody>())
            {
                var go = ab.gameObject;
                var mass = ab.mass;
                DestroyImmediate(ab);
                var rb = go.AddComponent<Rigidbody>();
                rb.mass = mass;
                rb.isKinematic = true;
            }




        }



        serializedObject.ApplyModifiedProperties();

    }
}
