using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using ManyWorlds;

public class DebugMarathonController : MonoBehaviour
{
  


    [Tooltip("Apply a random number to each action each framestep, set the character to have no motor update, or imitate a target animation")]
    public DebugModes debugMode;
    public enum DebugModes { 
        randomActions,
        testActionRangeInJoints,
        imitateTargetAnim
       

    }


    [Range(0f, 1f)]
    public float RandomRange = 1f;

    [Tooltip("Action applied to each motor")]
    public float[] Actions;


    [HideInInspector]
    public List<Rigidbody> targets4imitation;
    bool _initialized = false;


    ArticulationBody _root;
    Vector3 _initRootPos;

    public void LazyInitialize(List<ArticulationBody> _motors, Muscles _ragDollMuscles)
    {
        if (!_initialized)
        {
            foreach (var m in _motors)
            {
                TestActionRange dj = m.gameObject.AddComponent<TestActionRange>();
                dj.Init();
            }



           
            Debug.Log("we are in DEBUG mode, we freeze the root of our characters");
             _root = _ragDollMuscles.GetComponentsInChildren<ArticulationBody>()
                .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
                .First(x => x.isRoot);
            //root.immovable = true; //this makes the ragdoll go crazy, so:
            _initRootPos = _root.transform.position;


            


            if (debugMode == DebugMarathonController.DebugModes.imitateTargetAnim)
            {


                List<Rigidbody> rbs = new List<Rigidbody>();
                SpawnableEnv spawnableEnv = _ragDollMuscles.GetComponentInParent<SpawnableEnv>();
                List<Rigidbody> rblistraw = spawnableEnv.GetComponentInChildren<MapAnim2Ragdoll>().GetRigidBodies();



                foreach (var m in _motors)
                {

                    Rigidbody a = rblistraw.Find(x => x.name == m.name);
                    if (a != null)
                        rbs.Add(a);

                }
                targets4imitation = rbs;

            }

            _initialized = true;
        }
    }


    public void ApplyDebugActions(List<ArticulationBody> _motors, Muscles _ragDollMuscles, float actionTimeDelta) {



        LazyInitialize(_motors, _ragDollMuscles);


       // _root.TeleportRoot(_initRootPos, Quaternion.identity);






        switch (debugMode)
        {

            case DebugMarathonController.DebugModes.imitateTargetAnim:
                _ragDollMuscles.MimicRigidBodies(targets4imitation, actionTimeDelta);
                break;
            case DebugMarathonController.DebugModes.randomActions:
                var debugActions = new List<float>();
                foreach (var m in _motors)
                {
                    if (m.isRoot)
                        continue;


                    if (m.jointType != ArticulationJointType.SphericalJoint)
                        continue;
                    if (m.twistLock == ArticulationDofLock.LimitedMotion)
                        debugActions.Add(0);
                    if (m.swingYLock == ArticulationDofLock.LimitedMotion)
                        debugActions.Add(0);
                    if (m.swingZLock == ArticulationDofLock.LimitedMotion)
                        debugActions.Add(0);
                }
                Actions = debugActions.Select(x => Random.Range(-RandomRange, RandomRange)).ToArray();
                _ragDollMuscles.UpdateMuscles(Actions, actionTimeDelta);
                break;
                //the  case  MarathonTestBedController.DebugModes.moveWithDebugJoints is handled directly by the DebugJoints components.

        }

    }



}
