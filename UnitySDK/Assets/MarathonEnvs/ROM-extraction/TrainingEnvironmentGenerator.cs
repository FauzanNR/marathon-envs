using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents.Policies;
using Unity.MLAgents;

public class TrainingEnvironmentGenerator : MonoBehaviour
{

    [SerializeField]
    string AgentName;

    [SerializeField]
    string TrainingEnvName;



    //[SerializeField]
    //ROMparserSwingTwist ROMparser;

    [SerializeField]
    public RangeOfMotion004 info2store;



    [SerializeField]
    Animator characterReference;

    [SerializeField]
    Transform characterReferenceHead;


    [Header("Prefabs to generate training environment:")]
    [SerializeField]
    ManyWorlds.SpawnableEnv referenceSpawnableEnvironment;

    [SerializeField]
    Material trainerMaterial;

    [SerializeField]
    PhysicMaterial colliderMaterial;


    Animator character4training;
    Animator character4synthesis;

    RagDollAgent ragdoll4training;


    ManyWorlds.SpawnableEnv _outcome;

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
        character4training.gameObject.AddComponent<CharacterController>();



        MocapAnimatorController mac = character4training.gameObject.AddComponent<MocapAnimatorController>();
        mac.IsGeneratedProcedurally = true;


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

       
        RagDollAgent rda = generateRagDollFromAnimatedSource(rca, _outcome);

        _outcome.GetComponent<RenderingOptions>().ragdollcontroller = rda.gameObject;






        character4training.transform.SetParent(_outcome.transform);
        _outcome.GetComponent<RenderingOptions>().movementsource = character4training.gameObject;

        character4synthesis.transform.SetParent(_outcome.transform);






    }


     RagDollAgent  generateRagDollFromAnimatedSource( RagdollControllerArtanim target, ManyWorlds.SpawnableEnv trainingenv) {

   
        GameObject temp = GameObject.Instantiate(target.gameObject);


        //we remove everything we do not need:
        SkinnedMeshRenderer[] renderers = temp.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer r in renderers)
        {
            DestroyImmediate(r.gameObject);

        }
        DestroyImmediate( temp.GetComponent<Animator>());
        DestroyImmediate(temp.GetComponent<CharacterController>());
        DestroyImmediate(temp.GetComponent<RagdollControllerArtanim>());


        temp.name = "Ragdoll:" + AgentName ;
        temp.AddComponent<RagDoll004>();
        temp.AddComponent<BehaviorParameters>();
        temp.AddComponent<DecisionRequester>();
        temp.transform.position = target.transform.position;
        temp.transform.rotation = target.transform.rotation;


        Transform root = temp.transform.GetChild(0);

        Transform[] joints = root.transform.GetComponentsInChildren<Transform>();

        foreach (Transform j in joints) {
            ArticulationBody ab = j.gameObject.AddComponent<ArticulationBody>();
            ab.anchorRotation = Quaternion.identity;

            ab.mass = 0.1f;

            
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
                go.name = "collider:" + namebase;

                CapsuleCollider c = go.AddComponent<CapsuleCollider>();
                c.material = colliderMaterial;

                c.height = Vector3.Distance(dad.position, j.transform.position);
                c.center = (dad.position + j.transform.position) / 2.0f;
                c.radius = c.height / 5;


                // Rigidbody rb = go.AddComponent<Rigidbody>();
                // rb.mass = 4;
                ArticulationBody rb = go.AddComponent<ArticulationBody>();
                rb.jointType = ArticulationJointType.FixedJoint;
                rb.mass = 5;



                HandleOverlap ho = go.AddComponent<HandleOverlap>();
                ho.Parent = dad.gameObject;

               
                



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
        _ragdoll4training.transform.parent = trainingenv.transform;

        DReConRewards dcrew = temp.AddComponent<DReConRewards>();
        dcrew.headname = "articulation:" + characterReferenceHead.name;
        dcrew.targetedRootName = target.ArticulationBodyRoot.name; //it should be it's son, but let's see


        temp.AddComponent<SensorObservations>();
        DReConObservations dcobs = temp.AddComponent<DReConObservations>();
        dcobs.targetedRootName = target.ArticulationBodyRoot.name; 


        ApplyRangeOfMotion004 rom = temp.AddComponent<ApplyRangeOfMotion004>();
        rom.RangeOfMotion2Store = info2store;
        rom.ApplyROMInGamePlay = true;

        
        rom.CalculateDoF();



        if(rom.RangeOfMotion2Store != null)
            rom.ApplyRangeOfMotionToRagDoll();


        return _ragdoll4training;

    }


}
