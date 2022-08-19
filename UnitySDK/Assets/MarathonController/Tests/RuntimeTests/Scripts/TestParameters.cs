using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ManyWorlds;


[CreateAssetMenu(fileName = "TestParameters", menuName = "ScriptableObjects/TestParameters", order = 1)]
public class TestParameters : ScriptableObject
{
    //public static float tolerance4comparisons = 0.05f;


    public float fixedFreq = 25;
    public float KP = 0;

    public struct Object2Test {
        public SpawnableEnv spawnableEnv;


        public Transform rootRagdoll;
        public Transform rootSource;

        public  Transform leftHandRagdoll;
        public Transform leftHandSource;

        public Transform rightHandRagdoll;
        public Transform rightHandSource;
    }

    public Object2Test[] objects2test;

    public metric[] metrics;


    public float frameEnd=20;


    public void initMetrics()
    {
        foreach(metric m in metrics)
            m.initSampleList();
        
    }




}
