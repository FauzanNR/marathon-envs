using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupTestParameters : MonoBehaviour
{

    public  float PhysicsFPS = 150;
    public  float KP = 10000;

 
    private void OnEnable()
    {

        Time.fixedDeltaTime = 1 /PhysicsFPS;
        Debug.Log("fixed delta time: " + Time.fixedDeltaTime);
       

    }

   
}
