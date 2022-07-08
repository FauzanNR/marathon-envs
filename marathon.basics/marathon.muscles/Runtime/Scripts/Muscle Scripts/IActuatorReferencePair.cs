using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mujoco;
using System.Linq;
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

    public readonly List<IState> actStates;
    public readonly List<IState> refStates;
        public readonly List<int> activeStateIdxs;
    public IEnumerable<IState> activeActStates { get => activeStateIdxs.Select(i => actStates[i]); }
    public IEnumerable<IState> activeRefStates { get => activeStateIdxs.Select(i => refStates[i]); }

    public IEnumerable<int> stateIdxs { get => activeStateIdxs.Select(i => i + dofStartIndex); }
        private int dofStartIndex;

        public PhysXActuatorReferencePair(ArticulationBody act, Rigidbody reference, bool active)
    {
        this.act = act;
        this.reference = new RigidbodyAdapter(reference);
        aba = new ArticulationBodyAdapter(act);
        this.active = active;

            actStates = new List<IState>();
            refStates = new List<IState>();
        actStates.AddRange(GetStates(act));
        refStates.AddRange(GetStates(this.reference, act));
            activeStateIdxs = GetActiveStateIdxs(act);
            List<int> dofStartIndices = new List<int>();
            act.GetDofStartIndices(dofStartIndices);
            dofStartIndex = dofStartIndices[act.index];
    }

    private static IEnumerable<ArticulationBodyState> GetStates(ArticulationBody ab)
        {
            return Enumerable.Range(0, ab.dofCount).Select(i => new ArticulationBodyState(ab, i)).ToList();
        }

    private static IEnumerable<RigidbodyState> GetStates(RigidbodyAdapter rb, ArticulationBody pair)
        {
            return Enumerable.Range(0, pair.dofCount).Select(i => new RigidbodyState(rb, i)).ToList();
        }

    private static List<int> GetActiveStateIdxs(ArticulationBody body)
        {
            List<int> activeStateIdxs = new List<int>();
            if (body.jointType == ArticulationJointType.FixedJoint || body.isRoot) return activeStateIdxs;
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                activeStateIdxs.Add(0);
                return activeStateIdxs;
            }
            if (body.twistLock != ArticulationDofLock.LockedMotion) activeStateIdxs.Add(0);
            if (body.swingYLock != ArticulationDofLock.LockedMotion) activeStateIdxs.Add(1);
            if (body.swingZLock != ArticulationDofLock.LockedMotion) activeStateIdxs.Add(2);
            return activeStateIdxs;
        }

    }

}
