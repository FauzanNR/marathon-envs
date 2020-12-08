using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ROMparserSwingTwist : MonoBehaviour
{

    //we assume a decomposition where the twist is in the X axis.
    //This seems consistent with how ArticulationBody works (see constraints in the inspector)

    [SerializeField]
    Animator theAnimator;

    [SerializeField]
    Transform theRoot;

    Transform[] joints;

 
    float duration;

 
    [SerializeField]
    ROMinfoCollector info2store;


    // Start is called before the first frame update
    void Start()
    {

        //GameObject o = Instantiate( theReference.gameObject, new Vector3(0, 0, 0), Quaternion.identity);
        //Animator a = o.GetComponent<Animator>();
        //AnimatorStateInfo = a.GetCurrentAnimatorStateInfo();
        //a.Play();

       joints = theRoot.GetComponentsInChildren<Transform>();


        info2store.maxRotations = new Vector3[joints.Length];
        info2store.minRotations = new Vector3[joints.Length];
        info2store.jointNames = new string[joints.Length];





        for (int i = 0; i < joints.Length; i++)
        {
            info2store.jointNames[i] = joints[i].name;


        }

            AnimatorClipInfo[] info = theAnimator.GetCurrentAnimatorClipInfo(0);
        AnimationClip theClip = info[0].clip;
        duration = theClip.length;
        Debug.Log("The animation " + theClip.name + " has a duration of: " + duration);


    }
    



    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i< joints.Length; i++) {


            Quaternion localRotation = joints[i].localRotation;

            //the decomposition in swing-twist, typically works like this:
            Quaternion swing = new Quaternion(0.0f, localRotation.y, localRotation.z, localRotation.w);
            swing = swing.normalized;

            //Twist: assuming   q_localRotation = q_swing * q_twist 



            Quaternion twist =  Quaternion.Inverse(swing) * localRotation ;


            //double check:
            Quaternion temp = swing * twist ;

            bool isTheSame = (Mathf.Abs(Quaternion.Angle(temp, localRotation)) < 0.001f);


            if (!isTheSame)
                Debug.LogError("In joint " + gameObject.name + "I have: " + temp + "which does not match: " + localRotation + "because their angle is: " + Quaternion.Angle(temp, localRotation));


            Vector3 candidates4storage = new Vector3(twist.eulerAngles.x, swing.eulerAngles.y, swing.eulerAngles.z);

            //we make sure we keep the values nearest to 0 (with a modulus)
            if (Mathf.Abs(candidates4storage.x - 360) < Mathf.Abs(candidates4storage.x))
                candidates4storage.x = Mathf.Abs(candidates4storage.x - 360);
            if (Mathf.Abs(candidates4storage.y - 360) < Mathf.Abs(candidates4storage.y))
                candidates4storage.y = Mathf.Abs(candidates4storage.y - 360);
            if (Mathf.Abs(candidates4storage.z - 360) < Mathf.Abs(candidates4storage.z))
                candidates4storage.z = Mathf.Abs(candidates4storage.z - 360);


            if (info2store.maxRotations[i].x < candidates4storage.x)
                info2store.maxRotations[i].x = candidates4storage.x;
            if (info2store.maxRotations[i].y < candidates4storage.y)
                info2store.maxRotations[i].y = candidates4storage.y;
            if (info2store.maxRotations[i].z < candidates4storage.z)
                info2store.maxRotations[i].z = candidates4storage.z;


            if (info2store.minRotations[i].x > candidates4storage.x)
                info2store.minRotations[i].x = candidates4storage.x;
            if (info2store.minRotations[i].y > candidates4storage.y)
                info2store.minRotations[i].y = candidates4storage.y;
            if (info2store.minRotations[i].z > candidates4storage.z)
                info2store.minRotations[i].z = candidates4storage.z;



        }


        if (duration < Time.time)
        {
            Debug.Log("animation played");
            //Application.Quit();

        }

    }

   
}
