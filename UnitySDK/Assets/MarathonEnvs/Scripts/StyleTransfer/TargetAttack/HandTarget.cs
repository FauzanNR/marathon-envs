using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTarget : MonoBehaviour
{

    public bool isTouch;
    public bool isGround;
    public bool jointBroke;
    public Collider physicsCollider;
    public Material[] materials;


    public Collider getCollider => physicsCollider;
    public Rigidbody getRigidBody => GetComponent<Rigidbody>();
    private Renderer render => GetComponent<Renderer>();
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "left_hand")
        {
            isTouch = true;

            render.sharedMaterial = materials[1];
        }
        else if (other.gameObject.name == "Terrain" && other.gameObject.tag == "gound")
        {
            isGround = true;
        }
        else
        {
            isGround = false;
            isTouch = false;
        }

    }
    void OnTriggerExit(Collider other)
    {
        render.sharedMaterial = materials[0];
        isTouch = false;
        isGround = false;
    }

    void OnJointBreak()
    {
        jointBroke = true;
    }

    private void OnDisable()
    {
        isGround = false;
        isTouch = false;
        Destroy(gameObject.GetComponent<FixedJoint>());
    }
}