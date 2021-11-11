using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyManipulateTest : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    Transform trackTransform;

    [SerializeField]
    Rigidbody curRigidbody;

    [SerializeField]
    Vector3 offset;

    [SerializeField]
    bool useRigidbody;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(useRigidbody)
        {
            curRigidbody.MovePosition(trackTransform.position + offset);

            curRigidbody.MoveRotation(trackTransform.rotation);
        }
        else 
        {
            curRigidbody.position = trackTransform.position + offset;

            curRigidbody.rotation = trackTransform.rotation;
        }

        Debug.Log(curRigidbody.velocity);
        Debug.Log(curRigidbody.angularVelocity);

    }
}
