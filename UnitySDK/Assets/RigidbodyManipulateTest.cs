using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyManipulateTest : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    Transform trackTransform;
    [SerializeField]
    Vector3 startVelocity;

    [SerializeField]
    Rigidbody curRigidbody;

    [SerializeField]
    Vector3 offset;

    [SerializeField]
    bool useRigidbody;

    [SerializeField]
    bool useVelocity;

    void Start()
    {
        curRigidbody.velocity = startVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        


        if (useVelocity)
        {
            //curRigidbody.velocity = startVelocity;
            return;
        }

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
        Debug.Log(curRigidbody.transform.InverseTransformDirection(curRigidbody.angularVelocity));

    }
}
