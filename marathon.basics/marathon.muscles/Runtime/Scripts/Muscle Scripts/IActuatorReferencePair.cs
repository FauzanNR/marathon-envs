using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mujoco;
using Kinematic;

namespace MotorUpdate { 

public interface IActuatorReferencePair 
{



}




public class MujocoActuatorReferencePair :IActuatorReferencePair
{
    public readonly MjActuator act;
    public readonly MjHingeJoint reference;
    public readonly bool active;

    public MujocoActuatorReferencePair(MjActuator act, MjHingeJoint reference, bool active)
    {
        this.act = act;
        this.reference = reference;
        this.active = active;
    }
}

public class PhysXActuatorReferencePair : IActuatorReferencePair
{
    public readonly ArticulationBody act;
    public readonly RigidbodyAdapter reference;
    public readonly bool active;

    public readonly ArticulationBodyAdapter aba;

    public PhysXActuatorReferencePair(ArticulationBody act, Rigidbody reference, bool active)
    {
        this.act = act;
        this.reference = new RigidbodyAdapter(reference);
        aba = new ArticulationBodyAdapter(act);
        this.active = active;
    }


    }

}
