using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;
using UnityEngine;

using Unity.MLAgents.Policies;
using Unity.MLAgents;

public class ApplyRangeOfMotion004 : MonoBehaviour
{

    /*
    [SerializeField]
    bool applyROMInGamePlay;

    public bool ApplyROMInGamePlay {  set => applyROMInGamePlay = value; }
    */


    public RangeOfMotion004 RangeOfMotion2Store;

      [Range(0,359)]
    public int MinROMNeededForJoint = 5;

   // [Tooltip("the space of actions")]
   // public int DegreesOfFreedom = 0;

  //  [Tooltip("the space of observations")]
  //  public int ObservationDimensions = 0;



    [SerializeField]
    bool debugWithLargestROM = false;



    /*//NOT USED
    int CalculateDoF()
    {


        if (RangeOfMotion2Store == null || RangeOfMotion2Store.Values.Length == 0)
            return -1;
        int DegreesOfFreedom = 0;
        foreach (var rom in RangeOfMotion2Store.Values)
        {
            if (rom.rangeOfMotion.x > (float)MinROMNeededForJoint)
                DegreesOfFreedom++;
            if (rom.rangeOfMotion.y >= (float)MinROMNeededForJoint)
                DegreesOfFreedom++;
            if (rom.rangeOfMotion.z >= (float)MinROMNeededForJoint)
                DegreesOfFreedom++;
        }

        return DegreesOfFreedom;

    }*/

    public void ConfigureTrainingForRagdoll() {
       int dof = ApplyRangeOfMotionToRagDoll();
        if (dof == -1)
        {
            Debug.LogError("Problems applying the range of motion to the ragdoll");
        }
        else {

            ApplyDoFOnBehaviorParameters(dof);
        }

    }


    int ApplyRangeOfMotionToRagDoll()
    {
        if (RangeOfMotion2Store == null || RangeOfMotion2Store.Values.Length == 0)
            return -1;
        // var ragdollAgent = GetComponent<RagDollAgent>();
        ArticulationBody[] articulationBodies = GetComponentsInChildren<ArticulationBody>(true);


        //we only keep the ones that have colliders (and therefore do not correspond to joints).
        List<ArticulationBody> listofAB = new List<ArticulationBody>();
        foreach (ArticulationBody ab in articulationBodies) {
            if (ab.GetComponent<Collider>() == null)
                listofAB.Add(ab);

        }
        articulationBodies = listofAB.ToArray();

        int DegreesOfFreedom = 0;

        //below does not work because there are more Values than articulationBody (for example, fingers)
        //foreach (var rom in RangeOfMotion2Store.Values)

        //foreach (var rom in RangeOfMotion2Store.Values)
        foreach(ArticulationBody body in articulationBodies)
        {

            //string tname = temp[1];
            string keyword = "articulation:";
            string valuename = body.name.TrimStart(keyword.ToArray<char>());


            RangeOfMotionValue rom = RangeOfMotion2Store.Values.First(x => x.name == valuename);

            if(rom == null)
                {
                Debug.LogError("Could not find a rangoe of motionvalue for articulation: " + body.name);
                return -1;
                }

            bool isLocked = true;
            body.twistLock = ArticulationDofLock.LockedMotion;
            body.swingYLock = ArticulationDofLock.LockedMotion;
            body.swingZLock = ArticulationDofLock.LockedMotion;
            body.jointType = ArticulationJointType.FixedJoint;

            body.anchorRotation = Quaternion.identity; //we make sure the anchor has no Rotation, otherwise the constraints do not make any sense

            if (rom.rangeOfMotion.x > (float)MinROMNeededForJoint)
            {
                DegreesOfFreedom++;
                isLocked=false;
                body.twistLock = ArticulationDofLock.LimitedMotion;
                var drive = body.xDrive;
                drive.lowerLimit = rom.lower.x;
                drive.upperLimit = rom.upper.x;
                body.xDrive = drive;
                if (debugWithLargestROM) {
                    drive.lowerLimit = -170;
                    drive.upperLimit = +170;
                }

            }
            if (rom.rangeOfMotion.y >= (float)MinROMNeededForJoint)
            {
                DegreesOfFreedom++;
                isLocked=false;
                body.swingYLock = ArticulationDofLock.LimitedMotion;
                var drive = body.yDrive;
                drive.lowerLimit = rom.lower.y;
                drive.upperLimit = rom.upper.y;
                body.yDrive = drive;

                if (debugWithLargestROM)
                {
                    drive.lowerLimit = -170;
                    drive.upperLimit = +170;
                }


            }
            if (rom.rangeOfMotion.z >= (float)MinROMNeededForJoint)
            {
                DegreesOfFreedom++;
                isLocked=false;
                body.swingZLock = ArticulationDofLock.LimitedMotion;
                var drive = body.zDrive;
                drive.lowerLimit = rom.lower.z;
                drive.upperLimit = rom.upper.z;
                body.zDrive = drive;

                if (debugWithLargestROM)
                {
                    drive.lowerLimit = -170;
                    drive.upperLimit = +170;
                }

            }

            if (!isLocked)
            {
                body.jointType = ArticulationJointType.SphericalJoint;
            }

        }

        return DegreesOfFreedom;

    }


     


    void ApplyDoFOnBehaviorParameters(int DegreesOfFreedom) {
        // due to an obscure function call in the setter of ActionSpec inside bp, this can only run at runtime
        //the function is called SyncDeprecatedActionFields()


        BehaviorParameters bp = GetComponent<BehaviorParameters>();

        Unity.MLAgents.Actuators.ActionSpec myActionSpec = bp.BrainParameters.ActionSpec;



        myActionSpec.NumContinuousActions = DegreesOfFreedom;
        bp.BrainParameters.ActionSpec = myActionSpec;
        Debug.Log("Space of actions calculated at:" + myActionSpec.NumContinuousActions + " continuous dimensions");


        /*
         * To calculate the space of observations, apparently the formula is:
        number of colliders(19) *12 +number of actions(54) + number of sensors(6) + misc addition observations(16) = 304
        correction: it seeems to be:
        number of actions + number of sensors + misc additional observations = 101
        
        */

        //int numcolliders = GetComponentsInChildren<CapsuleCollider>().Length; //notice the sensors are Spherecolliders, so not included in this count
        int numsensors = GetComponentsInChildren<SensorBehavior>().Length;
        int num_miscelaneous = GetComponent<RagDollAgent>().calculateDreConObservationsize();

        int ObservationDimensions =  DegreesOfFreedom + numsensors + num_miscelaneous;
        bp.BrainParameters.VectorObservationSize = ObservationDimensions;
        Debug.Log("Space of perceptions calculated at:" + bp.BrainParameters.VectorObservationSize + " continuous dimensions, with: " + "sensors: " + numsensors + "and DreCon miscelaneous: " + num_miscelaneous);


    }



}
