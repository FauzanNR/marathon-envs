﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class MarathonTestBedController : MonoBehaviour
{
    [Tooltip("Action applied to each motor")]
    /**< \brief Edit to manually test each motor (+1/-1)*/
    public float[] Actions;

    [Tooltip("Apply a random number to each action each framestep")]
    /**< \brief Apply a random number to each action each framestep*/
    public bool ApplyRandomActions = true;

    //public bool FreezeHead = false;
    public bool FreezeHips = false;
    public bool DontUpdateMotor = false;

    public bool setTpose;


    bool _hasFrozen;




  //  bool tposeanimisloaded = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    void loadTposeanim() {

        GameObject sourceAgent = GameObject.Find("AgentMove-source");

        Animator anim = sourceAgent.GetComponent<Animator>();
        anim.runtimeAnimatorController = null; // Resources.Load("MarathonEnvs/Animations/Tpose") as RuntimeAnimatorController;


        Debug.Log("code commented may make the character not start as desired");
        //TODO: find a replacement for all this pack
        /*
        MocapAnimatorController animControl =  sourceAgent.GetComponent<MocapAnimatorController>();
        animControl.doFixedUpdate = false;
        animControl.MaxForwardVelocity = 0;

        MapAnimationController2RagdollRef animControlartanim = sourceAgent.GetComponent<MapAnimationController2RagdollRef>();
        animControlartanim.doFixedUpdate = false;
        */
        InputController input = FindObjectOfType<InputController>();
        input.DemoMockIfNoInput = false;



    }


    void FreezeBodyParts()
    {

        var marathonAgents = FindObjectsOfType<Agent>(true);
        
        foreach (var agent in marathonAgents)
        {
            ArticulationBody head = null;
            ArticulationBody butt = null;
            ArticulationBody[] children = null;
            switch (agent.name)
            {
                case "MarathonMan":
                    
                    _hasFrozen = true;
                    children = agent.GetComponentsInChildren<ArticulationBody>();
                    head = children.FirstOrDefault(x=>x.name=="torso");
                    butt = children.FirstOrDefault(x=>x.name=="butt");
                    // var rb = children.FirstOrDefault(x=>x.name == "MarathonMan");
                    // if (FreezeHead || FreezeHips)
                    //     rb.constraints = RigidbodyConstraints.FreezeAll;
                    // if (FreezeHead && !FreezeHips)
                    //     rb.GetComponentInChildren<FixedJoint>().connectedBody = head;
                    break;
                case "RagDoll":
                    if (!_hasFrozen && setTpose)
                        loadTposeanim();
                    _hasFrozen = true;
                    children = agent.GetComponentsInChildren<ArticulationBody>();
                    head = children.FirstOrDefault(x=>x.name=="torso");
                    butt = children.FirstOrDefault(x=>x.name=="butt");
                    break;
                case "Ragdoll-MarathonMan004":
                case "MarathonMan004":
                case "MarathonMan004Constrained":
                    if (!_hasFrozen && setTpose)
                        loadTposeanim();
                    _hasFrozen = true;
                    children = agent.GetComponentsInChildren<ArticulationBody>();
                    head = children.FirstOrDefault(x=>x.name=="head");
                    butt = children.FirstOrDefault(x=>x.name=="articulation:Hips");
                    break;
                case "humanoid":
                    _hasFrozen = true;
                    children = agent.GetComponentsInChildren<ArticulationBody>();
                    head = children.FirstOrDefault(x=>x.name=="head");
                    butt = children.FirstOrDefault(x=>x.name=="butt");
                    break;
                default:
                    throw new System.ArgumentException($"agent.name: {agent.name}");
                    //break;
            }
        //    if (FreezeHead && head != null)
        //        head.immovable = true;
            if (FreezeHips && butt != null)
                butt.immovable = true;
        }
    }

    public void OnAgentEpisodeBegin()
    {
        if (!_hasFrozen)
            FreezeBodyParts();
        if (setTpose)
            loadTposeanim();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ApplyRandomActions)
        {
            Actions = Actions.Select(x=>Random.Range(-1f,1f)).ToArray();
        }
    }
}
