using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Assertions;

using System.Linq.Expressions;

public class MapAnim2Ragdoll : MonoBehaviour, IOnSensorCollision 
{//previously Mocap Controller Artanim
	public List<float> SensorIsInTouch;
	List<GameObject> _sensors;

	internal Animator anim;

	[Range(0f,1f)]
	public float NormalizedTime;    
	public float Lenght;
	public bool IsLoopingAnimation;

	[SerializeField]
	Rigidbody _rigidbodyRoot;

	List<Transform> _animTransforms;
	List<Transform> _ragdollTransforms;


	// private List<Rigidbody> _rigidbodies;
	// private List<Transform> _transforms;

	public bool RequestCamera;
	public bool CameraFollowMe;
	public Transform CameraTarget;

	Vector3 _resetPosition;
	Quaternion _resetRotation;

	[Space(20)]

	//TODO: find a way to remove this dependency (otherwise, not fully procedural)
	private bool _usingMocapAnimatorController = false;

	AnimationController _mocapAnimController;

	// [SerializeField]
	// float _debugDistance = 0.0f;

	private List<MappingOffset> _offsetsSource2RB = null;

    //for debugging, we disable this when setTpose in MarathonTestBedController is on
    [HideInInspector]
    public bool doFixedUpdate = true;

	bool _hasLazyInitialized;

	// void SetOffsetSourcePose2RBInProceduralWorld() {

	// 	_transforms = GetComponentsInChildren<Transform>().ToList();

	// 	_offsetsSource2RB = new List<MappingOffset>();


	// 	if (_rigidbodies == null)
	// 	{
	// 		_rigidbodies = _rigidbodyRoot.GetComponentsInChildren<Rigidbody>().ToList();
	// 		// _transforms = GetComponentsInChildren<Transform>().ToList();
	// 	}
	// 	foreach (Rigidbody rb in _rigidbodies)
	// 	{

	// 		//ArticulationBody ab = _articulationbodies.First(x => x.name == abname);

	// 		string[] temp = rb.name.Split(':');

	// 		//string tname = temp[1];
	// 		string tname = rb.name.TrimStart(temp[0].ToArray<char>());

	// 		tname = tname.TrimStart(':');

	// 		//if structure is "articulation:" + t.name, it comes from a joint:

	// 		if (temp[0].Equals("articulation"))
	// 		{

	// 			Transform t = _transforms.First(x => x.name == tname);

	// 			//TODO: check these days if those values are different from 0, sometimes
	// 			Quaternion qoffset = rb.transform.rotation * Quaternion.Inverse(t.rotation);

	// 			MappingOffset r = new MappingOffset(t, rb, qoffset);

	// 			_offsetsSource2RB.Add(r);
	// 			r.UpdateRigidBodies = true;//TODO: check if really needed, probably the constructor already does it
	// 		}
	// 	}
	// }

	// MappingOffset SetOffsetSourcePose2RB(string rbname, string tname)
	// {
	// 	//here we set up:
	// 	// a. the transform of the rigged character input
	// 	// NO b. the rigidbody of the physical character
	// 	// c. the offset calculated between the rigged character INPUT, and the rigidbody


	// 	if (_transforms == null)
	// 	{
	// 		_transforms = GetComponentsInChildren<Transform>().ToList();
	// 		//Debug.Log("the number of transforms  in source pose is: " + _transforms.Count);

	// 	}

	// 	if (_offsetsSource2RB == null)
	// 	{
	// 		_offsetsSource2RB = new List<MappingOffset>();

	// 	}

	// 	if (_rigidbodies == null )
	// 	{
	// 		_rigidbodies = _rigidbodyRoot.GetComponentsInChildren<Rigidbody>().ToList();
	// 		// _transforms = GetComponentsInChildren<Transform>().ToList();
	// 	}

	// 	Rigidbody rb = null;

	// 	try
	// 	{
	// 		rb = _rigidbodies.First(x => x.name == rbname);
	// 	}
	// 	catch (Exception e)
	// 	{
	// 		Debug.LogError("no rigidbody with name " + rbname);
	// 	}

	// 	Transform tref = null;
	// 	try
	// 	{

