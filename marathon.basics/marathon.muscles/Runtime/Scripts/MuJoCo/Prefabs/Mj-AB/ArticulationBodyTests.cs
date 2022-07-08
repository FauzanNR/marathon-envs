using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulationBodyTests : MonoBehaviour
{
    ArticulationBody ab;

    [SerializeField]
    ArticulationBody monitoredAB;
    // Start is called before the first frame update
    void Start()
    {
        ab = GetComponent<ArticulationBody>();

        print(ab.GetJointForces(new List<float>()));
    }

    // Update is called once per frame
    void Update()
    {
        print(monitoredAB.twistLock);
        print(monitoredAB.swingZLock);
        print(monitoredAB.jointPosition[0]);
    }
}
