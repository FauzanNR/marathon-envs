using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DReCon
{

    [CustomEditor(typeof(DReConObservationSource))]
    public class DReConObservationSourceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            GUILayout.Label("");




            base.OnInspectorGUI();

            List<string> defaultSubset = new List<string> { "LeftToeBase", "RightToeBase", "Spine", "Head", "LeftForeArm", "RightForeArm", };


            if (GUILayout.Button("Attempt to auto-populate subset"))
            {
                DReConObservationSource t = target as DReConObservationSource;

                t.SetSimulationSubset(defaultSubset.Select(n => t.SimulationTransform.GetComponentsInChildren<Transform>().First(t => t.name.Contains(n))));
                t.SetKinematicSubset(defaultSubset.Select(n => t.KinematicTransform.GetComponentsInChildren<Transform>().First(t => t.name.Contains(n))));
            }



            serializedObject.ApplyModifiedProperties();

        }
    }
}