	// 		tref = _transforms.First(x => x.name == tname);

	// 	}
	// 	catch (Exception e)
	// 	{
	// 		Debug.LogError("no bone transform with name in input pose " + tname);

	// 	}

	// 	//from refPose to Physical body:
	// 	//q_{physical_body} = q_{offset} * q_{refPose}
	// 	//q_{offset} = q_{physical_body} * Quaternion.Inverse(q_{refPose})

	// 	//Quaternion qoffset = rb.transform.localRotation * Quaternion.Inverse(tref.localRotation);


	// 	//using the global rotation instead of the local one prevents from dependencies on bones that are not mapped to the rigid body (like the shoulder)
	// 	Quaternion qoffset = rb.transform.rotation * Quaternion.Inverse(tref.rotation);


	// 	MappingOffset r = new MappingOffset(tref, rb, qoffset);
	// 	r.UpdateRigidBodies = true;//not really needed, the constructor already does it

	// 	_offsetsSource2RB.Add(r);
	// 	return r;
	// }




	public void OnAgentInitialize()
	{
		LazyInitialize();
	}
	void LazyInitialize()
    {
		if (_hasLazyInitialized)
			return;

		// check if we need to create our ragdoll
		var ragdoll4Mocap = GetComponentsInChildren<Transform>()
			.Where(x=>x.name == "RagdollForMocap")
			.FirstOrDefault();
		if (ragdoll4Mocap == null)
			DynamicallyCreateRagdollForMocap();

		try
		{
			_mocapAnimController = GetComponent<AnimationController>();
			string s = _mocapAnimController.name;//this should launch an exception if there is no animator
			_usingMocapAnimatorController = true;
		}
		catch(Exception e) {
			_usingMocapAnimatorController = false;
			Debug.LogWarning("Mocap Controller is working WITHOUT AnimationController");
		}

		var ragdollTransforms = 
			GetComponentsInChildren<Transform>()
			.Where(x=>x.name.StartsWith("articulation"))
			.ToList();
		var ragdollNames = ragdollTransforms
			.Select(x=>x.name)
			.ToList();
		var animNames = ragdollNames
			.Select(x=>x.Replace("articulation:",""))
			.ToList();
		var animTransforms = animNames
			.Select(x=>GetComponentsInChildren<Transform>().FirstOrDefault(y=>y.name == x))
			.Where(x=>x!=null)
			.ToList();
		_animTransforms = new List<Transform>();
		_ragdollTransforms = new List<Transform>();
		// first time, copy position and rotation
		foreach (var animTransform in animTransforms)
		{
			var ragdollTransform = ragdollTransforms
				.First(x=>x.name == $"articulation:{animTransform.name}");
			ragdollTransform.position = animTransform.position;
			ragdollTransform.rotation = animTransform.rotation;
			_animTransforms.Add(animTransform);
			_ragdollTransforms.Add(ragdollTransform);
		}

	

        SetupSensors();

		//if (_usingMocapAnimatorController && !_isGeneratedProcedurally)

			if (RequestCamera && CameraTarget != null)
		{
			var instances = FindObjectsOfType<MapAnim2Ragdoll>().ToList();
			if (instances.Count(x=>x.CameraFollowMe) < 1)
				CameraFollowMe = true;
		}
        if (CameraFollowMe){
            var camera = FindObjectOfType<Camera>();
            var follow = camera.GetComponent<SmoothFollow>();
            follow.target = CameraTarget;
        }
		_resetPosition = transform.position;
		_resetRotation = transform.rotation;

		_hasLazyInitialized = true;

		// NOTE: do after setting _hasLazyInitialized as can trigger infinate loop
		anim = GetComponent<Animator>();
		if (_usingMocapAnimatorController)
		{
			anim.Update(0f);
		}

    }


