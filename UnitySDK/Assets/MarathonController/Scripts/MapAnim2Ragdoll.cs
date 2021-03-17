using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AI;
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
	List<Rigidbody> _ragDollRigidbody;


	// private List<Rigidbody> _rigidbodies;
	// private List<Transform> _transforms;

	public bool RequestCamera;
	public bool CameraFollowMe;
	public Transform CameraTarget;

	Vector3 _resetPosition;
	Quaternion _resetRotation;

	[Space(20)]
	[Header("Stats")]
    public Vector3 CenterOfMassVelocity;
    public float CenterOfMassVelocityMagnitude;
    public Vector3 LastCenterOfMassInWorldSpace;


	//TODO: find a way to remove this dependency (otherwise, not fully procedural)
	private bool _usingMocapAnimatorController = false;

	IAnimationController _mocapAnimController;

	// [SerializeField]
	// float _debugDistance = 0.0f;

	private List<MappingOffset> _offsetsSource2RB = null;

    //for debugging, we disable this when setTpose in MarathonTestBedController is on
    [HideInInspector]
    public bool doFixedUpdate = true;

	bool _hasLazyInitialized;

	bool _hackyNavAgentMode;

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

		_mocapAnimController = GetComponent<IAnimationController>();
		_usingMocapAnimatorController = _mocapAnimController != null;
		if (!_usingMocapAnimatorController)
		{
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
		_ragDollRigidbody = _ragdollTransforms
			.Select(x=>x.GetComponent<Rigidbody>())
			.Where(x=> x != null)
			.ToList();

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
		var navAgent = GetComponent<NavMeshAgent>();
		if (navAgent)
		{
			var radius = 16f;
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
			transform.position = finalPosition;
			_hackyNavAgentMode = true;
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

    void FixedUpdate()
	// void OnAnimatorMove()
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
        // get Center Of Mass velocity in f space
        float timeDelta = Time.fixedDeltaTime;
		var newCOM = GetCenterOfMass();
        var velocity = newCOM - LastCenterOfMassInWorldSpace;
        velocity /= timeDelta;
        CenterOfMassVelocity = transform.InverseTransformVector(velocity);
        CenterOfMassVelocityMagnitude = CenterOfMassVelocity.magnitude;
		LastCenterOfMassInWorldSpace = newCOM;
	}
	
    Vector3 GetCenterOfMass()
    {
        var centerOfMass = Vector3.zero;
        float totalMass = 0f;
        foreach (Rigidbody ab in _ragDollRigidbody)
        {
            centerOfMass += ab.worldCenterOfMass * ab.mass;
            totalMass += ab.mass;
        }
        centerOfMass /= totalMass;
        // centerOfMass -= _spawnableEnv.transform.position;
        return centerOfMass;
    }


	public void OnReset(Quaternion resetRotation)
	{
		LazyInitialize();

        if (!doFixedUpdate)
            return;

		if (!_hackyNavAgentMode)
		{
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
		}
		
		MimicAnimation();
		LastCenterOfMassInWorldSpace = GetCenterOfMass();
		CenterOfMassVelocity = Vector3.zero;
		CenterOfMassVelocityMagnitude = 0f;
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
	public void CopyVelocityTo(GameObject targetGameObject, Vector3 velocity)
    {
		LazyInitialize();

        var targets = targetGameObject.GetComponentsInChildren<ArticulationBody>().ToList();
		if (targets?.Count == 0)
			return;
        var root = targets.First(x=>x.isRoot);

		float totalVelocity = 0f;
		foreach (var target in targets)
        {
			// var source = GetComponentsInChildren<Rigidbody>().First(x=>x.name == target.name);
            // target.velocity = source.velocity;
			// totalVelocity += source.velocity.magnitude;
			target.velocity = velocity;
        }
	}
	public Vector3 SnapTo(Vector3 snapPosition)
	{
		snapPosition.y = transform.position.y;
		var snapDistance = snapPosition-transform.position;
		transform.position = snapPosition;
		LastCenterOfMassInWorldSpace = GetCenterOfMass();
		return snapDistance;
	}
	public List<Rigidbody> GetRigidBodies()
	{
		LazyInitialize();
		return GetComponentsInChildren<Rigidbody>().ToList();
	}
}