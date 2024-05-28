using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTarget : MonoBehaviour
{

    public bool isTouch;
    public Material[] materials;

    public Rigidbody getRigidBody => GetComponent<Rigidbody>();
    private Renderer render => GetComponent<Renderer>();
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "left_hand")
        {
            isTouch = true;

            render.sharedMaterial = materials[1];
        }
        else
        {
            isTouch = false;
        }

    }
    void OnTriggerExit(Collider other)
    {
        render.sharedMaterial = materials[0];
        isTouch = false;
    }
}