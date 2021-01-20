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



    [SerializeField]
    ROMparserSwingTwist ROMparser;
    [SerializeField]
    Animator characterReference;



    [SerializeField]
    ManyWorlds.SpawnableEnv referenceSpawnableEnvironment;


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
        character4training.gameObject.AddComponent<MocapAnimatorController>();
        character4training.gameObject.AddComponent<MocapControllerArtanim>();
        character4training.gameObject.AddComponent<TrackBodyStatesInWorldSpace>();
        character4training.name = AgentName + "Source";


        character4synthesis = Instantiate(characterReference.gameObject).GetComponent<Animator>();
        character4synthesis.name = AgentName + "Result" ;
        RagdollControllerArtanim rca = character4synthesis.gameObject.AddComponent<RagdollControllerArtanim>(); //TODO, add reference to the ragdoll, the articulationBodyRoot


        _outcome = Instantiate(referenceSpawnableEnvironment).GetComponent<ManyWorlds.SpawnableEnv>();
        _outcome.name = TrainingEnvName;

        generateRagDollFromAnimatedSource(rca, _outcome);





        character4training.transform.SetParent(_outcome.transform);
        character4synthesis.transform.SetParent(_outcome.transform);






    }


     RagDollAgent  generateRagDollFromAnimatedSource( RagdollControllerArtanim target, ManyWorlds.SpawnableEnv trainingenv) {

        //ragdoll4training = GameObject.Instantiate();

        GameObject temp = new GameObject(AgentName + "Ragdoll");
        temp.AddComponent<RagDoll004>();
        temp.AddComponent<BehaviorParameters>();
        temp.AddComponent<DecisionRequester>();
        temp.transform.position = target.transform.position;
        temp.transform.rotation = target.transform.rotation;


        RagDollAgent _ragdoll4training = temp.AddComponent<RagDollAgent>();
        _ragdoll4training.transform.parent = trainingenv.transform;

        DynamicallyCreateRagdollForMocap(target, ragdoll4training);



        temp.AddComponent<DReConRewards>();
        temp.AddComponent<SensorObservations>();
        temp.AddComponent<DReConObservations>();
        ApplyRangeOfMotion004 rom = temp.AddComponent<ApplyRangeOfMotion004>();
        rom.RangeOfMotion2Store = ROMparser.info2store;



        return _ragdoll4training;

    }



    static void DynamicallyCreateRagdollForMocap(RagdollControllerArtanim target, RagDollAgent ragDoll)
    {
        // Find Ragdoll in parent

        var ragdollRoot = ragDoll.transform.GetChild(0);
        // clone the ragdoll root
        var clone = Instantiate(ragdollRoot);

        //TODO,instantiate all the sons and the colliders


        // remove '(clone)' from names
        foreach (Transform t in target.GetComponentsInChildren<Transform>())
        {



            //t.name = t.name.Replace("(Clone)", "");
        }
        //clone.transform.SetParent(ragdollForMocap.transform, false);
        //// swap ArticulatedBody for RidgedBody
        //foreach (var abody in clone.GetComponentsInChildren<ArticulationBody>())
        //{
        //    var bodyGameobject = abody.gameObject;
        //    var rb = bodyGameobject.AddComponent<Rigidbody>();
        //    rb.mass = abody.mass;
        //    rb.useGravity = abody.useGravity;
        //    Destroy(abody);
        //}
        // make Kinematic
        foreach (var rb in clone.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }

    }




}
