using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ManyWorlds;
using Unity.MLAgents;

using System.Linq;



    public class Test2CompareHandsDistance : CompareHandsDistance, IMonoBehaviourTest
    {

        public void  OnEnable() {

            configObject = (TestParameters)Resources.Load("TestParameters/DReConAndMujoco120HZ500KP");

        }

        public bool IsTestFinished
        {
            get {
                return frameCount > 
                    configObject.frameEnd;
                   }
        }


        [UnityTest]

        public IEnumerator T01_TestHandsDistanceDReConAndMujoco120HZ500KP()

        {
            yield return new MonoBehaviourTest<Test2CompareHandsDistance>();
        }



    }  


