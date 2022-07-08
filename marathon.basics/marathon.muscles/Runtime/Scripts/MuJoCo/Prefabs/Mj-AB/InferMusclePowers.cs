using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System.Linq;
using System;

public class InferMusclePowers : TrainingEventHandler
{

    [SerializeField]
    Transform articulationBodyRoot;

    [SerializeField]
    ArticulationMuscles muscles;

    [SerializeField]
    float constantCtrlLimit;

    public override EventHandler Handler => (object sender, EventArgs args) => Infer();



    // Update is called once per frame
    void Infer()
    {
        Func<float, float> FiltPow = (float pow) => pow == float.MaxValue ? 0 : pow;
        muscles.MusclePowers = articulationBodyRoot.GetComponentsInChildren<ArticulationBody>().Where(x=>!x.isRoot && x.jointType!=ArticulationJointType.FixedJoint).Select(x => new ArticulationMuscles.MusclePower() { Muscle = x.name, PowerVector = new Vector3(FiltPow(x.xDrive.forceLimit) / constantCtrlLimit, FiltPow(x.yDrive.forceLimit) / constantCtrlLimit, FiltPow(x.zDrive.forceLimit)/ constantCtrlLimit) }).ToList();
    }
}
