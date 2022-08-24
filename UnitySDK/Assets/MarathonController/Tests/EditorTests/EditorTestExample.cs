using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using Unity.Mathematics;


public class EditorTestExample
{

    public bool compareAtStart;

  
    public
    ArticulationBody ab;

    float KP = 500;

    [UnityTest]
    public IEnumerator ExampleEditorTest()
    {
        GameObject hierarchy = (GameObject)Resources.Load("Prefabs/ExamplePrefab");


        yield return null;
    }



}