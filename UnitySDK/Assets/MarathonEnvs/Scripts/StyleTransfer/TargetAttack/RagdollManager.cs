using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RagdollManager : MonoBehaviour
{

  // List<Rigidbody> rbs;
  public Rigidbody spineRb;
  public float gripForce;
  public float totalWeight = 0;
  public string heaviestBodyPart;
  public Vector3 defaultPosition;

  float weightTmpt = 0;

  //    [ExecuteInEditMode]
  void CountRagdollWeight()
  {
    var rbs = this.GetComponentsInChildren<Rigidbody>();
    foreach (var rb in rbs)
    {
      totalWeight += rb.mass;
      if (rb.mass >= weightTmpt)
      {
        weightTmpt = rb.mass;
        heaviestBodyPart = rb.gameObject.name;
      }

    }
  }

  void Start()
  {
    CountRagdollWeight();
    defaultPosition = transform.position;
  }


  void FixedUpdate()
  {
    if (gripForce > 1f)
    {
      spineRb.isKinematic = false;
      spineRb.constraints = RigidbodyConstraints.None;
    }

  }
}
