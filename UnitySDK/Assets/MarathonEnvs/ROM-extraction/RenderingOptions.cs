using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingOptions : MonoBehaviour
{

    [SerializeField]
    bool renderOnlyTarget;

    [SerializeField]
    GameObject movementsource;
    [SerializeField] 
    GameObject ragdollcontroller;

    SkinnedMeshRenderer[] SkinnedRenderers;
    MeshRenderer[] MeshRenderers;
    MeshRenderer[] MeshRenderersRagdoll;

    bool currentRenderingState = false;

    // Start is called before the first frame update
    void Start()
    {
        SkinnedRenderers = movementsource.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        MeshRenderers = movementsource.GetComponentsInChildren<MeshRenderer>(true);
        MeshRenderersRagdoll = ragdollcontroller.GetComponentsInChildren<MeshRenderer>(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (renderOnlyTarget != currentRenderingState)
        {
            currentRenderingState = renderOnlyTarget;
            if (renderOnlyTarget)
            {
                foreach (SkinnedMeshRenderer r in SkinnedRenderers)
                    r.enabled = false;

                foreach (MeshRenderer r in MeshRenderers)
                    r.enabled = false;

                foreach (MeshRenderer r in MeshRenderersRagdoll)
                    r.enabled = false;

            }
            else {

                foreach (SkinnedMeshRenderer r in SkinnedRenderers)
                    r.enabled = true;

                foreach (MeshRenderer r in MeshRenderers)
                    r.enabled = true;

                foreach (MeshRenderer r in MeshRenderersRagdoll)
                    r.enabled = true;


            }

        }
    }
}
