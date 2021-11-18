using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System.Linq;

public class HideRenderers : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> renderersToHide;


    IEnumerable<SkinnedMeshRenderer> SkinnedRenderers;
    IEnumerable<MeshRenderer> MeshRenderers;

    bool currentRenderingState = false;

    // Start is called before the first frame update
    void Start()
    {

        SkinnedRenderers = renderersToHide.Select(rth => rth.GetComponentsInChildren<SkinnedMeshRenderer>(true)).SelectMany(x=>x);
        MeshRenderers = renderersToHide.Select(rth => rth.GetComponentsInChildren<MeshRenderer>(true)).SelectMany(x=>x);

        if(!Academy.Instance.IsCommunicatorOn) return;

        foreach (SkinnedMeshRenderer r in SkinnedRenderers)
            r.enabled = false;

        foreach (MeshRenderer r in MeshRenderers)
            r.enabled = false;
    }

}
