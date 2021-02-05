using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRagdoll2Anim : MonoBehaviour

    //this class does exactly the symetrical of MocapControllerArtanim: it maps animations from a ragdoll to a rigged character
{

    [SerializeField]
	ArticulationBody _articulationBodyRoot;

    //to generate an environment automatically from a rigged character and an animation (see folder ROM-extraction)
    public ArticulationBody ArticulationBodyRoot { set => _articulationBodyRoot = value; 
		get =>  _articulationBodyRoot; }
  

    private List<ArticulationBody> _articulationbodies = null;

	private List<Transform> _targetPoseTransforms = null;

    private List<MappingOffset> _offsetsRB2targetPoseTransforms = null;




	[Space(20)]



	//not used in 
	[SerializeField]
	float _debugDistance= 0.0f;


	


    // Start is called before the first frame update
    public void Start()
    {
       
    }



	//this one is for the case where everything is generated procedurally
	void  SetOffsetRB2anim() {

		if(_targetPoseTransforms == null)
			_targetPoseTransforms = GetComponentsInChildren<Transform>().ToList();

		if(_offsetsRB2targetPoseTransforms == null)
			_offsetsRB2targetPoseTransforms = new List<MappingOffset>();


		if(_articulationbodies == null)
			_articulationbodies = _articulationBodyRoot.GetComponentsInChildren<ArticulationBody>(true).ToList();





		foreach (ArticulationBody ab in _articulationbodies) {

			//ArticulationBody ab = _articulationbodies.First(x => x.name == abname);

			string[] temp = ab.name.Split(':');



			//if it has another ":" in the name, it crashes miserably
			//string tname = temp[1];
			//instead, we do:
			string tname = ab.name.TrimStart(temp[0].ToArray<char>());

			tname = tname.TrimStart(':');
			//Debug.Log("the full name is: " + ab.name + "  and the trimmed name is: " + tname);


			//if structure is "articulation:" + t.name, it comes from a joint:

			if (temp[0].Equals("articulation")) { 

				Transform t = _targetPoseTransforms.First(x => x.name == tname);


				//TODO: check these days if those values are different from 0, sometimes
				Quaternion qoffset = ab.transform.rotation * Quaternion.Inverse(t.rotation);
				MappingOffset r = new MappingOffset(t, ab, Quaternion.Inverse(qoffset));
				if (ab.isRoot)
				{
					r.SetAsRoot(true, _debugDistance);

				}

				_offsetsRB2targetPoseTransforms.Add(r);

			}
		}



	}






	

	void MimicPhysicalChar()
	{

		try
		{
			foreach (MappingOffset o in _offsetsRB2targetPoseTransforms)
			{
				o.UpdateRotation();
			}
		}
		catch (Exception e)
		{
			Debug.Log("not calibrated yet...");

		}


	}

    private void FixedUpdate()
    {
		MimicAnimationArtanim();
    }
    void MimicAnimationArtanim()
	{


        if (_offsetsRB2targetPoseTransforms == null)
		{

            
				SetOffsetRB2anim();
            

		}
		else
		{
			MimicPhysicalChar();


		}



	}


}