	public void DynamicallyCreateRagdollForMocap()
	{
		// Find Ragdoll in parent
		Transform parent = this.transform.parent;
		ProcRagdollAgent[] ragdolls = parent.GetComponentsInChildren<ProcRagdollAgent>(true);
		Assert.AreEqual(ragdolls.Length, 1, "code only supports one RagDollAgent");
		ProcRagdollAgent ragDoll = ragdolls[0];
		var ragdollForMocap = new GameObject("RagdollForMocap");
		ragdollForMocap.transform.SetParent(this.transform, false);
		Assert.AreEqual(ragDoll.transform.childCount, 1, "code only supports 1 child");
		var ragdollRoot = ragDoll.transform.GetChild(0);
		// clone the ragdoll root
		var clone = Instantiate(ragdollRoot);
		// remove '(clone)' from names
		foreach (var t in clone.GetComponentsInChildren<Transform>())
		{
			t.name = t.name.Replace("(Clone)", "");
		}


		// swap ArticulatedBody for RidgedBody
        List<string> bodiesNamesToDelete = new List<string>();
		foreach (var abody in clone.GetComponentsInChildren<ArticulationBody>())
		{
			var bodyGameobject = abody.gameObject;
			var rb = bodyGameobject.AddComponent<Rigidbody>();
			rb.mass = abody.mass;
			rb.useGravity = abody.useGravity;
			// it makes no sense but if i do not set the layer here, then some objects dont have the correct layer
			rb.gameObject.layer  = this.gameObject.layer;
			bodiesNamesToDelete.Add(abody.name);
		}
		foreach (var name in bodiesNamesToDelete)
		{
			var abody = clone
				.GetComponentsInChildren<ArticulationBody>()
				.First(x=>x.name == name);
			DestroyImmediate(abody);

		}
		// make Kinematic
		foreach (var rb in clone.GetComponentsInChildren<Rigidbody>())
		{
			rb.isKinematic = true;
		}


		//we do this after removing the ArticulationBody, since moving the root in the articulationBody creates TROUBLE
		clone.transform.SetParent(ragdollForMocap.transform, false);


		// setup HandleOverlap
		foreach (var rb in clone.GetComponentsInChildren<Rigidbody>())
		{
			// remove cloned HandledOverlap
			var oldHandleOverlap = rb.GetComponent<HandleOverlap>();
			if (oldHandleOverlap != null)
			{
				DestroyImmediate(oldHandleOverlap);
			}
			var handleOverlap = rb.gameObject.AddComponent<HandleOverlap>();
			handleOverlap.Parent = clone.gameObject;
		}

		// set the root
		this._rigidbodyRoot = clone.GetComponent<Rigidbody>();
		// set the layers
		ragdollForMocap.layer = this.gameObject.layer;
		foreach (Transform child in ragdollForMocap.GetComponentInChildren<Transform>())
		{
			child.gameObject.layer  = this.gameObject.layer;
		}
		var triggers = ragdollForMocap
			.GetComponentsInChildren<Collider>()
			.Where(x=>x.isTrigger);
		foreach (var trigger in triggers)
		{
			trigger.gameObject.SetActive(false);
			trigger.gameObject.SetActive(true);
		}
	}
	void SetupSensors()
	{
		_sensors = GetComponentsInChildren<SensorBehavior>()
			.Select(x=>x.gameObject)
			.ToList();
		SensorIsInTouch = Enumerable.Range(0,_sensors.Count).Select(x=>0f).ToList();
	}

    // void FixedUpdate()
	void OnAnimatorMove()
    {
		LazyInitialize();
        if (doFixedUpdate)
            OnFixedUpdate();
	
    }


