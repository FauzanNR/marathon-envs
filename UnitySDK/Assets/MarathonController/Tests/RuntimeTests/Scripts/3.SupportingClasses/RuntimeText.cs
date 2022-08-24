using UnityEngine;
using System.IO;

//modified from https://support.unity.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-

public class RuntimeText 

{

    public string filename = "test23.txt";
    public string path = "";




    public  void WriteString()

    {

#if UNITY_EDITOR
        path = "Assets/Resources/";
#else
        path = Application.persistentDataPath + "/";
#endif

        string pathandfilename = path + filename;


        //Write some text to the test.txt file

        StreamWriter writer = new StreamWriter(pathandfilename, true);

        writer.WriteLine("Test");

        writer.Close();

        StreamReader reader = new StreamReader(pathandfilename);


        reader.Close();
        Debug.Log("Wrote data to: " + pathandfilename);

    }

    public void ReadString()

    {
        
#if UNITY_EDITOR
        path = "Assets/Resources/";
#else
        path = Application.persistentDataPath + "/";
#endif

        string pathandfilename = path + filename;


        //Read the text from directly from the test.txt file

        StreamReader reader = new StreamReader(pathandfilename);

        reader.Close();
        Debug.Log("Read data from: " + pathandfilename);

    }

}

