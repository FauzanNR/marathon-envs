using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    ArticulationBody[] targetJoints;


    [System.Serializable]
    public struct PreviewValues
    {
        [HideInInspector]
        public string name;
        public Vector3 lower;
        public Vector3 upper;
        public Vector3 rangeOfMotion;
    }
    public PreviewValues[] Preview;

    MocapControllerArtanim _mocapControllerArtanim;
    Vector3 _rootStartPosition;
    Quaternion _rootStartRotation;

    public bool MimicMocap;
    [Range(0,359)]
    public int MaxROM = 180;
    [Range(0,359)]
    public int MinROMNeededForJoint = 3;

    [Range(0,500)]
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

        _mocapControllerArtanim = theAnimator.GetComponent<MocapControllerArtanim>();

        // get root start position and rotation
        var atriculationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();
        if (atriculationBodies.Length ==0)
            return;
        var root = atriculationBodies.First(x=>x.isRoot);
        _rootStartPosition = root.transform.position;
        _rootStartRotation = root.transform.rotation;

        // if no joints specified get joints using
        // not root (is static), begins with 'articulation:'
        if (targetJoints.Length == 0)
        {
            targetJoints = targetRagdollRoot
                .GetComponentsInChildren<ArticulationBody>()
                .Where(x=>x.isRoot == false)
                .Where(x=>x.name.StartsWith("articulation:"))
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
            if (atriculationBodies.Length ==0)
                return;
            var root = atriculationBodies.First(x=>x.isRoot);
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
        var root = targets.First(x=>x.isRoot);
		root.gameObject.SetActive(false);
        var mocapRoot = _mocapControllerArtanim.GetComponentsInChildren<Rigidbody>().First(x=>x.name == root.name);
        Vector3 offset = rootPosition - mocapRoot.transform.position;
        foreach (var body in targets)
        {
			var stat = _mocapControllerArtanim.GetComponentsInChildren<Rigidbody>().First(x=>x.name == body.name);
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

        CalcPreview();
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
            articulationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();
        
        List<PreviewValues> preview = new List<PreviewValues>();

        List<string> jNames = new List<string>(info2store.jointNames);
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
                var diff = info2store.minRotations[index] - info2store.maxRotations[index];
                var rangeOfMotion = new Vector3(
                    Mathf.Abs(diff.x),
                    Mathf.Abs(diff.y),
                    Mathf.Abs(diff.z)
                );
                var p = new PreviewValues{
                    name=parts[1],
                    lower = info2store.minRotations[index],
                    upper = info2store.maxRotations[index],
                    rangeOfMotion = rangeOfMotion
                };             
                preview.Add(p);
            }
        }
        Preview = preview.ToArray();
    }

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
    public void ApplyROMAsConstraints()
    {


        //these are the articulationBodies that we want to parse and apply the constraints to
        ArticulationBody[] articulationBodies = targetJoints;
        //we want them all:
        if (joints.Length == 0)
            articulationBodies = targetRagdollRoot.GetComponentsInChildren<ArticulationBody>();

        List<string> jNames = new List<string>(info2store.jointNames);

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
                //The swing in one axis:
                ArticulationDrive yboundaries = articulationBodies[i].yDrive;
                yboundaries.upperLimit = info2store.maxRotations[index].y;
                yboundaries.lowerLimit = info2store.minRotations[index].y;
                articulationBodies[i].yDrive = yboundaries;

                //The twist in the second axis:
                ArticulationDrive zboundaries = articulationBodies[i].zDrive;
                zboundaries.upperLimit = info2store.maxRotations[index].z;
                zboundaries.lowerLimit = info2store.minRotations[index].z;
                articulationBodies[i].zDrive = zboundaries;

                //The twist:
                ArticulationDrive xboundaries = articulationBodies[i].xDrive;
                xboundaries.upperLimit = info2store.maxRotations[index].x;
                xboundaries.lowerLimit = info2store.minRotations[index].x;
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


        //We also prepare everything inside the ragdoll agent :
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

                    Animator target = envPrefab.transform.GetComponentInChildren<Animator>();



                    //we assume the environment has an animated character, and in this there is a son which is the root of a bunch of rigidBodies forming a humanoid.
                    //TODO: replace this function with something that creates the rigidBody humanoid such a thing procedurally
                    activateMarathonManTarget(target);

                    //we also need our target animation to have this:
                    TrackBodyStatesInWorldSpace tracker = target.GetComponent<TrackBodyStatesInWorldSpace>();
                    if (tracker == null)
                        target.gameObject.AddComponent<TrackBodyStatesInWorldSpace>();



                    if (rda != null) { 
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
        MocapControllerArtanim mca = target.GetComponent<MocapControllerArtanim>();
        mca.enabled = true;






    }



}
