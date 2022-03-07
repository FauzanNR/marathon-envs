using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Kinematic;
using DReCon;
using Mujoco;

public class MjDReConRewardSource : DReConRewardSource
{
    private void Awake()
    {
        OnAgentInitialize();
    }

    public override void OnAgentInitialize()
    {
        AddColliders(simulationTransform);
        AddColliders(kinematicTransform);
        base.OnAgentInitialize();
        RemoveColliders(simulationTransform);
        RemoveColliders(kinematicTransform);
    }

    public void AddColliders(Transform rootTransform)
    {
        foreach (var inertial in rootTransform.GetComponentsInChildren<MjInertial>())
        {
            Debug.Log(inertial.name);

            if (inertial.transform.parent.GetComponent<MjBody>() == null)
            {
                continue;
            }

            var colObject = inertial.gameObject;
            colObject.transform.SetPositionAndRotation(inertial.transform.position, inertial.transform.rotation);


            var box = colObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
            box.size = inertial.GetBoxSize();

        }
    }

    public void RemoveColliders(Transform rootTransform)
    {
        foreach (var geom in rootTransform.GetComponentsInChildren<MjGeom>())
        {

            if (geom.transform.parent.GetComponent<MjBody>() == null)
            {
                continue;
            }

            var collider = geom.gameObject.GetComponent<Collider>();
            Destroy(collider);

        }
    }

}

    
