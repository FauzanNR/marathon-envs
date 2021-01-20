using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrainingEnvironmentGenerator : MonoBehaviour
{

    [SerializeField]
    ROMparserSwingTwist ROMparser;
    [SerializeField]
    Animator characterReference;

    [SerializeField]
    string SourceAgentName;




    Animator character4training;

  
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

        character4training.name = SourceAgentName;


    }




}
