using System.Collections;
using System.Collections.Generic;
//using System;

using UnityEngine;


using Unity.MLAgents.Policies;
using Unity.MLAgents;
using System.Linq;
using Unity.Barracuda;
using System.ComponentModel;

public class TrainingEnvironmentGenerator : MonoBehaviour
{



    [Header("The animated character:")]


    [SerializeField]
    Animator characterReference;

    [SerializeField]
    Transform characterReferenceHead;

    [SerializeField]
    Transform characterReferenceRoot;


    [Tooltip("fingers will be excluded from physics-learning")]
    [SerializeField]
    Transform[] characterReferenceHands;

    //we assume here is the end-effector, but feet have an articulaiton (sensors will be placed on these and their immediate parents)
    //strategy to be checked: if for a quadruped we add the 4 feet here, does it work?
    [Tooltip("same as above but not taking into account fingers. Put the last joint")]
    [SerializeField]
    Transform[] characterReferenceFeet;


    [Header("How we want the generated assets stored:")]

    [SerializeField]
    string AgentName;

    [SerializeField]
    string TrainingEnvName;



    [Header("Configuration options:")]
    [SerializeField]
    string LearningConfigName;

    [Range(0, 359)]
    public int MinROMNeededForJoint = 0;


    [Tooltip("body mass in grams/ml")]
    [SerializeField]
    float massdensity = 1.01f;


    
    [SerializeField]
    string trainingLayerName = "marathon";

    //[SerializeField]
    //ROMparserSwingTwist ROMparser;

    [SerializeField]
    public RangeOfMotionValues info2store;


    [Header("Prefabs to generate training environment:")]
    [SerializeField]
    ManyWorlds.SpawnableEnv referenceSpawnableEnvironment;

    [SerializeField]
    Material trainerMaterial;

    [SerializeField]
    PhysicMaterial colliderMaterial;



    //things generated procedurally that we store to configure after the generation of the environment:
    [HideInInspector]
    [SerializeField]
    Animator character4training;

    [HideInInspector]
    [SerializeField]
    Animator character4synthesis;

    [HideInInspector][SerializeField]
    ManyWorlds.SpawnableEnv _outcome;


    


    [HideInInspector]
    [SerializeField]
    List<ArticulationBody> articulatedJoints;

    [HideInInspector]
    [SerializeField]
    Muscles muscleteam;

    public ManyWorlds.SpawnableEnv Outcome{ get { return _outcome; } }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void GenerateTrainingEnv() {

        character4training = Instantiate(characterReference.gameObject).GetComponent<Animator>();
        character4training.gameObject.SetActive(true);

        //we assume those are already there (i.e., an animated character with a controller) 
        //character4training.gameObject.AddComponent<CharacterController>();
        //MocapAnimatorController mac = character4training.gameObject.AddComponent<MocapAnimatorController>();
        //mac.IsGeneratedProcedurally = true;


        MapAnim2Ragdoll mca =character4training.gameObject.AddComponent<MapAnim2Ragdoll>();
        //mca.IsGeneratedProcedurally = true;

        character4training.gameObject.AddComponent<TrackBodyStatesInWorldSpace>();
        character4training.name = "Source:" + AgentName;

        SkinnedMeshRenderer[] renderers = character4training.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer r in renderers) {

            Material[] mats = r.sharedMaterials;
            for (int i =0; i < mats.Length; i++) {
                mats[i] = trainerMaterial;
            }
            r.sharedMaterials = mats;
        }

        



        character4synthesis = Instantiate(characterReference.gameObject).GetComponent<Animator>();
        character4synthesis.gameObject.SetActive(true);

        character4synthesis.name = "Result:" + AgentName ;


        //we remove everything except the transform
        UnityEngine.Component[] list = character4synthesis.GetComponents(typeof(UnityEngine.Component));
        foreach (UnityEngine.Component c in list)
        {

            if (c is Transform || c is Animator || c is CharacterController)
            {
            }
            else
            {
                DestroyImmediate(c);

            }

        }

        character4synthesis.GetComponent<Animator>().runtimeAnimatorController = null;




        MapRagdoll2Anim rca = character4synthesis.gameObject.AddComponent<MapRagdoll2Anim>();
      

        _outcome = Instantiate(referenceSpawnableEnvironment).GetComponent<ManyWorlds.SpawnableEnv>();
        _outcome.name = TrainingEnvName;


        ProcRagdollAgent ragdollMarathon = generateRagDollFromAnimatedSource(rca, _outcome);


