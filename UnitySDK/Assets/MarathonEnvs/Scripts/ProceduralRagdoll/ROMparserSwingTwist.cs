using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using System.Linq;


public class ROMparserSwingTwist : MonoBehaviour
{

    //we assume a decomposition where the twist is in the X axis.
    //This seems consistent with how ArticulationBody works (see constraints in the inspector)

    public
    Animator theAnimator;

    public
    Transform skeletonRoot;

    Transform[] joints;


    float duration;


    [SerializeField]
    //public ROMinfoCollector info2store;
    public RangeOfMotion004 info2store;

    //those are to generate a prefab from a bunch of articulated bodies and the constraints parsed

    //[SerializeField]
    public ArticulationBody targetRagdollRoot;


    [Tooltip("Learning Environment where to integrate the constrained ragdoll. Leave blanc if you do not want to generate any training environment")]
    public
    ManyWorlds.SpawnableEnv trainingEnv;



    [Tooltip("Leave blanc if you want to apply on all the children of targetRoot")]
    [SerializeField]
    ArticulationBody[] targetJoints;

    string animationEndMessage = "First animation played.If there are no more animations, the constraints have been stored.If there are, wait until the ROM info collector file does not update anymore";

    //[SerializeField]

    //public RangeOfMotion004 RangeOfMotion2Store;

    // public ROMinfoCollector RangeOfMotionStored;

    [SerializeField]

    public RangeOfMotionValue[] RangeOfMotionPreview;

    MapAnimationController2RagdollRef _mocapControllerArtanim;
    Vector3 _rootStartPosition;
    Quaternion _rootStartRotation;

    public bool MimicMocap;
    [Range(0, 359)]
    public int MaxROM = 180;

    [Range(0, 500)]
    public int MimicSkipPhysicsSteps = 50;
    int _physicsStepsToNextMimic = 0;
    public float stiffness = 40000f;
    public float damping = 0f;
    public float forceLimit = float.MaxValue;



    // Start is called before the first frame update
    void Start()
    {

        //GameObject o = Instantiate( theReference.gameObject, new Vector3(0, 0, 0), Quaternion.identity);
        //Animator a = o.GetComponent<Animator>();
        //AnimatorStateInfo = a.GetCurrentAnimatorStateInfo();
        //a.Play();


        joints = skeletonRoot.GetComponentsInChildren<Transform>();


        foreach (Transform j in joints) {
            info2store.addJoint(j);

        }

        //info2store.Values = new RangeOfMotionValue[joints.Length];
        //info2store.maxRotations = new Vector3[joints.Length];
        //info2store.minRotations = new Vector3[joints.Length];
        //info2store.jointNames = new string[joints.Length];





    

        try
        {

            AnimatorClipInfo[] info = theAnimator.GetCurrentAnimatorClipInfo(0);
            AnimationClip theClip = info[0].clip;
            duration = theClip.length;
            Debug.Log("The animation " + theClip.name + " has a duration of: " + duration);

        }
        catch (Exception e) {
            Debug.Log("the character does not seem to have an animator. Make sure it is moving in some way to extract the Range of Motion");

        }


        _mocapControllerArtanim = theAnimator.GetComponent<MapAnimationController2RagdollRef>();

        // get root start position and rotation
        var articulationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();
        if (articulationBodies.Length == 0)
            return;
        var root = articulationBodies.First(x => x.isRoot);
        _rootStartPosition = root.transform.position;
        _rootStartRotation = root.transform.rotation;

        // if no joints specified get joints using
        // not root (is static), begins with 'articulation:'
        if (targetJoints.Length == 0)
        {
            targetJoints = targetRagdollRoot
                .GetComponentsInChildren<ArticulationBody>()
                .Where(x => x.isRoot == false)
                .Where(x => x.name.StartsWith("articulation:"))
                .ToArray();
            SetJointsToMaxROM();
        }
    }


