using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;

public class TeleportMJ : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    MjBody targetModel;

    [SerializeField]
    MjBody sourceModel;

    [SerializeField]
    Transform teleportTransform;

    [SerializeField]
    Transform animationTransform;

    [SerializeField]
    MjKinematicRig kinematicRig;

    Vector3 posOffset;

    private void Start()
    {
        if (animationTransform != null)
        {
            posOffset =  animationTransform.position - targetModel.transform.position;
        }
    }

    private void  Update()
    {
        Debug.Log(targetModel.transform.position);
        PrintPosFromSim();
        
    }

    public unsafe void  PrintPosFromSim()
    {
        MjScene scene = MjScene.Instance;
        int start = targetModel.MujocoId * 7;
        Debug.Log(new Vector3((float)scene.Data->qpos[start], (float)scene.Data->qpos[start + 2], (float)scene.Data->qpos[start + 1]));
    }

    public void CopyState()
    {
        MjScene mjScene = MjScene.Instance;
        (var positions, var velocities) = mjScene.GetMjKinematics(sourceModel);
        mjScene.SetMjKinematics(targetModel, positions, velocities);
    }

    public void TeleportRoot()
    {
        MjScene mjScene = MjScene.Instance;

        //mjScene.TeleportMjRoot(targetModel.GetComponentInChildren<MjFreeJoint>(), teleportTransform.position, teleportTransform.rotation);

        if(animationTransform != null)
        {
            animationTransform.position = teleportTransform.position + posOffset;
            animationTransform.rotation = teleportTransform.rotation;
        }

        if (kinematicRig != null)
        {
            kinematicRig.TrackKinematics();
        }

        mjScene.TeleportMjRoot(targetModel.GetComponentInChildren<MjFreeJoint>(), teleportTransform.position, teleportTransform.rotation);

/*        if (animationTransform != null)
        {
            animationTransform.position = teleportTransform.position + posOffset;
            animationTransform.rotation = teleportTransform.rotation;
        }

        if (kinematicRig != null)
        {
            kinematicRig.TrackKinematics();
        }*/
    }

    public void RotateRoot()
    {
        MjScene mjScene = MjScene.Instance;
        var root = targetModel.GetComponentInChildren<MjFreeJoint>();
        Debug.Log(root.name);
        mjScene.TeleportMjRoot(root, targetModel.transform.position, Quaternion.Euler(0f, 10f, 0f) * targetModel.transform.localRotation);
    }

}
