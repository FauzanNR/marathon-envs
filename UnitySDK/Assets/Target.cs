using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : TrainingEventHandler
{
    // Start is called before the first frame update
    [SerializeField]
    Collider projectile;

    [SerializeField]
    bool shouldTrackHeight;

    [SerializeField]
    float targetHeight;

    [SerializeField]
    DReConAgent agent;

    public bool h;

    Vector3 lastPosition;

    [SerializeField]
    Vector3 idealPosition;

    [SerializeField]
    Transform pivot;
    Matrix4x4 pivotMatrix;

    [SerializeField]
    bool shouldMoveTarget;

    [SerializeField]
    Transform currentTarget;

    public override EventHandler Handler => (sender, args) => MoveTarget();


    private void Awake()
    {
        pivotMatrix = pivot.localToWorldMatrix;
        h = false;
        agent.onBeginHandler += (sender, args) => h = false;
    }



    void Start()
    {
        lastPosition = projectile.transform.position;
    }

    private void FixedUpdate()
    {
        if (shouldTrackHeight)
        {
            
            var curPosition = projectile.transform.position;

            if (curPosition.y <= targetHeight && lastPosition.y > targetHeight) idealPosition = curPosition;

            lastPosition = projectile.transform.position;
        }


    }

    void MoveTarget()
    {
        if (!shouldMoveTarget) return;
        Debug.Log("Move!");
        Vector3 relTargetPos = pivotMatrix.inverse.MultiplyPoint3x4(idealPosition);
        float meanRadius = relTargetPos.Horizontal3D().magnitude;
        float meanAngle = Mathf.Deg2Rad * Vector3.Angle(Vector3.right, relTargetPos.Horizontal3D());
        float meanHeight = relTargetPos.y;

        float radiusRange = 0.5f;
        float angleRange = 0.1f;
        float heightRange = 0.125f;

        float sampledRadius = UnityEngine.Random.Range(meanRadius - radiusRange, meanRadius + radiusRange);
        float sampledAngle = UnityEngine.Random.Range(meanAngle - angleRange, meanAngle + angleRange);
        float sampledHeight = UnityEngine.Random.Range(meanHeight - heightRange, meanHeight + heightRange);

        Debug.Log($"Radius: {sampledRadius}\nAngle: {sampledAngle}\nHeight: {sampledHeight}");

        Vector3 sampledPosition = pivotMatrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(sampledAngle) * sampledRadius, sampledHeight, Mathf.Sin(sampledAngle) * sampledRadius));

        currentTarget.position = sampledPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        h = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
