using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents.Policies;
using Unity.MLAgents;

public class TrainingEnvironmentGenerator : MonoBehaviour
{



    [Header("The animated character:")]


    [SerializeField]
    Animator characterReference;

    [SerializeField]
    Transform characterReferenceHead;

    [SerializeField]
    Transform characterReferenceRoot;


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


    //[SerializeField]
    //ROMparserSwingTwist ROMparser;

    [SerializeField]
    public RangeOfMotion004 info2store;


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
    RagDoll004 muscleteam;

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


        MocapControllerArtanim mca =character4training.gameObject.AddComponent<MocapControllerArtanim>();
        mca.IsGeneratedProcedurally = true;

        character4training.gameObject.AddComponent<TrackBodyStatesInWorldSpace>();
        character4training.name = "Source:" + AgentName;

        SkinnedMeshRenderer[] renderers = character4training.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer r in renderers) {

            Material[] mats = r.sharedMaterials;
            for (int i =0; i < mats.Length; i++) {
                mats[i] = trainerMaterial;
            }
            
        }

        



        character4synthesis = Instantiate(characterReference.gameObject).GetComponent<Animator>();
        character4synthesis.gameObject.SetActive(true);

        character4synthesis.name = "Result:" + AgentName ;


        RagdollControllerArtanim rca = character4synthesis.gameObject.AddComponent<RagdollControllerArtanim>();
        rca.IsGeneratedProcedurally = true;

        _outcome = Instantiate(referenceSpawnableEnvironment).GetComponent<ManyWorlds.SpawnableEnv>();
        _outcome.name = TrainingEnvName;


        RagDollAgent ragdollMarathon = generateRagDollFromAnimatedSource(rca, _outcome);




        MocapAnimatorController004 animationcontroller = character4training.GetComponent<MocapAnimatorController004>();

        if(animationcontroller != null)
        //we make sure they are in the same layers:
        {

           int  _layerMask = 1 << ragdollMarathon.gameObject.layer;
            _layerMask |= 1 << character4training.gameObject.layer;
            _layerMask = ~(_layerMask);

            character4training.GetComponent<MocapAnimatorController004>()._layerMask = _layerMask;
            //TODO: this will only work if we have a MocapAnimatorController004. We should REMOVE this dependency

        }





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


        MocapControllerArtanim mca = character4training.gameObject.GetComponent<MocapControllerArtanim>();
        mca.DynamicallyCreateRagdollForMocap();

    }


    RagDollAgent  generateRagDollFromAnimatedSource( RagdollControllerArtanim target, ManyWorlds.SpawnableEnv trainingenv) {

   
        GameObject temp = GameObject.Instantiate(target.gameObject);
        

        //we remove everything we do not need:
        SkinnedMeshRenderer[] renderers = temp.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer rend in renderers)
        {

            if(rend.gameObject != null)
                DestroyImmediate(rend.gameObject);

        }

        Animator a = temp.GetComponent<Animator>();
        if(a!= null)
            DestroyImmediate(a);
        CharacterController b = temp.GetComponent<CharacterController>();
        if(b!=null)
            DestroyImmediate(b);
        RagdollControllerArtanim r = temp.GetComponent<RagdollControllerArtanim>();
        if(r!=null)
            DestroyImmediate(r);


        temp.name = "Ragdoll:" + AgentName ;
        muscleteam=  temp.AddComponent<RagDoll004>();
        temp.transform.position = target.transform.position;
        temp.transform.rotation = target.transform.rotation;

        //it might be important to have this BEFORE we add ArticulationBody members, doing it afterwards, and having it active, makes the entire thing crash 
        temp.transform.parent = trainingenv.transform;
        temp.gameObject.SetActive(false);


        //TODO: this assumes the first son will always be the good one. A more secure method seems cautious 
        Transform root = temp.transform.GetChild(0);

        Transform[] joints = root.transform.GetComponentsInChildren<Transform>();


         articulatedJoints = new List<ArticulationBody>();


        foreach (Transform j in joints) {
            ArticulationBody ab = j.gameObject.AddComponent<ArticulationBody>();
            ab.anchorRotation = Quaternion.identity;

            ab.mass = 0.1f;



            articulatedJoints.Add(ab);

          


            string namebase = j.name.Replace("(Clone)", "");


            j.name = "articulation:" + namebase;

            //we only add a collider if it has a parent:
            Transform dad = ab.transform.parent;

            ArticulationBody articulatedDad = null;

            if (dad != null)
            {
                articulatedDad = dad.GetComponent<ArticulationBody>();
            }

            if(articulatedDad != null) { 

                GameObject go = new GameObject();
                go.transform.parent = dad;

                //dad.gameObject

                go.name = "collider:" + namebase;

                CapsuleCollider c = go.AddComponent<CapsuleCollider>();
                c.material = colliderMaterial;

                c.height = Vector3.Distance(dad.position, j.transform.position);



                //ugly but it seems to work.
                Vector3 direction = (dad.position - j.transform.position).normalized;
                float[] directionarray = new float[3] { Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z) };
                float maxdir = Mathf.Max(directionarray);

                List<float> directionlist = new List<float>(directionarray);
                c.direction = directionlist.IndexOf(maxdir);

                 

                c.center = (dad.position + j.transform.position) / 2.0f;
                c.radius = c.height / 5;



                // Rigidbody rb = go.AddComponent<Rigidbody>();
                ArticulationBody rb = go.AddComponent<ArticulationBody>();
                rb.jointType = ArticulationJointType.FixedJoint;
                rb.mass = massdensity *  Mathf.PI * c.radius *c.radius *c.height * Mathf.Pow(10,2); //we are aproximating as a cylinder, assuming it wants it in kg



                HandleOverlap ho = go.AddComponent<HandleOverlap>();

                //ho.Parent = dad.gameObject;
                ho.Parent = root.gameObject;




            }
                    
        
        }


        //we add reference to the ragdoll, the articulationBodyRoot
        foreach (Transform j in joints)
        {
            ArticulationBody ab = j.transform.GetComponent<ArticulationBody>();
            if (ab.isRoot) {
                target.ArticulationBodyRoot = ab;
            }
        }


        RagDollAgent _ragdoll4training = temp.AddComponent<RagDollAgent>();
        //      _ragdoll4training.transform.parent = trainingenv.transform;
        //_ragdoll4training.transform.SetParent(trainingenv.transform);

    

        return _ragdoll4training;

    }




    //it needs to go after adding ragdollAgent or it automatically ads an Agent, which generates conflict
    void addTrainingParameters(RagdollControllerArtanim target, RagDollAgent temp) {



        BehaviorParameters bp = temp.gameObject.GetComponent<BehaviorParameters>();

        bp.BehaviorName = LearningConfigName;



        DecisionRequester dr =temp.gameObject.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = 2;
        dr.TakeActionsBetweenDecisions = false;



        DReConRewards dcrew = temp.gameObject.AddComponent<DReConRewards>();
        dcrew.headname = "articulation:" + characterReferenceHead.name;
        dcrew.targetedRootName = target.ArticulationBodyRoot.name; //it should be it's son, but let's see


        temp.MaxStep = 2000;
        temp.FixedDeltaTime = 0.0125f;
        temp.RequestCamera = true;


        temp.gameObject.AddComponent<SensorObservations>();
        DReConObservations dcobs = temp.gameObject.AddComponent<DReConObservations>();
        dcobs.targetedRootName = target.ArticulationBodyRoot.name;


        ApplyRangeOfMotion004 rom = temp.gameObject.AddComponent<ApplyRangeOfMotion004>();
        rom.RangeOfMotion2Store = info2store;
        //rom.ApplyROMInGamePlay = true;


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

        rom.targetRagdollRoot = character4synthesis.GetComponent<RagdollControllerArtanim>().ArticulationBodyRoot;

        rom.trainingEnv = _outcome;

    }

    void generateMuscles() {

        //muscles

        foreach(ArticulationBody ab in articulatedJoints) { 

            RagDoll004.MusclePower muscle = new RagDoll004.MusclePower();
            muscle.PowerVector = new Vector3(40, 40, 40);


            muscle.Muscle = ab.name;

            if (muscleteam.MusclePowers == null)
                muscleteam.MusclePowers = new List<RagDoll004.MusclePower>();

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

        ApplyRangeOfMotion004 ROMonRagdoll = Outcome.GetComponentInChildren<ApplyRangeOfMotion004>(true);
        ROMonRagdoll.MinROMNeededForJoint = MinROMNeededForJoint;
        ROMonRagdoll.ApplyRangeOfMotionToRagDoll();
        ROMonRagdoll.applyDoFOnBehaviorParameters();

        generateMuscles();

        ROMonRagdoll.GetComponent<DecisionRequester>().DecisionPeriod = 2;





    }




}
