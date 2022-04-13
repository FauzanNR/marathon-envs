using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using System;
using System.Linq;

public class MjBallJointInterpreter : MonoBehaviour
{
    [SerializeField]
    Transform mockArm;

    [SerializeField]
    Transform mockThreeAxes;

    [SerializeField]
    List<MjActuator> actuators;

    [SerializeField]
    List<int> order;

    MjBallJoint ballJoint;

    Quaternion defaultRot;
    Quaternion defaultRot2;
    // Start is called before the first frame update
    void Start()
    {
        defaultRot = mockArm.rotation;
        defaultRot2 = mockThreeAxes.rotation;
        ballJoint = GetComponent<MjBallJoint>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        mockArm.rotation =  ballJoint.GetQuaternion() * defaultRot;
        var euler = new Vector3(actuators[0].LengthInDeg(g:0), actuators[1].LengthInDeg(g:1), actuators[2].LengthInDeg(g:2));
        var eulerx = Quaternion.Euler(euler.x, 0, 0);
        var eulery = Quaternion.Euler(0, euler.y, 0);
        var eulerz = Quaternion.Euler(0, 0, euler.z);
        var rotations = new List<Quaternion> { eulerx, eulery, eulerz };
        rotations = rotations.Select((a, i) => new Tuple<Quaternion, int>(a, i)).OrderBy(t => order[t.Item2]).Select(t => t.Item1).ToList();
        var rotation = MjEngineTool.UnityQuaternion(rotations.Aggregate((prod, cur) => cur * prod));


        print(euler);
        mockThreeAxes.rotation = rotation * defaultRot2;
    }
}