        Transform[] ts= ragdollMarathon.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) {
            t.gameObject.layer = LayerMask.NameToLayer(trainingLayerName);
        }




        /*
        AnimationController animationcontroller = character4training.GetComponent<AnimationController>();


        
        if(animationcontroller != null)
        //we make sure they are in the same layers:
        {

           int  _layerMask = 1 << ragdollMarathon.gameObject.layer;
            _layerMask |= 1 << character4training.gameObject.layer;
            _layerMask = ~(_layerMask);

            character4training.GetComponent<AnimationController>()._layerMask = _layerMask;
            //TODO: this will only work if the character is animated with an AnimationController. We should REMOVE this dependency

        }
        */




        addTrainingParameters(rca, ragdollMarathon);


        //UNITY BUG
        //This below seems to make it crash. My guess is that:
        /*
        ArticulationBody is a normal component, but it also does some odd thing: it affects the physical simluation, since it is a rigidBody plus an articulatedJoint. The way it is defined is that it has the properties of a rigidBody, but it ALSO uses the hierarchy of transforms  to define a chain of articulationJoints, with their rotation constraints, etc. Most notably, the ArticulationBody that is highest in the root gets assigned a property automatically, "isRoot", which means it's physics constraints are different.My guess is that when you change the hierarchy in the editor, at some point in the future the chain of ArticulationBody recalculates who is the root, and changes its properties. However since this relates to physics, it is not done in the same thread.
If the script doing this is short, it works because this is finished before the update of the ArticulationBody chain is triggered. But when I add more functionality, the script lasts longer, and then it crashes. This is why I kept getting those Physx errors, and why it kept happening in a not-so-reliable way, because we do not have any way to know when this recalculation is done.The fact that ArticulationBody is a fairly recent addition to Unity also makes me suspect they did not debug it properly.The solution seems to be to do all the setup while having the game objects that have articulationBody components with no hierarchy changes, and also having the rootgameobject inactive. When I do this,  I am guessing it does not trigger the update of the ArticulationBody chain.

        I seem to have a reliable crash:

            1.if I use ragdollroot.gameObject.SetActive(true) at the end of my configuration script, it crashes.
            2.if I comment that line, it does not.
            3.if I set it to active manually, through the editor, after running the script with that line commented, it works.

        */
        //ragdoll4training.gameObject.SetActive(true);


        _outcome.GetComponent<RenderingOptions>().ragdollcontroller = ragdollMarathon.gameObject;


        character4training.transform.SetParent(_outcome.transform);
        _outcome.GetComponent<RenderingOptions>().movementsource = character4training.gameObject;

        character4synthesis.transform.SetParent(_outcome.transform);


        //ragdoll4training.gameObject.SetActive(true);




    }


    /*
    public void activateRagdoll() {

        RagDollAgent ragdoll4training = _outcome.GetComponentInChildren<RagDollAgent>(true);
        if(ragdoll4training!=null)
            ragdoll4training.gameObject.SetActive(true);


    }
    */



    public void GenerateRagdollForMocap() {


        MapAnim2Ragdoll mca = character4training.gameObject.GetComponent<MapAnim2Ragdoll>();
        mca.DynamicallyCreateRagdollForMocap();

  

    }




