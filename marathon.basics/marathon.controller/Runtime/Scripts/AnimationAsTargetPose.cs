using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


using Unity.Mathematics;
using MotorUpdate;

using Kinematic;
public class AnimationAsTargetPose : MonoBehaviour
{
 
    ModularMuscles _ragDollMuscles;

    [SerializeField]
    GameObject kinematicRigObject;

    IKinematicReference kinematicRig;

   
  
    IKinematic[] lrb;

   


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

        _ragDollMuscles = GetComponent<ModularMuscles>();
        _ragDollMuscles.OnAgentInitialize();


        if (kinematicRigObject != null)
        {
            kinematicRig = kinematicRigObject.GetComponent<IKinematicReference>();
            kinematicRig.OnAgentInitialize();
        }
        else {
            Debug.LogError("I do not know what is the reference kinematic");
        
        }

         lrb = Utils.GetMotors(gameObject);

       
    }





    // Update is called once per frame
    void FixedUpdate()
    {
        float[] actions = _ragDollMuscles.GetActionsFromState();

        /*      
            int im = 0;

          foreach (var a in targets)
            {
                targetRotations[im] = a.JointPosition;

                im++;
            }
      */

        float[] previousActions = _ragDollMuscles.GetActionsFromState();
       

        _ragDollMuscles.ApplyActions(Enumerable.Repeat(0f, previousActions.Length).ToArray());


    
    }
     



    

}