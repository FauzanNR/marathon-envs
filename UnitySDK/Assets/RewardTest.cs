using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class RewardTest : MonoBehaviour
{
    public Transform tagetTransform;
    public float distanceToTarget;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, tagetTransform.position);
        print("Distance debug: " + Mathf.Clamp((Mathf.Exp(-distanceToTarget)), 0, 1));
        print("targetPosition" + tagetTransform.position);

    }
}