ProcRagdollAgent  generateRagDollFromAnimatedSource( MapRagdoll2Anim target, ManyWorlds.SpawnableEnv trainingenv) {

   
        GameObject temp = GameObject.Instantiate(target.gameObject);
        

        //we remove everything we do not need:
        SkinnedMeshRenderer[] renderers = temp.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer rend in renderers)
        {

            if(rend.gameObject != null)
                DestroyImmediate(rend.gameObject);

        }


        //we remove everything except the transform
        UnityEngine.Component[] list = temp.GetComponents(typeof(UnityEngine.Component));
        foreach (UnityEngine.Component c in list) {

            if (c is Transform)
            {
            }
            else {
                DestroyImmediate(c);

            }

        }
        

        temp.name = "Ragdoll:" + AgentName ;
        muscleteam=  temp.AddComponent<Muscles>();
        temp.transform.position = target.transform.position;
        temp.transform.rotation = target.transform.rotation;

        //it might be important to have this BEFORE we add ArticulationBody members, doing it afterwards, and having it active, makes the entire thing crash 
        temp.transform.parent = trainingenv.transform;
        temp.gameObject.SetActive(false);


       
        Transform[] pack = temp.GetComponentsInChildren<Transform>();

        Transform root = pack.First<Transform>(x => x.name == characterReferenceRoot.name);



        Transform[] joints = root.transform.GetComponentsInChildren<Transform>();



        List<Transform> listofjoints = new List<Transform>(joints);
        //we drop the sons of the limbs (to avoid including fingers in the following procedural steps)
        foreach (Transform t in characterReferenceHands) {
            string limbname = t.name;// + "(Clone)";
            Debug.Log("checking sons of: " + limbname);
            Transform limb = joints.First<Transform>(x => x.name == limbname);




            List<Transform> childstodelete = new List<Transform>(limb.GetComponentsInChildren<Transform>());
            childstodelete.Remove(limb);
            foreach (Transform t2 in childstodelete)
            {
                listofjoints.Remove(t2);
                t2.DetachChildren();//otherwise, it tries to destroy the children later, and fails.
            }
            foreach (Transform t2 in childstodelete)
            {
                DestroyImmediate(t2.gameObject);
            }




        }
        joints = listofjoints.ToArray();
        articulatedJoints = new List<ArticulationBody>();

        var colliders = new List<CapsuleCollider>();

        foreach (Transform j in joints) {
            ArticulationBody ab = j.gameObject.AddComponent<ArticulationBody>();
            ab.anchorRotation = Quaternion.identity;
            ab.mass = 0.1f;
            ab.jointType = ArticulationJointType.FixedJoint;
            articulatedJoints.Add(ab);

            //note: probably not needed
            string namebase = j.name.Replace("(Clone)", "");

            j.name = "articulation:" + namebase;

            GameObject go = new GameObject();
            go.transform.position = j.gameObject.transform.position;
            go.transform.parent = j.gameObject.transform;
            go.name = "collider:" + namebase;
            CapsuleCollider c = go.AddComponent<CapsuleCollider>();
            c.material = colliderMaterial;
            c.height = .12f;
            c.radius = c.height;
            ab = go.AddComponent<ArticulationBody>();
            ab.anchorRotation = Quaternion.identity;
            ab.jointType = ArticulationJointType.FixedJoint;
            ab.mass = massdensity *  Mathf.PI * c.radius *c.radius *c.height * Mathf.Pow(10,2); 
            colliders.Add(c);
        }

        List<string> colliderNamesToDelete = new List<string>();
        foreach (var c in colliders)
        {
            string namebase = c.name.Replace("collider:", "");
            Transform j = joints.First(x=>x.name=="articulation:" + namebase);
            // if root, skip
            var articulatedDad = j.GetComponent<ArticulationBody>();
            if (articulatedDad == null || articulatedDad.transform == joints[0])
                continue;
            j = joints.FirstOrDefault(x=>x.transform.parent.name=="articulation:" + namebase);
            if (j==null)
            {
                // mark to delete as is an end point
                colliderNamesToDelete.Add(c.name);
                continue;
            }
            var ab = j.GetComponent<ArticulationBody>();
            if (ab == null || ab.transform == joints[0])
                continue;
            Vector3 dadPosition = articulatedDad.transform.position;
            c.height = Vector3.Distance(dadPosition, j.transform.position);

            //ugly but it seems to work.
            Vector3 direction = (dadPosition - j.transform.position).normalized;
            float[] directionarray = new float[3] { Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z) };
            float maxdir = Mathf.Max(directionarray);
            List<float> directionlist = new List<float>(directionarray);
            c.direction = directionlist.IndexOf(maxdir);

            c.center = (j.transform.position - dadPosition) / 2.0f;
            c.radius = c.height / 7;
            ab = c.GetComponent<ArticulationBody>();
            ab.mass = massdensity *  Mathf.PI * c.radius *c.radius *c.height * Mathf.Pow(10,2); //we are aproximating as a cylinder, assuming it wants it in kg
        }
        foreach (var name in colliderNamesToDelete)
        {
            var toDel = colliders.First(x=>x.name == name);
            colliders.Remove(toDel);
            GameObject.DestroyImmediate(toDel);
        }

        //I add reference to the ragdoll, the articulationBodyRoot:
        target.ArticulationBodyRoot = root.GetComponent<ArticulationBody>();

        foreach (var articulationBody in root.GetComponentsInChildren<ArticulationBody>())
        {
            var overlap = articulationBody.gameObject.AddComponent<HandleOverlap>();
            overlap.Parent = target.ArticulationBodyRoot.gameObject;
        }

        addSensorsInFeet(root);







        //at this stage, every single articulatedBody is root. Check it out with the script below
        /*
        foreach (Transform j in joints)
        {
            ArticulationBody ab = j.transform.GetComponent<ArticulationBody>();
            if (ab.isRoot)
            {
                Debug.Log(ab.name + "is root ");
            }
        }
        */




        ProcRagdollAgent _ragdoll4training = temp.AddComponent<ProcRagdollAgent>();
        //      _ragdoll4training.transform.parent = trainingenv.transform;
        //_ragdoll4training.transform.SetParent(trainingenv.transform);

        _ragdoll4training.CameraTarget = root;

    

        return _ragdoll4training;

    }


    void addSensorsInFeet(Transform root) {

        //I add the sensors in the feet:
        Transform[] pack2 = root.GetComponentsInChildren<Transform>();
        foreach (Transform t in characterReferenceFeet)
        {

            Transform foot = pack2.First<Transform>(x => x.name == "articulation:" + t.name);

            GameObject sensorL = new GameObject();
            SphereCollider sphL = sensorL.AddComponent<SphereCollider>();
            sphL.radius = 0.03f;
            sphL.isTrigger = true;
            sensorL.AddComponent<SensorBehavior>();
            sensorL.AddComponent<HandleOverlap>();

            //TODO: we are assuming it faces towards the +z axis. It could be done more generic looking into the direction of the collider
            sensorL.transform.parent = foot;
            sensorL.transform.localPosition = new Vector3(-0.02f, 0, 0);
            sensorL.name = foot.name + "sensor_L";

            GameObject sensorR = GameObject.Instantiate(sensorL);
            sensorR.transform.parent = foot;
            sensorR.transform.localPosition = new Vector3(0.02f, 0, 0);
            sensorR.name = foot.name + "sensor_R";

            //we add another sensor for the toe:
            GameObject sensorT = GameObject.Instantiate(sensorL);
            sensorT.transform.parent = foot.parent;
            sensorT.transform.localPosition = new Vector3(0.0f, -0.01f, -0.04f);
            sensorT.name = foot.name + "sensor_T";




        }




    }

    //it needs to go after adding ragdollAgent or it automatically ads an Agent, which generates conflict
    void addTrainingParameters(MapRagdoll2Anim target, ProcRagdollAgent temp) {



        BehaviorParameters bp = temp.gameObject.GetComponent<BehaviorParameters>();

        bp.BehaviorName = LearningConfigName;



        DecisionRequester dr =temp.gameObject.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = 2;
        dr.TakeActionsBetweenDecisions = false;



        Rewards2Learn dcrew = temp.gameObject.AddComponent<Rewards2Learn>();
        dcrew.headname = "articulation:" + characterReferenceHead.name;
        dcrew.targetedRootName = "articulation:" + characterReferenceRoot.name; //it should be it's son, but let's see



        temp.MaxStep = 2000;
        temp.FixedDeltaTime = 0.0125f;
        temp.RequestCamera = true;


        temp.gameObject.AddComponent<SensorObservations>();
        Observations2Learn dcobs = temp.gameObject.AddComponent<Observations2Learn>();
        dcobs.targetedRootName = characterReferenceRoot.name;  // target.ArticulationBodyRoot.name;

        dcobs.targetedRootName = "articulation:" + characterReferenceRoot.name; //it should be it's son, but let's see

        MapRangeOfMotion2Constraints rom = temp.gameObject.AddComponent<MapRangeOfMotion2Constraints>();

        //used when we do not parse
        rom.info2store = info2store;
        

    }






    public void GenerateRangeOfMotionParser() {

        
        ROMparserSwingTwist rom = gameObject.GetComponentInChildren<ROMparserSwingTwist>();
        if (rom == null) {
            GameObject go = new GameObject();
            go.name = "ROM-parser";
            go.transform.parent = gameObject.transform;
            rom = go.AddComponent<ROMparserSwingTwist>();



        }


        rom.info2store = info2store;
        rom.theAnimator = characterReference;
        rom.skeletonRoot = characterReferenceRoot;

        rom.targetRagdollRoot = character4synthesis.GetComponent<MapRagdoll2Anim>().ArticulationBodyRoot;

        rom.trainingEnv = _outcome;

    }

    void generateMuscles() {

        //muscles

        foreach(ArticulationBody ab in articulatedJoints) { 

            Muscles.MusclePower muscle = new Muscles.MusclePower();
            muscle.PowerVector = new Vector3(40, 40, 40);


            muscle.Muscle = ab.name;

            if (muscleteam.MusclePowers == null)
                muscleteam.MusclePowers = new List<Muscles.MusclePower>();

            muscleteam.MusclePowers.Add(muscle);

        }


    }






    public void Prepare4RangeOfMotionParsing()
    {
        _outcome.gameObject.SetActive(false);
        characterReference.gameObject.SetActive(true);

    }


    public void Prepare4EnvironmentStorage()
    {

        characterReference.gameObject.SetActive(false);

        _outcome.gameObject.SetActive(true);
      
       // RagDollAgent ra = _outcome.GetComponentInChildren<RagDollAgent>(true);
       // ra.gameObject.SetActive(true);





    }

    public void ApplyROMasConstraintsAndConfigure() {

        MapRangeOfMotion2Constraints ROMonRagdoll = Outcome.GetComponentInChildren<MapRangeOfMotion2Constraints>(true);
        //ROMonRagdoll.MinROMNeededForJoint = MinROMNeededForJoint;

        if (ROMonRagdoll.info2store == null) {
            ROMonRagdoll.info2store = this.info2store;

        }

        ROMonRagdoll.ConfigureTrainingForRagdoll(MinROMNeededForJoint);

        
        generateMuscles();

        ROMonRagdoll.GetComponent<DecisionRequester>().DecisionPeriod = 2;

        ROMonRagdoll.gameObject.SetActive(true); 
    }
}
