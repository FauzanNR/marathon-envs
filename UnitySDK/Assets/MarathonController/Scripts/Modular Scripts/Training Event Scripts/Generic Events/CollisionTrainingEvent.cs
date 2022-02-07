using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTrainingEvent : TrainingEvent
{
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        OnTrainingEvent(System.EventArgs.Empty);
    }
}
