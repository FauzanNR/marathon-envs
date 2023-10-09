using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileControler : MonoBehaviour
{

    public ProjectileManager parent;
    public float delayOff = 3f;
    private void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.tag == "Agent" || other.gameObject.tag == "ground")
        {
            StartCoroutine(delayToOff());
        }
    }

    IEnumerator delayToOff()
    {
        yield return new WaitForSeconds(delayOff);
        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(false);
        // transform.position = parent.transform.position;
        // transform.rotation = parent.transform.rotation;

    }

}