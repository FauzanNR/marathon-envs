using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mujoco
{

    [CustomEditor(typeof(MjPositionMuscles), true)]
    public class MjPositionMusclesEditor : Editor
    {
        float VelocityLimit = 40f;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            GUILayout.Label("");




            base.OnInspectorGUI();

            if(target is MjPDMuscles pd)
            {
                if(pd.useBaseline)
                {
                    pd.kinematicRef = EditorGUILayout.ObjectField("KinematicRef", pd.kinematicRef, typeof(Transform), true) as Transform;
                }
            }

            if (GUILayout.Button("Update Gains"))
            {
                MjPositionMuscles t = target as MjPositionMuscles;
                foreach (var act in t.Actuators)
                {

                    if (IsVelocity(act))
                    {
                        var tpd = t as MjPDMuscles;
                        ZeroParams(act);
                        act.CustomParams.GainPrm[0] = tpd.kv;
                        act.CustomParams.BiasPrm[2] = -tpd.kv;
                    }
                    else
                    {
                        ZeroParams(act);
                        act.CustomParams.GainPrm[0] = t.kp;
                        act.CustomParams.BiasPrm[1] = -t.kp;
                    }
                }
            }


            GUILayout.BeginHorizontal();
            VelocityLimit = EditorGUILayout.FloatField(label:"Velocity Limit", VelocityLimit);
            if (GUILayout.Button("Set Control Limits"))
            {
                MjPositionMuscles t = target as MjPositionMuscles;
                foreach(var a in t.Actuators)
                {
                    if (a.CustomParams.BiasPrm[2] != 0)
                    {
                        a.CommonParams.CtrlLimited = true;
                        float scale = Mathf.Deg2Rad * a.CommonParams.Gear[0];

                        if (t is MjPDMuscles tpd && tpd.useBaseline)
                        {
                            scale *= GameObject.FindObjectOfType<DReConAgent>().ActionTimeDelta;
                        }


                        a.CommonParams.CtrlRange = new Vector2(-VelocityLimit * scale, VelocityLimit * scale);
                    }
                    else 
                    {
                        var h = a.Joint as MjHingeJoint;
                        a.CommonParams.CtrlLimited = true;
                        a.CommonParams.CtrlRange = new Vector2(h.RangeLower * Mathf.Deg2Rad * a.CommonParams.Gear[0], h.RangeUpper * Mathf.Deg2Rad * a.CommonParams.Gear[0]);
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Remove Control Limits"))
            {
                MjPositionMuscles t = target as MjPositionMuscles;
                foreach (var a in t.Actuators)
                {
                    var h = a.Joint as MjHingeJoint;
                    a.CommonParams.CtrlLimited = false;
                    a.CommonParams.CtrlRange = Vector2.zero;
                }
            }


            



            serializedObject.ApplyModifiedProperties();

        }

        protected static void ZeroParams(MjActuator act)
        {
            for (int i = 0; i < 3; i++)
            {
                act.CustomParams.GainPrm[i] = 0;
            }

            for (int i = 0; i < 6; i++)
            {
                act.CustomParams.BiasPrm[i] = 0;
            }
        }

        protected bool IsVelocity(MjActuator act) => act.CustomParams.BiasPrm[2] != 0;
    }
}