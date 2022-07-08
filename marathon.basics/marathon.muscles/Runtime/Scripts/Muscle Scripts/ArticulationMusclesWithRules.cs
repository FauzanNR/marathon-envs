using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using MotorUpdate;




using Kinematic;

public class ArticulationMusclesWithRules : ModularMuscles
{
    /*Assumptions in this model:
        * 1. We provide a kinematic reference directly to the PD controllers
        * 2. The action array trained has per main purpose to learn  what to "add" to  the kinematic reference to keep balance
        */

    [SerializeField]
    ArticulationBody root;


    [SerializeField]
    Transform kinematicRef;

    //we need 6 extra zeros to apply nothing to the root Articulation when we do apply actions
    float[] nullactions4root = new float[6] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };


    protected IReadOnlyList<PhysXActuatorReferencePair> actuatorPairs;
    public virtual IReadOnlyList<IKinematic> Actuators { get => Utils.GetArticulationMotors(root.gameObject); }


    public override void OnAgentInitialize()
    {

        base.OnAgentInitialize();

        //   IReadOnlyList<ArticulationBody> subset = actuatorSubset == null ? new List<ArticulationBody> { } : actuatorSubset;
        //   actuatorPairs = Actuators.Select(a => new ActuatorReferencePair(a, FindReference(a), subset.Contains(a))).ToList();
        IReadOnlyList<ArticulationBody> subset = new List<ArticulationBody>();
        //                actuatorPairs = Actuators.OrderBy(act => act.index).Select(a => new PhysXActuatorReferencePair(a, FindReference(a), subset.Contains(a))).ToList();

        //  actuatorPairs = Actuators.OrderBy(act => act.index).Select(a => new PhysXActuatorReferencePair(a.gameObject.GetComponent<ArticulationBody>(), FindReference(a.gameObject.GetComponent<ArticulationBody>()), subset.Contains(a.gameObject.GetComponent<ArticulationBody>()))).ToList();
        actuatorPairs = Actuators.OrderBy(act => act.index).Select(a => new PhysXActuatorReferencePair(a.gameObject.GetComponent<ArticulationBody>(), FindReference(a.gameObject.GetComponent<ArticulationBody>()), true)).ToList();


        DebugIndices();
    }


    private Rigidbody FindReference(ArticulationBody act)
    {
        return kinematicRef ? kinematicRef.GetComponentsInChildren<Rigidbody>().First(rj => rj.name.Contains(act.name)) : null;
    }




    public override int ActionSpaceSize
    {
        get => GetActionsFromState().Length;
    }



    public override float[] GetActionsFromState()
    {
        var vectorActions = new List<float>();
        if (actuatorPairs != null) {
            foreach (var actupair in actuatorPairs)
            {

                var m = actupair.aba;
                if (m.isRoot)
                    continue;
                int i = 0;

                if (m.JointAxes.c0[0] != 0)
                {

                    var target = m.JointPosition[0];
                    vectorActions.Add(target);
                }

                if (m.JointAxes.c1[1] != 0)
                {

                    var target = m.JointPosition[1];
                    vectorActions.Add(target);
                }

                if (m.JointAxes.c1[1] != 0)
                {

                    var target = m.JointPosition[2];
                    vectorActions.Add(target);
                }
            }

        }
        else
        {
            var tempPairs = Actuators.OrderBy(act => act.index).Select(a => new PhysXActuatorReferencePair(a.gameObject.GetComponent<ArticulationBody>(), FindReference(a.gameObject.GetComponent<ArticulationBody>()), true)).ToList();

            foreach (var actupair in tempPairs)
            {
                var m = actupair.aba;
                if (m.isRoot)
                    continue;
                int i = 0;

                if (m.JointAxes.c0[0] != 0)
                {

                    var target = m.JointPosition[0];
                    vectorActions.Add(target);
                }

                if (m.JointAxes.c1[1] != 0)
                {

                    var target = m.JointPosition[1];
                    vectorActions.Add(target);
                }

                if (m.JointAxes.c1[1] != 0)
                {

                    var target = m.JointPosition[2];
                    vectorActions.Add(target);
                }
            }
        }
        return vectorActions.ToArray();
    }



    public override void ApplyActions(float[] actions)
    {

        //Since these are static, we could make them fields of the behaviour as well.
        IState[] currentStates = actuatorPairs.SelectMany(arp => arp.activeActStates).ToArray();
        IState[] targetStates = actuatorPairs.SelectMany(arp => arp.activeRefStates).ToArray();

        int j = 0;

        List<float> jointForces = new List<float>();
        actuatorPairs.First().act.GetJointForces(jointForces);
        int dofCount = 0;
        List<List<int>> activeIdxs = new List<List<int>>();

        List<float> torques = updateRule.GetJointForces(currentStates.ToArray(), targetStates.ToArray());

        //Could be field as well
        int[] dofIndices = actuatorPairs.SelectMany(arp => arp.stateIdxs).ToArray();
        foreach((float force, int idx) in torques.Zip(dofIndices, Tuple.Create))
        {
            jointForces[idx] = force;
        }

        
        root.SetJointForces(jointForces);

    }




    void DebugIndices() {

        List<int> vectorTorquesIndices = new List<int>();


        List<float> currentForces = new List<float>();


        ArticulationBody myRootAB = root;
        int totalDOF = myRootAB.GetJointForces(currentForces);

        myRootAB.GetDofStartIndices(vectorTorquesIndices);


        string s = " totalDOF: " + totalDOF + " indices: ";
        for (int index = 0; index < vectorTorquesIndices.Count; index++)
        {

            s += vectorTorquesIndices[index] + " ";
        }

        Debug.Log(s);


        int i = 0;
        foreach (PhysXActuatorReferencePair actPair in actuatorPairs)
        {
           
            ArticulationBody abtemp = actPair.act;
            Debug.Log("ActuatorPair:" + i + "articulation: " + abtemp.name + " index: " + abtemp.index + "  " + vectorTorquesIndices[abtemp.index] + " " + abtemp.dofCount);
            i++;

        }



    }













}
