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

    // Start is called before the first frame update
    void Start()
    {
        SpawnableEnv _spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _mapAnim2Ragdoll = _spawnableEnv.GetComponentInChildren<MapAnim2Ragdoll>();

        _mapAnim2Ragdoll.OnAgentInitialize();


        _motors = GetComponentsInChildren<ArticulationBody>()
            .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
            .Where(x => !x.isRoot)
            .Distinct()
            .ToList();
        //var individualMotors = new List<float>();
        _ragDollMuscles = GetComponent<Muscles>();


    }

    // Update is called once per frame
    void Update()
    {
        _mapAnim2Ragdoll.OnStep(Time.fixedDeltaTime);

    
       List<Quaternion> targetRotation = _mapAnim2Ragdoll.LastRotation;

        int j = 0;//keeps track of the number of motoros
        foreach (var m in _motors)
        {
            if (m.isRoot)
                continue;

            Vector3 targetNormalizedRotation = Utils.GetSwingTwist(targetRotation[j]);


            _ragDollMuscles.UpdateMotor(m, targetNormalizedRotation, Time.fixedDeltaTime);
           



        }



    }
}
