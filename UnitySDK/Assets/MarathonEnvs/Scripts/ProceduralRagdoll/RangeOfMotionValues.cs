using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Barracuda;

[System.Serializable]
public class RangeOfMotionValue
{
    // [HideInInspector]
    public string name;
    public Vector3 lower;
    public Vector3 upper;
    public Vector3 rangeOfMotion;
}

[CreateAssetMenu(fileName = "RangeOfMotion", menuName = "Parser/CreateRangeOfMotionFile")]
public class RangeOfMotionValues : ScriptableObject
{
    public RangeOfMotionValue[] Values { get { return _values.ToArray(); }  }


    //[HideInInspector]
    [SerializeField]
    List<RangeOfMotionValue> _values;

    public void addJoint(Transform joint) {
        RangeOfMotionValue r = new RangeOfMotionValue();
        r.name = joint.name;

        _values.Add(r);
    }

    //public
    //a model trained with these constraints
    //NNModel InferenceModel;

    public string[] getNames() {
        

        string[] temp = new string[Values.Length];

        for(int i = 0; i <Values.Length; i++)
        {
            temp[i] = (Values[i].name);


        }
        return temp;
    
    }






}

