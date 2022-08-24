using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct metric
{

    //a simple struct to generate metrics from arrays of samples.
    //each sample can also be an array of values.

    public string name;
    public float mean;
    public float std;

    public struct sample
    {
        public float[] values;

    }


    List<sample> samples;

    //sample currentSample;

    public static sample emptySample(int sizeSingleSample)
    {

        sample currentSample;

        currentSample.values = new float[sizeSingleSample];

        return currentSample;


    }

    public void initSampleList()
    {
        samples = new List<sample>();

    }
    public void addSample(float[] val) {

        sample a;
        a.values = val;
        samples.Add(a);
    
    }

    public void addSample(float val)
    {

        sample a;
        a.values = new float[1];
        
        a.values[0] = val;
        samples.Add(a);

    }




    public void updateStats()
    {
        mean = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            for (int j = 0; j < samples[i].values.Length; j++)
                mean += samples[i].values[j];

        }
        mean = mean / (samples.Count*samples[0].values.Length);

        std = 0;
        for (int i = 0; i < samples.Count; i++)
        {
            for (int j = 0; j < samples[i].values.Length; j++)
                std += Mathf.Pow((samples[i].values[j] - mean), 2);

        }
        std = Mathf.Sqrt(std / (samples.Count * samples[0].values.Length));


    }


}


