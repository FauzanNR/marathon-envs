using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ManyWorlds;
using Unity.MLAgents;

using System.Linq;

public class CompareHandsDistance : MonoBehaviour
{

    [SerializeField]
    TestParameters myParameters;


    public struct Transforms4metrics
    {


        public Transform rootRagdoll;
        public Transform rootSource;

        public Transform leftHandRagdoll;
        public Transform leftHandSource;

        public Transform rightHandRagdoll;
        public Transform rightHandSource;


    }






    //returns the difference between the ragdoll hands and the reference hands, relative to each root
    public static float[] GetHandsDistance(Transforms4metrics o)
    {

        return new float[2] {
               Mathf.Abs( Vector3.Distance(o.leftHandRagdoll.position, o.rootRagdoll.position) -  Vector3.Distance(o.leftHandSource.position, o.rootSource.position)),
               Mathf.Abs( Vector3.Distance(o.rightHandRagdoll.position, o.rootRagdoll.position)-  Vector3.Distance(o.rightHandSource.position, o.rootSource.position)) };

    }


        public Transforms4metrics[] objects2measure;

        protected TestParameters configObject;
        protected int frameCount;

        protected SpawnableEnv[] spawnedEnvs;

      

        public static Transform FindDeepChild(Transform t, string name)
        {

            return t.GetComponentsInChildren<Transform>()
                              .FirstOrDefault(c => c.gameObject.name == name);

        }


    private void OnEnable()
    {
        configObject = myParameters;
        //in the Unit test version, this will be re-defined and defined like the line below:
        //configObject = (TestParameters)Resources.Load("TestParameters/DReConAndMujoco120HZ500KP");

    }

    void Start()
        {

        Time.fixedDeltaTime = 1 / configObject.fixedFreq;


        Vector3 spawnStartPos = Vector3.zero;


            List<SpawnableEnv> spawnedList = new List<SpawnableEnv>();
            foreach (SpawnableEnv spawnableEnv in configObject.envs4test)
            {

                spawnableEnv.UpdateBounds();
                Vector3 step = new Vector3(0f, 0f, spawnableEnv.bounds.size.z + (spawnableEnv.bounds.size.z * spawnableEnv.paddingBetweenEnvs));

                var env = Agent.Instantiate(spawnableEnv, spawnStartPos, spawnableEnv.gameObject.transform.rotation);
                spawnStartPos += step;

                spawnedList.Add(env);


            }

            spawnedEnvs = spawnedList.ToArray();


            //configure it how we want it

            //most of this can probably be a method in class test parameters 
            metric checkDistance1 = new metric();
            checkDistance1.name = "DReCon Hand Distance";

            metric checkDistance2 = new metric();
            checkDistance2.name = "Mujoco Hand Distance";

            configObject.metrics = new metric[] { checkDistance1, checkDistance2 };
            configObject.initMetrics();



            DReConAgent ragdoll1 = spawnedEnvs[0].GetComponentInChildren<DReConAgent>();

            Transforms4metrics t4m1 = new Transforms4metrics();
            t4m1.rootRagdoll = ragdoll1.transform.Find("articulation:mixamorig:Hips");

            t4m1.leftHandRagdoll = FindDeepChild(t4m1.rootRagdoll, "articulation:mixamorig:LeftHand");
            t4m1.rightHandRagdoll = FindDeepChild(t4m1.rootRagdoll, "articulation:mixamorig:RightHand");

            t4m1.rootSource = ragdoll1.KinematicRigObject.transform.Find("articulation:mixamorig:Hips");
            t4m1.leftHandSource = FindDeepChild(t4m1.rootSource, "articulation:mixamorig:LeftHand");
            t4m1.rightHandSource = FindDeepChild(t4m1.rootSource, "articulation:mixamorig:RightHand");

            /*
            Debug.Log("Transforms for metrics in root1: " + t4m1.rootRagdoll + "\n leftHand: " + 
                t4m1.leftHandRagdoll + "\n rightHand: " + t4m1.rightHandRagdoll);

            Debug.Log("Transforms for metrics in SOURCE1: " + t4m1.rootSource + "\n leftHand: " +
    t4m1.leftHandSource + "\n rightHand: " + t4m1.rightHandSource);
        */



            DReConAgent ragdoll2 = spawnedEnvs[1].GetComponentInChildren<DReConAgent>();



            Transforms4metrics t4m2 = new Transforms4metrics();

            Transform simulation = ragdoll2.transform.parent.Find("Simulation");

            t4m2.rootRagdoll = simulation.Find("Hips");

            t4m2.leftHandRagdoll = FindDeepChild(t4m2.rootRagdoll, "LeftHand");
            t4m2.rightHandRagdoll = FindDeepChild(t4m2.rootRagdoll, "RightHand");

            t4m2.rootSource = simulation.Find("K_Hips");
            t4m2.leftHandSource = FindDeepChild(t4m2.rootSource, "K_LeftHand");
            t4m2.rightHandSource = FindDeepChild(t4m2.rootSource, "K_RightHand");

            /*
            Debug.Log("Transforms for metrics in root2: " + t4m2.rootRagdoll + "\n leftHand: " + t4m2.leftHandRagdoll + "\n rightHand: " + t4m2.rightHandRagdoll);


            Debug.Log("Transforms for metrics in SOURCE2: " + t4m2.rootSource + "\n leftHand: " +
t4m2.leftHandSource + "\n rightHand: " + t4m2.rightHandSource);
    */


            objects2measure = new Transforms4metrics[2] { t4m1, t4m2 };


        configObject.test2store = new Vector3[2] { Vector3.zero, 3.14f* Vector3.one };


        }



        void Update()
        {
            frameCount++;


            if (frameCount > 1)
            {
                int whichmetric = 0;
                foreach (metric m in configObject.metrics)
                {

                    var floatarray = GetHandsDistance(objects2measure[whichmetric]);
                  //  Debug.Log("checking in frame " + frameCount + "object: " + whichmetric + "float array: " + floatarray[0] + " " + floatarray[1]);

                    m.addSample(floatarray);

                    whichmetric++;


                }


            }





        }












}
