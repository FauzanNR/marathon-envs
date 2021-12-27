using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Rigidbody throwableRigidbody;

    [SerializeField]
    Transform trackedTransform;

    [SerializeField]
    float releaseTime;

    float startTime;

    List<(float, float)> distanceTimeStamps;
    Vector3 posOffset;
    Quaternion rotOffset;

    private void Awake()
    {
        posOffset = throwableRigidbody.position - trackedTransform.position;
        rotOffset = Quaternion.Inverse(throwableRigidbody.rotation) * trackedTransform.rotation;
        distanceTimeStamps = new List<(float, float)>();
    }

    private void Start()
    {
        throwableRigidbody.isKinematic = true;
        startTime = 0f;
        if (releaseTime > 0f) StartCoroutine(DelayedRelease(releaseTime));
        //StartCoroutine(DelayedReset(5f));
    }

    void FixedUpdate()
    {
        if (!throwableRigidbody.isKinematic)
        {
            return;
        }

        if (releaseTime <= 0f)
        {
            distanceTimeStamps.Add((BallisticsRange(throwableRigidbody.velocity, throwableRigidbody.position.y), Time.time - startTime));
        }

        throwableRigidbody.MovePosition(trackedTransform.position + posOffset);
        throwableRigidbody.MoveRotation(rotOffset * trackedTransform.rotation);
    }

    private void Reset()
    {
        releaseTime = distanceTimeStamps.Skip(5).OrderByDescending(i => i.Item1).First().Item2;
        throwableRigidbody.isKinematic = true;
        startTime = 0f;
        if(releaseTime > 0f) StartCoroutine(DelayedRelease(releaseTime));
    }

    IEnumerator DelayedRelease(float delay)
    {
        yield return new WaitForSeconds(delay);

        throwableRigidbody.isKinematic = false;
    }

    IEnumerator DelayedReset(float delay)
    {
        yield return new WaitForSeconds(delay);

        Reset();
    }

    private static float BallisticsRange(Vector3 velocity, float height)
    {
        Vector2 velX = new Vector2(velocity.x, velocity.y);
        Vector2 velZ = new Vector2(velocity.z, velocity.y);

        Vector2 thetas = new Vector2(Vector2.Angle(velX, Vector2.right), Vector2.Angle(velZ, Vector2.right));
        Vector2 speeds = new Vector2(velX.magnitude, velZ.magnitude);

        float dX = BallisticsRange(speeds.x, thetas.x, Physics.gravity.magnitude, height);
        float dZ = BallisticsRange(speeds.y, thetas.y, Physics.gravity.magnitude, height);

        return new Vector2(dX, dZ).magnitude;
    
    }

    private static float BallisticsRange(float speed, float angle, float gravity, float height)
    {
        float d = speed * Mathf.Cos(angle) / gravity *
            (speed * Mathf.Sin(angle) + Mathf.Sqrt(speed * speed * Mathf.Sin(angle) * Mathf.Sin(angle) + 2 * gravity * height));
        return d;
    }

}
