using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ManyWorlds;


[CreateAssetMenu(fileName = "TestParameters", menuName = "ScriptableObjects/TestParameters", order = 1)]
public class TestParameters : ScriptableObject
{
    //public static float tolerance4comparisons = 0.05f;


    public float fixedFreq = 25;
    public float KP = 0;


    //TextAsset file2store;
    RuntimeText textWriter = new RuntimeText();


    public SpawnableEnv[] envs4test;


    public Vector3[] test2store;

    [SerializeField]
    public Metric[] metrics;
    

    public float frameEnd=20;


    public void InitMetrics()
    {
        foreach(Metric m in metrics)
            m.initSampleList();
        
    }

    public void WriteMetricsToFile()
    {

        string s= "name mean std \n";


        foreach (Metric m in metrics)
        {
            m.updateStats();
            s += m.name + " " + m.mean + m.std  + "\n";

        
        
        }

        string now = System.DateTime.Now.Year + "-" + System.DateTime.Now.Month.ToString("00") + "-" + System.DateTime.Now.Day + "-"  + System.DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":","");

        textWriter.filename = "file-" + this.name +now;


        textWriter.WriteString(s);

    }


}
