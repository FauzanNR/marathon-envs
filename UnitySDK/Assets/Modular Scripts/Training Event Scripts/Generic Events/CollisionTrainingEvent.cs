using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollisionTrainingEvent : TrainingEvent
{
    // Start is called before the first frame update

    [SerializeField]
    List<GameObject> collisionFilter;

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionFilter.Count==0 ||  collisionFilter.Any(go => go == collision.gameObject)) OnTrainingEvent(System.EventArgs.Empty);
    }
}
