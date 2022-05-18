using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


using Unity.Mathematics;

using Kinematic;
public class AnimationAsTargetPose : MonoBehaviour
{
 
    ModularMuscles _ragDollMuscles;

   
    MapAnim2Ragdoll _mapAnim2Ragdoll;
    float3[] targetRotations;

    IKinematic[] lrb;

    List<IReducedState> targets;



    List<IReducedState> getTargets() {

      

        List<IReducedState> rbs = new List<IReducedState>();

        List<Rigidbody> rblistraw = transform.parent.GetComponentInChildren<MapAnim2Ragdoll>().GetRigidBodies();

        foreach (var m in lrb)
        {

            Rigidbody a = rblistraw.Find(x => x.name == m.gameObject.name);

            if (a != null)
                rbs.Add(new RigidbodyAdapter(a)) ;

        }
        return rbs;


    }

    // Start is called before the first frame update
    void OnEnable()
    {

      

        _mapAnim2Ragdoll = transform.parent.GetComponentInChildren<MapAnim2Ragdoll>();

        _ragDollMuscles = GetComponent<ModularMuscles>();
        lrb = Utils.GetMotors(gameObject);

        targets = getTargets();
        targetRotations = new float3[targets.Count];

    }





    // Update is called once per frame
    void FixedUpdate()
    {
        float[] actions = _ragDollMuscles.GetActionsFromState();

        int im = 0;
        foreach (var a in targets)
        {
            targetRotations[im] = a.JointPosition;

            im++;
        }

       

        Debug.LogError("TODO: need to apply the rule as a forces");


        //_ragDollMuscles.ApplyRuleAsRelativeTorques(targetRotations);
        /*
       public void ApplyRuleAsRelativeTorques(float3[] targetRotation)
       {



           float3[] torques = updateRule.GetJointForces(targetRotation);
           Debug.LogError("NEED TO APPLY THIS AS JOINT FORCES");


       }


          public override float3[] GetJointForces(float3[] targetRotation)
     {
         float3[] result = new float3[_motors.Length];

         for (int i = 0; i < _motors.Length; i++)
             result[i] = GetRelativeTorque(_motors[i], targetRotation[i]);

         return result;

     }*/


    }

}