using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using ManyWorlds;

public class AnimationAsTargetPose : MonoBehaviour
{
    List<ArticulationBody> _motors;
    Muscles _ragDollMuscles;
    MapAnim2Ragdoll _mapAnim2Ragdoll;


    List<Rigidbody> targets;

    /*
    Vector3 debugDistance;

    [SerializeField]
    bool blockReferenceMovement;
    */
    ArticulationBody myRoot;

    // Start is called before the first frame update
    void Start()
    {
        SpawnableEnv _spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _mapAnim2Ragdoll = _spawnableEnv.GetComponentInChildren<MapAnim2Ragdoll>();

        _mapAnim2Ragdoll.OnAgentInitialize();

        targets = _mapAnim2Ragdoll.GetRigidBodies();

        // debugDistance = _mapAnim2Ragdoll.transform.position - transform.position;


        _motors = GetComponentsInChildren<ArticulationBody>()
            .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
            .Where(x => !x.isRoot)
            .Distinct()
            .ToList();
        
        _ragDollMuscles = GetComponent<Muscles>();

       
            ArticulationBody[] roots = GetComponentsInChildren<ArticulationBody>()
            .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
            .Where(x => x.isRoot).ToArray();
            myRoot = roots[0];
        
        
        /*
        if (blockReferenceMovement)        
            roots[0].immovable = true;
        */
    }



    void UpdateMuscles(float[] vectorAction)
    {


        switch (_ragDollMuscles.MotorUpdateMode)
        {

            case (Muscles.MotorMode.linearPD):


                break;

            default:

                foreach (var m in _motors)
                {
                    if (m.isRoot)
                        continue;

                    Rigidbody a = targets.Find(x => x.name == m.name);

                    Vector3 targetNormalizedRotation = Utils.GetSwingTwist(a.transform.localRotation);

                    _ragDollMuscles.UpdateMotor(m, targetNormalizedRotation, Time.fixedDeltaTime);
                   

                }



                break;
        }//




    }



    // Update is called once per frame
    void Update()
    {
        _mapAnim2Ragdoll.OnStep(Time.fixedDeltaTime);


       

      

        foreach (var m in _motors)
        {
           

            Rigidbody a = targets.Find(x => x.name == m.name);

            if (m.isRoot)
            {
                continue; //neveer happens because excluded from list
            }
            else { 


            Vector3 targetNormalizedRotation = Utils.GetSwingTwist( a.transform.localRotation);
            

            _ragDollMuscles.UpdateMotor(m, targetNormalizedRotation, Time.fixedDeltaTime);
            }



        }



    }
}