    void OnFixedUpdate() {
		LazyInitialize();

        //if (!_usesMotionMatching)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
            Lenght = stateInfo.length;
            NormalizedTime = stateInfo.normalizedTime;
            IsLoopingAnimation = stateInfo.loop;
            var timeStep = stateInfo.length * stateInfo.normalizedTime;
            //var endTime = 1f;
            //if (IsLoopingAnimation)
            //    endTime = 3f;
            // if (NormalizedTime <= endTime) {
            // }       
        }
		MimicAnimation();
    }

	void MimicAnimation() {
		if (!anim.enabled)
			return;
		// copy position for root (assume first target is root)
		_ragdollTransforms[0].position = _animTransforms[0].position;
		// copy rotation
		for (int i = 0; i < _animTransforms.Count; i++)
		{
			_ragdollTransforms[i].rotation = _animTransforms[i].rotation;
		}	

		// SetOffsetSourcePose2RBInProceduralWorld();
		// MimicCynematicChar();
	}
	// void MimicCynematicChar()
	// {

	// 	try
	// 	{
	// 		foreach (MappingOffset o in _offsetsSource2RB)
	// 		{
	// 			o.UpdateRotation();

	// 		}
	// 	}
	// 	catch (Exception e)
	// 	{
	// 		Debug.Log("not calibrated yet...");
	// 	}
	// }



	public void OnReset(Quaternion resetRotation)
	{
		LazyInitialize();

        if (!doFixedUpdate)
            return;

            if (_usingMocapAnimatorController)
		{
			_mocapAnimController.OnReset();
			//TODO. we should find a more general way to define those relations wiht MocapAnimatorController004, to decouple the different pieces of code

		}
		else
		{
			Debug.Log("I am resetting the reference animation with MxMAnimator (no _mocapController)");

		


		}





		transform.position = _resetPosition;
		// handle character controller skin width
		var characterController = GetComponent<CharacterController>();
		if (characterController != null)
		{
			var pos = transform.position;
			pos.y += characterController.skinWidth;
			transform.position = pos;
		}
		transform.rotation = resetRotation;
		
		MimicAnimation();
		
	}

    public void OnSensorCollisionEnter(Collider sensorCollider, GameObject other)
	{
		LazyInitialize();

		//if (string.Compare(other.name, "Terrain", true) !=0)
		if (other.layer != LayerMask.NameToLayer("Ground"))
				return;
		var sensor = _sensors
			.FirstOrDefault(x=>x == sensorCollider.gameObject);
		if (sensor != null) {
			var idx = _sensors.IndexOf(sensor);
			SensorIsInTouch[idx] = 1f;
		}
	}
	public void OnSensorCollisionExit(Collider sensorCollider, GameObject other)
	{
		LazyInitialize();

		if (other.layer != LayerMask.NameToLayer("Ground"))
				return;
		var sensor = _sensors
			.FirstOrDefault(x=>x == sensorCollider.gameObject);
		if (sensor != null) {
			var idx = _sensors.IndexOf(sensor);
			SensorIsInTouch[idx] = 0f;
		}
	}   
    public void CopyStatesTo(GameObject target)
    {
		LazyInitialize();

        var targets = target.GetComponentsInChildren<ArticulationBody>().ToList();
		if (targets?.Count == 0)
			return;
        var root = targets.First(x=>x.isRoot);
		root.gameObject.SetActive(false);
        foreach (var targetRb in targets)
        {
			var stat = GetComponentsInChildren<Rigidbody>().First(x=>x.name == targetRb.name);
            targetRb.transform.position = stat.position;
            targetRb.transform.rotation = stat.rotation;
            if (targetRb.isRoot)
            {
                targetRb.TeleportRoot(stat.position, stat.rotation);
            }
			float stiffness = 0f;
			float damping = 10000f;
			if (targetRb.twistLock == ArticulationDofLock.LimitedMotion)
			{
				var drive = targetRb.xDrive;
				drive.stiffness = stiffness;
				drive.damping = damping;
				targetRb.xDrive = drive;
			}			
            if (targetRb.swingYLock == ArticulationDofLock.LimitedMotion)
			{
				var drive = targetRb.yDrive;
				drive.stiffness = stiffness;
				drive.damping = damping;
				targetRb.yDrive = drive;
			}
            if (targetRb.swingZLock == ArticulationDofLock.LimitedMotion)
			{
				var drive = targetRb.zDrive;
				drive.stiffness = stiffness;
				drive.damping = damping;
				targetRb.zDrive = drive;
			}
        }
		root.gameObject.SetActive(true);
    }	   
	public Vector3 SnapTo(Vector3 snapPosition)
	{
		snapPosition.y = transform.position.y;
		var snapDistance = snapPosition-transform.position;
		transform.position = snapPosition;
		return snapDistance;
	}
	public List<Rigidbody> GetRigidBodies()
	{
		LazyInitialize();
		return GetComponentsInChildren<Rigidbody>().ToList();
	}
}