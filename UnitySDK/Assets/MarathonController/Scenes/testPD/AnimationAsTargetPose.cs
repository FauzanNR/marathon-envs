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

    // Update is called once per frame
    void Update()
    {
        _mapAnim2Ragdoll.OnStep(Time.fixedDeltaTime);


        //List<Quaternion> targetRotation = _mapAnim2Ragdoll.LastRotation;


       List<Rigidbody> targets= _mapAnim2Ragdoll.GetRigidBodies();

        /*
        if (blockReferenceMovement)
        {
            _mapAnim2Ragdoll.transform.position = myRoot.transform.position + debugDistance;
            _mapAnim2Ragdoll.transform.rotation = myRoot.transform.rotation;
        }
        else
        {
            //this will make the character fall:
            
            myRoot.transform.position = _mapAnim2Ragdoll.transform.position + debugDistance;
            myRoot.transform.rotation = _mapAnim2Ragdoll.transform.rotation;
        }
        */

        foreach (var m in _motors)
        {
           

            Rigidbody a = targets.Find(x => x.name == m.name);

            if (m.isRoot)
            {
                continue; //neveer happens because excluded from list
            }
            else { 


            Vector3 targetNormalizedRotation = Utils.GetSwingTwist( a.transform.localRotation);
            //Vector3 targetNormalizedRotation = Utils.GetSwingTwist(targetRotation[j]);


            _ragDollMuscles.UpdateMotor(m, targetNormalizedRotation, Time.fixedDeltaTime);
            }



        }



    }
}
