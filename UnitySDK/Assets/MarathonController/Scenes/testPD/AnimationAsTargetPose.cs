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


    public List<Rigidbody> targets;

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

       
        //it is set up from the Muscles script to make sure we have the same order than the others.
        //targets = _mapAnim2Ragdoll.GetRigidBodies();

     
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
    void FixedUpdate()
    {
        _mapAnim2Ragdoll.OnStep(Time.fixedDeltaTime);


        _ragDollMuscles.MimicRigidBodies(targets);

     

    }



}
