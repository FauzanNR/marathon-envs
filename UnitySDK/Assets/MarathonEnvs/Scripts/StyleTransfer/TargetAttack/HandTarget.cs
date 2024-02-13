using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTarget : MonoBehaviour
{

    public bool isTouch;

    public Rigidbody getRigidBody => GetComponent<Rigidbody>();
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "left_hand")
        {
            isTouch = true;
        }
        else
        {
            isTouch = false;
        }

    }
    void OnTriggerExit(Collider other)
    {
        isTouch = false;
    }
}