using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mujoco
{

    public class MjRagdollTweaker : MonoBehaviour
    {

        public Transform tweakedRoot;

        public Transform actuatorRoot;

        public float stiffnessScale;

        public float dampingScale;

        public float gearScale;

        public IEnumerable<MjHingeJoint> joints => tweakedRoot.GetComponentsInChildren<MjHingeJoint>();
    }
}