    void CopyMocap()
    {
        if (_mocapControllerArtanim != null &&
            targetRagdollRoot != null &&
            _mocapControllerArtanim.enabled)
        {
            var atriculationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();
            if (atriculationBodies.Length == 0)
                return;
            var root = atriculationBodies.First(x => x.isRoot);
            CopyMocapStatesTo(root.gameObject, _rootStartPosition);
            // teleport back to start position
            // var curRotation = root.transform.rotation;
            // root.TeleportRoot(_rootStartPosition, curRotation);
            // Vector3 offset = _rootStartPosition - root.transform.position;
            // root.gameObject.SetActive(false);
            // foreach (var t in root.GetComponentsInChildren<Transform>())
            // {
            //     t.position = t.position + offset;
            // }
            // root.transform.position = _rootStartPosition;
            // root.gameObject.SetActive(true);

            // foreach (var body in atriculationBodies)
            // {
            //     if (body.twistLock == ArticulationDofLock.LimitedMotion)
            //     {
            //         var xDrive = body.xDrive;
            //         List<float> targets = new List<float>();
            //         var bb = body.GetDriveTargets(targets);
            //         var cc = 22;
            //     }
            // }
        }
    }
    void CopyMocapStatesTo(GameObject target, Vector3 rootPosition)
    {

        var targets = target.GetComponentsInChildren<ArticulationBody>().ToList();
        if (targets?.Count == 0)
            return;
        var root = targets.First(x => x.isRoot);
        root.gameObject.SetActive(false);
        var mocapRoot = _mocapControllerArtanim.GetComponentsInChildren<Rigidbody>().First(x => x.name == root.name);
        Vector3 offset = rootPosition - mocapRoot.transform.position;
        foreach (var body in targets)
        {
            var stat = _mocapControllerArtanim.GetComponentsInChildren<Rigidbody>().First(x => x.name == body.name);
            body.transform.position = stat.position + offset;
            body.transform.rotation = stat.rotation;
            if (body.isRoot)
            {
                body.TeleportRoot(stat.position + offset, stat.rotation);
            }
        }
        root.gameObject.SetActive(true);
        foreach (var body in targets)
        {
            // body.AddForce(new Vector3(0.1f, -200f, 3f));
            // body.AddTorque(new Vector3(0.1f, 200f, 3f));
            body.velocity = (new Vector3(0.1f, 4f, .3f));
            body.angularVelocity = (new Vector3(0.1f, 20f, 3f));
        }
    }
    void Update()
    {
        for (int i = 0; i < joints.Length; i++)
        {


            Quaternion localRotation = joints[i].localRotation;

            //the decomposition in swing-twist, typically works like this:
            Quaternion swing = new Quaternion(0.0f, localRotation.y, localRotation.z, localRotation.w);
            swing = swing.normalized;

            //Twist: assuming   q_localRotation = q_swing * q_twist 



            Quaternion twist = Quaternion.Inverse(swing) * localRotation;


            //double check:
            Quaternion temp = swing * twist;

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
            /*
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
            */


            if (info2store.Values[i].upper.x < candidates4storage.x)
                info2store.Values[i].upper.x = candidates4storage.x;
            if (info2store.Values[i].upper.y < candidates4storage.y)
                info2store.Values[i].upper.y = candidates4storage.y;
            if (info2store.Values[i].upper.z < candidates4storage.z)
                info2store.Values[i].upper.z = candidates4storage.z;


            if (info2store.Values[i].lower.x > candidates4storage.x)
                info2store.Values[i].lower.x = candidates4storage.x;
            if (info2store.Values[i].lower.y > candidates4storage.y)
                info2store.Values[i].lower.y = candidates4storage.y;
            if (info2store.Values[i].lower.z > candidates4storage.z)
                info2store.Values[i].lower.z = candidates4storage.z;
        }

        if (duration < Time.time)
        {
            if(animationEndMessage.Length > 0) { 
                Debug.Log(animationEndMessage);
                animationEndMessage = "";
            }

        }

        CalcPreview();//this only stores the ROM value?
    }
    // void FixedUpdate()
    void OnRenderObject()
    {
        if (MimicMocap)
        {
            if (_physicsStepsToNextMimic-- < 1)
            {
                CopyMocap();
                _physicsStepsToNextMimic = MimicSkipPhysicsSteps;
            }
        }
    }

