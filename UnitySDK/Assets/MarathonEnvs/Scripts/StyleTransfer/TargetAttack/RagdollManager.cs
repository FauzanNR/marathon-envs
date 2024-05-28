using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Unity.MLAgents.SideChannels;
using UnityEngine;
using UnityEngine.UIElements;

public class RagdollManager : MonoBehaviour
{

  public bool isRagdolled = false;
  public List<Transform> trBodyDefault;
  public List<Rigidbody> rbs;
  public List<CharacterJoint> charJoints;
  public List<Collider> colliders;
  public List<Collider> toBeIgnores;
  public Animator ragdollAnimator;
  public Rigidbody spineRb;
  public Transform hipsTr;
  public Rigidbody hipsRb;
  public Rigidbody handRb;
  public GameObject agent;
  public float gripForce;
  public float totalWeight = 0;
  public string heaviestBodyPart;
  public Vector3 hipsDefaultPosition;
  public Quaternion hipsDefaultRotation;
  public Vector3 hipsDefaultLocalPosition;
  public Quaternion hipsDefaultLocalRotation;
  public Vector3 spineDefaultPosition;
  public Quaternion spineDefaultRotation;
  public Vector3 handDefaultPosition;

  public float handMagnitudeMax = 3;
  // [SerializeField]
  // private Vector3 handTargetVelocityChange;
  [SerializeField]
  private float handMagnitude;

  Vector3 handPreviousVelocity;

  float weightTmpt = 0;



  void Start()
  {
    CountRagdollWeight();

    rbs = GetComponentsInChildren<Rigidbody>().ToList();
    ragdollAnimator = GetComponent<Animator>();
    charJoints = GetComponentsInChildren<CharacterJoint>().ToList();
    colliders = GetComponentsInChildren<Collider>().ToList();
    if (trBodyDefault.Count == 0 && trBodyDefault != null)
    {
      // print("one time only");
      hipsDefaultPosition = hipsTr.position;
      hipsDefaultRotation = hipsTr.rotation;
      hipsDefaultLocalPosition = hipsTr.localPosition;
      hipsDefaultLocalRotation = hipsTr.localRotation;

      spineDefaultPosition = spineRb.transform.position;
      spineDefaultRotation = spineRb.transform.rotation;
      handPreviousVelocity = handRb.velocity;
      handDefaultPosition = handRb.transform.position;
      foreach (var rb in rbs)
      {
        trBodyDefault.Add(rb.transform);
      }
    }
  }

  public float hipDistance = 0;
  void FixedUpdate()
  {
    hipDistance = Vector3.Distance(hipsDefaultLocalPosition, hipsTr.localPosition);
    var handCurrentVelocityy = handRb.velocity;
    var handTargetVelocityChange = handCurrentVelocityy - handPreviousVelocity;
    handMagnitude = handTargetVelocityChange.magnitude;
    // Debug.Log("hand magnitude " + handTargetVelocityChange.magnitude);
    if (handTargetVelocityChange.magnitude > handMagnitudeMax && !ragdollAnimator.isActiveAndEnabled)
    {
      // print("Velocity Limit");
      // spineRb.isKinematic = false;
      // spineRb.useGravity = true;
      // spineRb.constraints = RigidbodyConstraints.None;
      isRagdolled = true;
      hipsRb.isKinematic = false;
      hipsRb.constraints = RigidbodyConstraints.None;

      foreach (var rb in rbs)
      {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
      }
      //PART OF CHAR JOINT HAND GRIP
      // var agentsScript = agent.GetComponent<StyleTransfer002Agent>();
      // if (agentsScript.joint != null && agentsScript.joint.connectedBody != null)
      // {
      //   Destroy(agentsScript.joint);
      // }
    }
    //Prevent ragdoll target from flying over the arena, I also add physicalcage cage barrier(a.k.a collider)
    if (hipDistance > 50.0f)
    {
      // print("more than threshold FX" + hipDistance);
      var a = 0;
      foreach (var rb in rbs)
      {
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        rb.transform.position = trBodyDefault[a].position;
        a++;
      }
      resetRadoll2();
    }
    handPreviousVelocity = handCurrentVelocityy;
  }

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

  IEnumerator resetRagdoll()
  {
    yield return new WaitForSeconds(2);
    var targetVelocities = rbs;
    var targetTr = trBodyDefault;

    for (var i = 0; i < targetVelocities.Count; i++)
    {
      targetVelocities[i].velocity = Vector3.zero;
      targetVelocities[i].angularVelocity = Vector3.zero;
      // // targetVelocities[i].transform.position = targetTr[i].transform.position;
      // targetVelocities[i].transform.localPosition = targetTr[i].transform.localPosition;
      // // targetVelocities[i].transform.rotation = targetTr[i].transform.rotation;
      // targetVelocities[i].transform.localRotation = targetTr[i].transform.localRotation;
      // targetVelocities[i].gameObject.SetActive(false);
      // targetVelocities[i].gameObject.SetActive(true);
    }

    hipsTr.GetComponent<Rigidbody>().isKinematic = true;
    hipsTr.GetComponent<Rigidbody>().velocity = Vector3.zero;
    hipsTr.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    hipsTr.SetPositionAndRotation(hipsDefaultPosition, hipsDefaultRotation);

    // yield return new WaitForSeconds(1);
    // foreach (var rb in rbs)
    // {
    //   rb.gameObject.SetActive(true);
    // }
  }

  public void resetRadoll2()
  {
    isRagdolled = false;

    foreach (var rb in rbs)
    {
      rb.velocity = Vector3.zero;
      rb.detectCollisions = false;
      rb.useGravity = false;
      rb.isKinematic = true;
    }
    foreach (var cldr in colliders)
    {
      cldr.enabled = false;
    }
    foreach (var cj in charJoints)
    {
      cj.enableCollision = false;
    }

    foreach (var rb in rbs)
    {
      rb.interpolation = RigidbodyInterpolation.None;
      rb.detectCollisions = true;
      rb.useGravity = true;
      rb.velocity = Vector3.zero;
      rb.isKinematic = false;
    }
    foreach (var cj in charJoints)
    {
      cj.enableCollision = true;
    }
    foreach (var cldr in colliders)
    {
      cldr.enabled = true;
    }



    hipsRb.isKinematic = true;
    hipsRb.velocity = Vector3.zero;
    hipsRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    hipsTr.SetPositionAndRotation(hipsDefaultPosition, hipsDefaultRotation);
    hipsTr.localPosition = hipsDefaultLocalPosition;
  }

}
