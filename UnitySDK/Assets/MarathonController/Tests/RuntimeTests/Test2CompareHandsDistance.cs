using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ManyWorlds;
using Unity.MLAgents;

public class Test2CompareHandsDistance
{

  
    //returns the difference between the ragdoll hands and the reference hands, relative to each root
    public static float[] GetHandsDistance(TestParameters.Object2Test o)
    {

        return new float[2] {
               Mathf.Abs( Vector3.Distance(o.leftHandRagdoll.position, o.rootRagdoll.position) -  Vector3.Distance(o.leftHandSource.position, o.rootSource.position)),
               Mathf.Abs( Vector3.Distance(o.rightHandRagdoll.position, o.rootRagdoll.position)-  Vector3.Distance(o.rightHandSource.position, o.rootSource.position)) };

    }

    public class MyMonoBehaviourTest : MonoBehaviour, IMonoBehaviourTest
    {


        TestParameters configObject;
        private int frameCount;



        public bool IsTestFinished
        {
            get { return frameCount > configObject.frameEnd;  }
        }



        void OnEnable()
        {

            Debug.Log("enabling the config object");
           
            configObject = new TestParameters();

            configObject = (TestParameters)Resources.Load("TestParameters/LSPDSetup");
            

            Time.fixedDeltaTime = 1 / configObject.fixedFreq;
            Vector3 spawnStartPos = Vector3.zero;

            foreach (TestParameters.Object2Test object2Test in configObject.objects2test) {

                SpawnableEnv spawnableEnv = object2Test.spawnableEnv;
                spawnableEnv.UpdateBounds();
                Vector3 step = new Vector3(0f, 0f, spawnableEnv.bounds.size.z + (spawnableEnv.bounds.size.z * spawnableEnv.paddingBetweenEnvs));

                var env = Agent.Instantiate(spawnableEnv, spawnStartPos, spawnableEnv.gameObject.transform.rotation);
                spawnStartPos += step;


            }

            //configure it how we want it

            Debug.LogWarning("the config values set up in the test parameters are not applied to the instances of the objects");




            //most of this can probably be a method in class test parameters 
            metric checkDistance1 = new metric();
            checkDistance1.name = "DReCon Hand Distance";

            metric checkDistance2 = new metric();
            checkDistance2.name = "Mujoco Hand Distance";

            configObject.metrics = new metric[] { checkDistance1, checkDistance2 };

            foreach (metric m in configObject.metrics)
            
                m.initSampleList();

            


        }

        void Start()
        {


          






        }


        void Update()
        {
            frameCount++;


            if (frameCount > 1)
            {
                int whichmetric = 0;
                foreach (TestParameters.Object2Test object2Test in configObject.objects2test)
                {

                    configObject.metrics[whichmetric].addSample(GetHandsDistance(object2Test));

                    whichmetric++;


                }


        }

          



        }






        [UnityTest]

    public IEnumerator T01_TestHandsDistance()

    {
      yield return new MonoBehaviourTest<MyMonoBehaviourTest>();
    }



   


}