    // preview range of motion
    void CalcPreview()
    {
        ArticulationBody[] articulationBodies = targetJoints;
        //we want them all:
        if (articulationBodies.Length == 0)
            articulationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>(true);

        List<RangeOfMotionValue> preview = new List<RangeOfMotionValue>();

        //List<string> jNames = new List<string>(info2store.jointNames);
        List<string> jNames = new List<string>(info2store.getNames());
        for (int i = 0; i < articulationBodies.Length; i++)
        {
            string s = articulationBodies[i].name;
            string[] parts = s.Split(':');
            //we assume the articulationBodies have a name structure of hte form ANYNAME:something-in-the-targeted-joint

            int index = -1;

            index = jNames.FindIndex(x => x.Contains(parts[1]));

            if (index < 0)
                    Debug.Log("Could not find a joint name matching " + s + " and specifically: " + parts[1]);
            else
            {
                //var diff = info2store.minRotations[index] - info2store.maxRotations[index];
                var diff = info2store.Values[index].upper - info2store.Values[index].lower;

                info2store.Values[index].rangeOfMotion = new Vector3(
                    Mathf.Abs(diff.x),
                    Mathf.Abs(diff.y),
                    Mathf.Abs(diff.z)
                );
                preview.Add(info2store.Values[index]);
                //var rangeOfMotion = new Vector3(
                //    Mathf.Abs(diff.x),
                //    Mathf.Abs(diff.y),
                //    Mathf.Abs(diff.z)
                //);
                //var p = new RangeOfMotionValue{
                //    name=parts[1],
                //    lower = info2store.minRotations[index],
                //    upper = info2store.maxRotations[index],
                //    rangeOfMotion = rangeOfMotion
                //};             
                //preview.Add(p);
            }
        }
        RangeOfMotionPreview = preview.ToArray();
    }

    //Not needed, the previous function already does that
    //public void WriteRangeOfMotion()
    //{
    //    if (RangeOfMotion2Store == null)
    //        RangeOfMotion2Store = RangeOfMotion004.CreateInstance<RangeOfMotion004>();
    //    RangeOfMotion2Store.Values = RangeOfMotionPreview;
    //}

    // Make all joints use Max Range of Motion 
    public void SetJointsToMaxROM()
    {
        //these are the articulationBodies that we want to parse and apply the constraints to
        ArticulationBody[] articulationBodies;

        ArticulationBody[] joints = targetJoints;
        //we want them all:
        if (joints.Length == 0)
            joints = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();

        articulationBodies = joints.ToArray();

        foreach (var body in articulationBodies)
        {
            // root has no DOF
            if (body.isRoot)
                continue;
            body.jointType = ArticulationJointType.SphericalJoint;
            body.twistLock = ArticulationDofLock.LimitedMotion;
            body.swingYLock = ArticulationDofLock.LimitedMotion;
            body.swingZLock = ArticulationDofLock.LimitedMotion;

            var drive = new ArticulationDrive();
            drive.lowerLimit = -(float)MaxROM;
            drive.upperLimit = (float)MaxROM;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            body.xDrive = drive;

            drive = new ArticulationDrive();
            drive.lowerLimit = -(float)MaxROM;
            drive.upperLimit = (float)MaxROM;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            body.yDrive = drive;

            drive = new ArticulationDrive();
            drive.lowerLimit = -(float)MaxROM;
            drive.upperLimit = (float)MaxROM;
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            body.zDrive = drive;

            // body.useGravity = false;
        }
    }



    //to apply the Range of Motion to MarathonMan004 (the ragdoll made of articulationBodies).
    //This function is called from an Editor Script

    //this is now done in ApplyRangeOfMotion004
    //public void ApplyROMAsConstraints()
    //{


    //    //these are the articulationBodies that we want to parse and apply the constraints to
    //    ArticulationBody[] articulationBodies = targetJoints;
    //    //we want them all:
    //    if (joints.Length == 0)
    //        articulationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();

