using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ROMparser : MonoBehaviour
{
    [SerializeField]
    Animator theAnimator;

    [SerializeField]
    Transform theRoot;

    Transform[] joints;


    //this is just to check if putting the data between -180 and +180 gives a smaller range of motion.
    Vector3[] maxRotations2;
    Vector3[] minRotations2;


    int frameCounter = 0;


    float duration;

    bool done = false;

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


        maxRotations2 = new Vector3[joints.Length];
        minRotations2 = new Vector3[joints.Length];




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

            if (info2store.maxRotations[i].x < joints[i].rotation.eulerAngles.x)
                info2store.maxRotations[i].x = joints[i].rotation.eulerAngles.x;
            if (info2store.maxRotations[i].y < joints[i].rotation.eulerAngles.y)
                info2store.maxRotations[i].y = joints[i].rotation.eulerAngles.y;
            if (info2store.maxRotations[i].z < joints[i].rotation.eulerAngles.z)
                info2store.maxRotations[i].z = joints[i].rotation.eulerAngles.z;


            if (info2store.minRotations[i].x > joints[i].rotation.eulerAngles.x)
                info2store.minRotations[i].x = joints[i].rotation.eulerAngles.x;
            if (info2store.minRotations[i].y > joints[i].rotation.eulerAngles.y)
                info2store.minRotations[i].y = joints[i].rotation.eulerAngles.y;
            if (info2store.minRotations[i].z > joints[i].rotation.eulerAngles.z)
                info2store.minRotations[i].z = joints[i].rotation.eulerAngles.z;


            //info2store.allRotVals[i][frameCounter] = joints[i].rotation.eulerAngles;
            Vector3 temp = joints[i].rotation.eulerAngles;

            if (temp.x > 180)
                temp.x -= 360;
            if (temp.y > 180)
                temp.y -= 360;
            if (temp.z > 180)
                temp.z -= 360;


            if (maxRotations2[i].x < temp.x)
                maxRotations2[i].x = temp.x;
            if (maxRotations2[i].y < temp.y)
                maxRotations2[i].y = temp.y;
            if (maxRotations2[i].z < temp.z)
                maxRotations2[i].z = temp.z;


            if (minRotations2[i].x > temp.x)
                minRotations2[i].x = temp.x;
            if (minRotations2[i].y > temp.y)
                minRotations2[i].y = temp.y;
            if (minRotations2[i].z > temp.z)
                minRotations2[i].z = temp.z;


        }
        //info2store.maxRotations = maxRotations;
        //info2store.minRotations = minRotations;

        if (duration < Time.time) { 
            Debug.Log("animation played");
            //Application.Quit();

            if (done == false)
            {
                done = true;
                for (int i = 0; i < joints.Length; i++)
                {

                    if ((maxRotations2[i].x - minRotations2[i].x) < (info2store.maxRotations[i].x - info2store.minRotations[i].x)){

                    //    Debug.Log("case " + i + " maxRotations2[i].x: " + maxRotations2[i].x + "minRotations2[i].x : " + minRotations2[i].x);

                    //    Debug.Log("case " + i +  " info2store.maxRotations[i].x: " + info2store.maxRotations[i].x + "info2store.minRotations[i].x : " + info2store.minRotations[i].x);

                        info2store.maxRotations[i].x = maxRotations2[i].x;
                        info2store.minRotations[i].x = minRotations2[i].x;
                        
                    }


                    if ((maxRotations2[i].y - minRotations2[i].y) < (info2store.maxRotations[i].y - info2store.minRotations[i].y))
                    {


                        //Debug.Log("case " + i + " maxRotations2[i].y: " + maxRotations2[i].y + "minRotations2[i].y : " + minRotations2[i].y);

                        //Debug.Log("case " + i + " info2store.maxRotations[i].y: " + info2store.maxRotations[i].y + "info2store.minRotations[i].y : " + info2store.minRotations[i].y);


                        info2store.maxRotations[i].y = maxRotations2[i].y;
                        info2store.minRotations[i].y = minRotations2[i].y;

                    }


                    if ((maxRotations2[i].z - minRotations2[i].z) < (info2store.maxRotations[i].z - info2store.minRotations[i].z))
                    {
                        info2store.maxRotations[i].z = maxRotations2[i].z;
                        info2store.minRotations[i].z = minRotations2[i].z;

                    }













                }




            }

        }


    }

   
}
