using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;
using UnityEngine;

using Unity.MLAgents.Policies;


public class ApplyRangeOfMotion004 : MonoBehaviour
{
    [SerializeField]
    bool applyROMInGamePlay;

    public bool ApplyROMInGamePlay {  set => applyROMInGamePlay = value; }



    public RangeOfMotion004 RangeOfMotion2Store;

      [Range(0,359)]
    public int MinROMNeededForJoint = 5;

    public int DegreesOfFreedom = 0;

    [SerializeField]
    bool debugWithLargestROM = false;


    private void OnEnable()
    {
        if (applyROMInGamePlay)
        {
            ApplyRangeOfMotionToRagDoll();


            CalculateDoF();
            applyDoFOnBehaviorParameters();



            if ( (RangeOfMotion2Store != null) && RangeOfMotion2Store.InferenceModel != null)
                GetComponent<BehaviorParameters>().Model = RangeOfMotion2Store.InferenceModel;

        }



    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CalculateDoF()
    {
        if (RangeOfMotion2Store == null || RangeOfMotion2Store.Values.Length == 0)
            return;
        DegreesOfFreedom = 0;
        foreach (var rom in RangeOfMotion2Store.Values)
        {
            if (rom.rangeOfMotion.x > (float)MinROMNeededForJoint)
                DegreesOfFreedom++;
            if (rom.rangeOfMotion.y >= (float)MinROMNeededForJoint)
                DegreesOfFreedom++;
            if (rom.rangeOfMotion.z >= (float)MinROMNeededForJoint)
                DegreesOfFreedom++;
        }
    }

    public void ApplyRangeOfMotionToRagDoll()
    {
        if (RangeOfMotion2Store == null || RangeOfMotion2Store.Values.Length == 0)
            return;
        // var ragdollAgent = GetComponent<RagDollAgent>();
        var articulationBodies = GetComponentsInChildren<ArticulationBody>();
        DegreesOfFreedom = 0;
        foreach (var rom in RangeOfMotion2Store.Values)
        {
            ArticulationBody body = null;


            try
            {
                body = articulationBodies.First(x => x.name == $"articulation:{rom.name}");

            }
           catch (InvalidOperationException e) { Debug.Log("no articulationBody with name: " + rom.name + "Exception: " +e); }
                
                if (body == null)
                return;

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

    }


     


    public void applyDoFOnBehaviorParameters() {
        // due to an obscure function call in the setter of ActionSpec inside bp, this can only run at runtime
        //the function is called SyncDeprecatedActionFields()


        BehaviorParameters bp = GetComponent<BehaviorParameters>();

        Unity.MLAgents.Actuators.ActionSpec myActionSpec = bp.BrainParameters.ActionSpec;



        myActionSpec.NumContinuousActions = DegreesOfFreedom;
        bp.BrainParameters.ActionSpec = myActionSpec;
        Debug.Log("applied DoF on ActionSpec:" + myActionSpec.NumContinuousActions);



    }



}
