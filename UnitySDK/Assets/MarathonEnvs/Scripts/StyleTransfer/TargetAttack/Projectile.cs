using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public GameObject projectilePref;

    void Update()
    {
        var projectile = Instantiate(projectilePref);
        projectile.transform.localScale = new Vector3(2, 1, 1);

    }
}
