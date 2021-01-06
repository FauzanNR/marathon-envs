using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ROMparserSwingTwist : MonoBehaviour
{

    //we assume a decomposition where the twist is in the X axis.
    //This seems consistent with how ArticulationBody works (see constraints in the inspector)

    [SerializeField]
    Animator theAnimator;

 //   [SerializeField]
 //   Transform sourceAnimationRoot;

    Transform[] joints;

 
    float duration;

 
    [SerializeField]
    public ROMinfoCollector info2store;


    //those are to generate a prefab from a bunch of articulated bodies and the constraints parsed

    //[SerializeField]
    public Transform targetRagdollRoot;


    [Tooltip("Learning Environment where to integrate the constrained ragdoll. Leave blanc if you do not want to generate any training environment")]
    public
    ManyWorlds.SpawnableEnv trainingEnv;



    [Tooltip("Leave blanc if you want to apply on all the children of targetRoot")]
    [SerializeField]
    Transform[] targetJoints;





    // Start is called before the first frame update
    void Start()
    {

        //GameObject o = Instantiate( theReference.gameObject, new Vector3(0, 0, 0), Quaternion.identity);
        //Animator a = o.GetComponent<Animator>();
        //AnimatorStateInfo = a.GetCurrentAnimatorStateInfo();
        //a.Play();

     
        joints = theAnimator.GetComponentsInChildren<Transform>();

        
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
                candidates4storage.x = (candidates4storage.x - 360);
            if (Mathf.Abs(candidates4storage.y - 360) < Mathf.Abs(candidates4storage.y))
                candidates4storage.y = (candidates4storage.y - 360);
            if (Mathf.Abs(candidates4storage.z - 360) < Mathf.Abs(candidates4storage.z))
                candidates4storage.z = (candidates4storage.z - 360);


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
            Debug.Log("First animation played. If there are no more animations, the constraints have been stored. If there are, wait until the ROM info collector file does not update anymore");
        
        }

    }


    //to apply the Range of Motion to MarathonMan004 (the ragdoll made of articulationBodies).
    //This function is called from an Editor Script
    public void ApplyROMAsConstraints()
    {


        //these are the articulationBodies that we want to parse and apply the constraints to
        ArticulationBody[] articulationBodies;


        Transform[] joints = targetJoints;
        //we want them all:
        if (joints.Length == 0)
            joints = targetRagdollRoot.GetComponentsInChildren<Transform>();


        List<ArticulationBody> temp = new List<ArticulationBody>();
        for (int i = 0; i < joints.Length; i++)
        {
            ArticulationBody a = joints[i].GetComponent<ArticulationBody>();
            if (a != null)
                temp.Add(a);
        }

        articulationBodies = temp.ToArray();


        List<string> jNames = new List<string>(info2store.jointNames);

        for (int i = 0; i < articulationBodies.Length; i++)
        {
            string s = articulationBodies[i].name;
            string[] parts = s.Split(':');
            //we assume the articulationBodies have a name structure of hte form ANYNAME:something-in-the-targeted-joint

            int index = -1;

            index = jNames.FindIndex(x => x.Contains(parts[1]));

            if (index < 0)
                Debug.Log("Could not find a joint name matching " + s + "and specifically: " + parts[1]);
            else
            {

                //The swing in one axis:
                ArticulationDrive yboundaries = new ArticulationDrive
                {
                    upperLimit = info2store.maxRotations[index].y,
                    lowerLimit = info2store.minRotations[index].y
                };
                articulationBodies[i].yDrive = yboundaries;


                //The twist in the second axis:
                ArticulationDrive zboundaries = new ArticulationDrive
                {
                    upperLimit = info2store.maxRotations[index].z,
                    lowerLimit = info2store.minRotations[index].z
                };
                articulationBodies[i].zDrive = zboundaries;


                //The twist:
                ArticulationDrive xboundaries = new ArticulationDrive
                {
                    upperLimit = info2store.maxRotations[index].x,
                    lowerLimit = info2store.minRotations[index].x
                };
                articulationBodies[i].xDrive = xboundaries;


                articulationBodies[i].anchorRotation = Quaternion.identity; //the anchor cannot be rotated, otherwise the constraints make no sense

            }






        }


        Debug.Log("applied constraints to: " + targetJoints.Length + " articulationBodies in ragdoll object: " + targetRagdollRoot.name);


    }

    //we assume the constraints have been well applied (see the previous function, Apply ROMAsConstraints)
    //This function is called from an Editor Script
    public void Prepare4PrefabStorage(out RagDollAgent rda, out ManyWorlds.SpawnableEnv envPrefab)
    {

        Transform targetRagdollPrefab = GameObject.Instantiate(targetRagdollRoot);

        //if there is a spawnableEnv, there is a ragdollAgent:
        rda = targetRagdollPrefab.GetComponent<RagDollAgent>();
        if (rda != null)
            Debug.Log("Setting up the  ragdoll agent");
        
        envPrefab = null;


        


        //these are all the articulationBodies in the ragdoll prefab
        ArticulationBody[] articulationBodies;

        Transform[] joints = targetRagdollPrefab.GetComponentsInChildren<Transform>();


        List<ArticulationBody> temp = new List<ArticulationBody>();
        for (int i = 0; i < joints.Length; i++)
        {
            ArticulationBody a = joints[i].GetComponent<ArticulationBody>();
            if (a != null)
                temp.Add(a);
        }

        articulationBodies = temp.ToArray();


        //We also prepare the stuff inside the Marathon Classes:





        for (int i = 0; i < articulationBodies.Length; i++)
        {
            articulationBodies[i].transform.localRotation = Quaternion.identity;
            if (articulationBodies[i].isRoot) {
                articulationBodies[i].immovable = false;
                if(rda != null)
                    rda.CameraTarget = articulationBodies[i].transform;


                if (trainingEnv)
                {
                    envPrefab = GameObject.Instantiate(trainingEnv);

                    //we assume the environment has an animated character, and in this there is a son which is the root of a bunch of rigidBodies forming a humanoid.
                    //TODO: replace this with something that creates the rigidBody humanoid such a thing procedurally
                    activateMarathonManTarget(envPrefab);




                    if (rda != null) { 
                        rda.transform.parent = envPrefab.transform;
                        rda.name = targetRagdollRoot.name;
                        rda.enabled = true;//only when the animation source is a son of the SpawnableEnv, or it does not find the MocapControllerArtanim when it initializes
                    }
                    RagdollControllerArtanim agentOutcome = envPrefab.GetComponentInChildren<RagdollControllerArtanim>(true);
                    if (agentOutcome != null)
                    {
                        agentOutcome.gameObject.SetActive(true);
                        //agentOutcome.enabled = true;
                        agentOutcome.ArticulationBodyRoot = articulationBodies[i];
                    }
                }
            }

        }
        
    }



    static void activateMarathonManTarget(ManyWorlds.SpawnableEnv env)
    {
        Animator target = env.transform.GetComponentInChildren<Animator>();

        Transform[] rbs = target.GetComponentsInChildren<Transform>(true);


        //Rigidbody[] rbs = target.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].gameObject.SetActive(true);

        }






    }



}
