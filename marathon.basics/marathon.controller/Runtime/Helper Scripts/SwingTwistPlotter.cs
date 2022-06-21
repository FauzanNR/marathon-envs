using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingTwistPlotter : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Transform trackedTransform;

    [DebugGUIGraph(min: 0, max: 5, r: 1, g: 0, b: 0, autoScale: true)]
    public float twist;
    [DebugGUIGraph(min: 0, max: 5, r: 0, g: 1, b: 0, autoScale: true)]
    public float swing1;
    [DebugGUIGraph(min: 0, max: 5, r: 0, g: 0, b: 1, autoScale: true)]
    public float swing2;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 decomp = Utils.GetSwingTwist(trackedTransform.localRotation);
        twist = decomp.x;
        swing1 = decomp.y;
        swing2 = decomp.z;
    }
}