    //    //List<string> jNames = new List<string>(info2store.jointNames);
    //    List<string> jNames = new List<string>(info2store.getNames());

    //    for (int i = 0; i < articulationBodies.Length; i++)
    //    {
    //        string s = articulationBodies[i].name;
    //        string[] parts = s.Split(':');
    //        //we assume the articulationBodies have a name structure of hte form ANYNAME:something-in-the-targeted-joint

    //        int index = -1;

    //        index = jNames.FindIndex(x => x.Contains(parts[1]));

    //        if (index < 0)
    //            Debug.Log("Could not find a joint name matching " + s + " and specifically: " + parts[1]);
    //        else
    //        {
    //            //The swing in one axis:
    //            ArticulationDrive yboundaries = articulationBodies[i].yDrive;
    //            yboundaries.upperLimit = info2store.maxRotations[index].y;
    //            yboundaries.lowerLimit = info2store.minRotations[index].y;
    //            articulationBodies[i].yDrive = yboundaries;

    //            //The twist in the second axis:
    //            ArticulationDrive zboundaries = articulationBodies[i].zDrive;
    //            zboundaries.upperLimit = info2store.maxRotations[index].z;
    //            zboundaries.lowerLimit = info2store.minRotations[index].z;
    //            articulationBodies[i].zDrive = zboundaries;

    //            //The twist:
    //            ArticulationDrive xboundaries = articulationBodies[i].xDrive;
    //            xboundaries.upperLimit = info2store.maxRotations[index].x;
    //            xboundaries.lowerLimit = info2store.minRotations[index].x;
    //            articulationBodies[i].xDrive = xboundaries;

    //            articulationBodies[i].anchorRotation = Quaternion.identity; //the anchor cannot be rotated, otherwise the constraints make no sense
    //        }
    //    }
    //    Debug.Log("applied constraints to: " + targetJoints.Length + " articulationBodies in ragdoll object: " + targetRagdollRoot.name);
    //}

    //we assume the constraints have been well applied (see the previous function, Apply ROMAsConstraints)
    //This function is called from an Editor Script
    public void Prepare4PrefabStorage(out RagDollAgent rda, out ManyWorlds.SpawnableEnv envPrefab)
    {


        ArticulationBody targetRagdollPrefab = GameObject.Instantiate(targetRagdollRoot);

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


        //We also prepare everything inside the ragdoll agent :
        for (int i = 0; i < articulationBodies.Length; i++)
        {
            articulationBodies[i].transform.localRotation = Quaternion.identity;
            if (articulationBodies[i].isRoot)
            {
                articulationBodies[i].immovable = false;
                if (rda != null)
                    rda.CameraTarget = articulationBodies[i].transform;


                if (trainingEnv)
                {
                    envPrefab = GameObject.Instantiate(trainingEnv);

                    Animator target = envPrefab.transform.GetComponentInChildren<Animator>();



                    //we assume the environment has an animated character, and in this there is a son which is the root of a bunch of rigidBodies forming a humanoid.
                    //TODO: replace this function with something that creates the rigidBody humanoid such a thing procedurally
                    activateMarathonManTarget(target);

                    //we also need our target animation to have this:
                    TrackBodyStatesInWorldSpace tracker = target.GetComponent<TrackBodyStatesInWorldSpace>();
                    if (tracker == null)
                        target.gameObject.AddComponent<TrackBodyStatesInWorldSpace>();



                    if (rda != null)
                    {
                        rda.transform.parent = envPrefab.transform;
                        rda.name = targetRagdollRoot.name;
                        rda.enabled = true;//this should already be the case, but just ot be cautious
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



    static void activateMarathonManTarget(Animator target)
    {

        Transform[] rbs = target.GetComponentsInChildren<Transform>(true);


        //Rigidbody[] rbs = target.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].gameObject.SetActive(true);

        }



        //the animation source is a son of the SpawnableEnv, or it does not find the MocapControllerArtanim when it initializes
        MapAnimationController2RagdollRef mca = target.GetComponent<MapAnimationController2RagdollRef>();
        mca.enabled = true;






    }



}
