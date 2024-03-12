using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RagdollManager : MonoBehaviour
{

  // List<Rigidbody> rbs;
  public Rigidbody spineRb;
  public Transform hipsRbTr;
  public Rigidbody handRb;
  public GameObject agent;
  public float gripForce;
  public float totalWeight = 0;
  public string heaviestBodyPart;
  public Vector3 defaultPosition;
  public Quaternion defaultRotation;
  // [SerializeField]
  // private Vector3 handTargetVelocityChange;
  [SerializeField]
  private float handMagnitude;

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
    handMagnitude = handTargetVelocityChange.magnitude;
    // Debug.Log("hand magnitude " + handTargetVelocityChange.magnitude);
    if (handTargetVelocityChange.magnitude > 3)
    {
      // print("Velocity Limit");
      spineRb.isKinematic = false;
      spineRb.useGravity = true;
      spineRb.constraints = RigidbodyConstraints.None;
      hipsRbTr.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
      Destroy(agent.GetComponent<BoxAgent>().joint);
      // if (agent.GetComponent<BoxAgent>().joint != null && agent.GetComponent<BoxAgent>().joint.connectedBody != null)
      // {
      //   agent.GetComponent<BoxAgent>().joint.connectedBody = null;
      // }
    }
    handPreviousVelocity = handCurrentVelocityy;

  }
}
