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
    public Vector3 rangeOfMotion { get {
        var diff = upper - lower;
        var rom = new Vector3(
            Mathf.Abs(diff.x),
            Mathf.Abs(diff.y),
            Mathf.Abs(diff.z)
        );
        return rom;
    }}
}

[System.Serializable]
public class OscillationParameters
{
    public string name;

    public float dampingRatio;
    public float natFreq;




    //    [SerializeField]
    //    List<float> _cumulatedRotation = new List<float>(); //we keep here the rotation acumulated through time.

    // cumulRot is directly rot-> rotation axis in last frame is:  Vector3(rot.x,rot.y,rot.z).normalized
    //List<Vector3> _rotDirection = new List<Vector3>(); 

    //    Quaternion lastRot = Quaternion.identity;



    [SerializeField]
    List<Quaternion> _animRots = new List<Quaternion>();



    public void AddRotationValue(Quaternion rot) {


        //   _cumulatedRotation.Add(Quaternion.Angle(lastRot, rot)+cumulatedAngle);

        //_cumulatedRotation.Add(Quaternion.Angle( rot,lastRot));

        //  _rotDirection.Add(new Vector3(rot.x, rot.y, rot.z).normalized);

        //lastRot = rot;


        _animRots.Add(rot);

    }


    /*
    public float[] CumulatedRotations {
        get { return _cumulatedRotation.ToArray(); }
    
    }
    */

    public Quaternion[] AnimRotations
    {
        get { return _animRots.ToArray(); }

    }



}

[CreateAssetMenu(fileName = "RangeOfMotion", menuName = "Parser/CreateRangeOfMotionFile")]
public class RangeOfMotionValues : ScriptableObject
{
    public RangeOfMotionValue[] Values { get { return _values.ToArray(); } }


    //[HideInInspector]
    [SerializeField]
    List<RangeOfMotionValue> _values;


    public OscillationParameters[] Osc { get { return _osc.ToArray(); } }
    
    [SerializeField]
    List<OscillationParameters> _osc;


    public void emptyContent() {
        _osc = new List<OscillationParameters>();
        _values = new List<RangeOfMotionValue>();



    }


    public void addJoint(Transform joint)
    {
        RangeOfMotionValue r = new RangeOfMotionValue();
        r.name = joint.name;

        _values.Add(r);


        OscillationParameters o = new OscillationParameters();
        o.name = joint.name;
        _osc.Add(o);


    }

    //public
    //a model trained with these constraints
    //NNModel InferenceModel;

    public string[] getNames()
    {


        string[] temp = new string[Values.Length];

        for (int i = 0; i < Values.Length; i++)
        {
            temp[i] = (Values[i].name);


        }
        return temp;

    }






}

