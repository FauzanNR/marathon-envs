using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestJointRange : MonoBehaviour
{
    public float SphereSize = 0.03f;

    static Color[] _axisColor = { 
        new Color(219f / 255, 62f / 255, 29f / 255, .93f), 
        new Color(154f / 255, 243f / 255, 72f / 255, .93f), 
        new Color(58f / 255, 122f / 255, 248f / 255, .93f)};
    static Vector3[] _axisVector = { Vector3.right, Vector3.up, Vector3.forward };
    ArticulationBody _body;
  
    DebugMarathonController _debugController;
    
    ManyWorlds.SpawnableEnv _spawnableEnv;

    [Tooltip("When Debuging with TestActionRange, use this to try the range of motion of the articulated bodies")]
    
    [Range(0,1)]
    public float normalizedTwistX;
    [Range(0, 1)]
    public float normalizedSwingY;
    [Range(0, 1)]
    public float normalizedSwingZ;
        
      


    [Space(20)]


    Muscles muscles;


   
    void Start()
    {
        Init();

    }
    public void Init()
    { 

        _body = GetComponent<ArticulationBody>();
       
        _debugController = FindObjectOfType<DebugMarathonController>();
        _spawnableEnv = GetComponentInParent<ManyWorlds.SpawnableEnv>();

        muscles = _spawnableEnv.GetComponentInChildren<Muscles>();

    }




    // Update is called once per frame
    void FixedUpdate()
    {
        if (_body == null)
            return;

        muscles.UpdateMotorPDWithVelocity(_body, new Vector3(normalizedTwistX,normalizedSwingY,normalizedSwingZ), Time.fixedDeltaTime);
         



    }
   
    void OnDrawGizmos()
    //void OnDrawGizmosSelected()
    {
        if (_body == null)
            return;
        Gizmos.color = Color.white;
        Vector3 position = _body.transform.TransformPoint(_body.anchorPosition);
        Quaternion rotation = _body.transform.rotation * _body.anchorRotation;


        for (int i = 0; i < _axisColor.Length; i++)
        {
            var axisColor = _axisColor[i];
            var axis = _axisVector[i];
            Gizmos.color = axisColor;
            // Vector3 rotationEul = _body.transform.TransformDirection(_body.anchorRotation * axis);
            Vector3 rotationEul = rotation * axis;
            Gizmos.DrawSphere(position, SphereSize);
            Vector3 direction = rotationEul;
            Gizmos.DrawRay(position, direction);
            
        }
    }
}
