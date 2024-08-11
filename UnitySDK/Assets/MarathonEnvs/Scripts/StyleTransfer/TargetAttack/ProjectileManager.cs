using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public GameObject agent;
    public GameObject projectilePref;
    public List<GameObject> projectileContainer;
    public ForceMode forceMode;
    public bool isRotate = false;
    public float shootDelay = 1f;
    public float orbitSpeed = 10f;
    public float orbitRadius = 30f;
    public float orbitHeight = 1f;
    public float projectileForce = 5f;
    public bool drawRey;

    void Start()
    {
        generateProjectile();
    }
    void FixedUpdate()
    {
        if (isRotate)
            launcherMovement();
        StartCoroutine(lunchProjectile());
        // StartCoroutine(projectile());
        Debug.DrawRay(transform.position, transform.forward.normalized * orbitRadius, Color.red);

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (drawRey)
            Gizmos.DrawSphere(agent.transform.position, 1);
    }
    void launcherMovement()
    {
        var agentPosition = agent.GetComponent<Transform>().position;

        // Calculate the new position for the object in a circular orbit
        float angle = Time.time * orbitSpeed;
        Vector3 newPosition = agentPosition + new Vector3(Mathf.Cos(angle), orbitHeight, Mathf.Sin(angle)) * orbitRadius;

        // Move the object to the new position
        transform.position = newPosition;

        // Make the object look at the center point
        transform.LookAt(agentPosition);
    }

    void generateProjectile()
    {
        if (projectileContainer != null || projectileContainer.Count != 0)
            for (var i = 0; i < 5; i++)
            {
                var projectile = Instantiate(projectilePref, transform.position, Quaternion.identity);
                projectile.transform.localScale = new Vector3(Random.Range(0.2f, 0.7f), Random.Range(0.2f, 0.7f), Random.Range(0.2f, 0.7f));
                projectile.gameObject.SetActive(false);
                projectile.GetComponent<ProjectileControler>().parent = this;
                projectileContainer.Add(projectile);
            }
    }

    IEnumerator lunchProjectile()
    {
        foreach (var projectile in projectileContainer)
        {
            if (projectile.activeInHierarchy == false)
            {
                shootProjectile(projectile);
                yield return new WaitForSeconds(shootDelay);

            }
        }
    }

    void shootProjectile(GameObject projectile)
    {

        // var projectileDirection = (transform.position - agent.GetComponent<Transform>().position).normalized * projectileForce;

        // projectile.GetComponent<Rigidbody>()
        //         .AddForceAtPosition(projectileDirection, transform.forward, ForceMode.Impulse);
        projectile.transform.position = transform.position;
        var getForward = transform.forward.normalized;

        projectile.gameObject.SetActive(true);
        projectile.GetComponent<Rigidbody>()
                .AddForce(getForward * projectileForce, forceMode);
        // .velocity = getForward * projectileForce;
    }

    IEnumerator projectile()
    {
        yield return new WaitForSeconds(shootDelay);
        var projectile = Instantiate(projectilePref, transform.position, Quaternion.identity);
        projectile.transform.localScale = new Vector3(Random.Range(1, 3), Random.Range(1, 3), Random.Range(1, 3));
        var getForward = transform.forward;
        projectile.GetComponent<Rigidbody>()
                .AddForce(getForward * projectileForce, forceMode);
    }
}
