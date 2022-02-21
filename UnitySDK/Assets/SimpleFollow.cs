using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    [SerializeField]
    Transform from;

    [SerializeField]
    Transform to;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        to.position = from.position;
        to.rotation = from.rotation;
    }
}
