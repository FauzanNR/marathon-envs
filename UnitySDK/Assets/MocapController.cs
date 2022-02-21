using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class MocapController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Vector3 desiredPosition;

    [SerializeField]
    MjMocapBody mocapBody;

    unsafe void Start()
    {
        
    }

    // Update is called once per frame
    unsafe void Update()
    {
        Debug.Log(mocapBody.MujocoId);

    }
}
