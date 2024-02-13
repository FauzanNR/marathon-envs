using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RagdollManager : MonoBehaviour
{

  // List<Rigidbody> rbs;
  public Rigidbody spineRb;
  public Transform hipsRbTr;
  public Rigidbody handRb;
  public float gripForce;
  public float totalWeight = 0;
  public string heaviestBodyPart;
  public Vector3 defaultPosition;
  public Quaternion defaultRotation;

  Vector3 handPreviousVelocity;

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
    defaultPosition = hipsRbTr.position;
    defaultRotation = hipsRbTr.rotation;
    handPreviousVelocity = handRb.velocity;

  }


  void FixedUpdate()
  {
    var handCurrentVelocityy = handRb.velocity;
    var handTargetVelocityChange = handCurrentVelocityy - handPreviousVelocity;
    // Debug.Log("hand magnitude " + handTargetVelocityChange.magnitude);
    if (handTargetVelocityChange.magnitude > 8)
    {
      spineRb.isKinematic = false;
      spineRb.constraints = RigidbodyConstraints.None;
    }
    handPreviousVelocity = handCurrentVelocityy;

  }
}
