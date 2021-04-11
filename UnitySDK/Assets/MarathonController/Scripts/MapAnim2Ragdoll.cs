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
    // public Vector3 CenterOfMassVelocity;
    // public float CenterOfMassVelocityMagnitude;
    // public Vector3 CenterOfMassVelocityInRootSpace;
    // public float CenterOfMassVelocityMagnitudeInRootSpace;
    public Vector3 LastCenterOfMassInWorldSpace;
	public List<Vector3> LastPosition;
	public List<Quaternion> LastRotation;
	public List<Vector3> Velocities;
	public List<Vector3> AngularVelocities;


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
		var lastPosition = _ragDollRigidbody
			.Select(x=>x.position)
			.ToList();
		var lastRotation = _ragDollRigidbody
			.Select(x=>x.rotation)
			.ToList();
		MimicAnimation();
        // get Center Of Mass velocity in f space
		var newCOM = GetCenterOfMass();
		var lastCOM = LastCenterOfMassInWorldSpace;
		LastCenterOfMassInWorldSpace = newCOM;
		if (_lastPositionTime != Time.time)
		{
			float timeDelta = Time.fixedDeltaTime;
			var velocity = newCOM - lastCOM;
			velocity /= timeDelta;
			// CenterOfMassVelocity = velocity;
			// CenterOfMassVelocityMagnitude = CenterOfMassVelocity.magnitude;
			// CenterOfMassVelocityInRootSpace = transform.InverseTransformVector(velocity);
			// CenterOfMassVelocityMagnitudeInRootSpace = CenterOfMassVelocityInRootSpace.magnitude;

			LastPosition = lastPosition;
			LastRotation = lastRotation;

			var newPosition = _ragDollRigidbody
				.Select(x=>x.position)
				.ToList();
			var newRotation = _ragDollRigidbody
				.Select(x=>x.rotation)
				.ToList();
			Velocities = LastPosition
				.Zip(newPosition, (last, cur)=> (cur-last-_snapOffset)/timeDelta)
				.ToList();
			AngularVelocities = LastRotation
				.Zip(newRotation, (last, cur)=> GetAngularVelocity(cur, last, timeDelta))
				.ToList();
			_snapOffset = Vector3.zero;
		}
		_lastPositionTime = Time.time;		
    }

	float _lastPositionTime = float.MinValue;
	Vector3 _snapOffset = Vector3.zero;
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
	}
	Quaternion FromToRotation(Quaternion from, Quaternion to)
    {
        if (to == from) return Quaternion.identity;

        return to * Quaternion.Inverse(from);
    }
	// Adjust the value of an angle to lie within [-pi, +pi].
    float NormalizedAngle(float angle)
    {
        if (angle < 180)
        {
            return angle * Mathf.Deg2Rad;
        }
        return (angle - 360) * Mathf.Deg2Rad;
    }
	Vector3 NormalizedEulerAngles(Vector3 eulerAngles)
    {
        var x = NormalizedAngle(eulerAngles.x);
        var y = NormalizedAngle(eulerAngles.y);
        var z = NormalizedAngle(eulerAngles.z);
        return new Vector3(x, y, z);
    }

    // Find angular velocity. The delta rotation is converted to radians within [-pi, +pi].
    Vector3 GetAngularVelocity(Quaternion from, Quaternion to, float timeDelta)
    {
        var rotationVelocity = FromToRotation(from, to);
        var angularVelocity = NormalizedEulerAngles(rotationVelocity.eulerAngles) / timeDelta;
        return angularVelocity;
    }
	
    public Vector3 GetCenterOfMass()
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
		// CenterOfMassVelocity = Vector3.zero;
		// CenterOfMassVelocityMagnitude = 0f;
		// CenterOfMassVelocityInRootSpace = Vector3.zero;
		// CenterOfMassVelocityMagnitudeInRootSpace = 0f;
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
		// root.gameObject.SetActive(false);
		var rstat = _ragDollRigidbody.First(x=>x.name == root.name);
		root.TeleportRoot(rstat.position, rstat.rotation);
		root.transform.position = rstat.position;
		root.transform.rotation = rstat.rotation;
		// root.gameObject.SetActive(true);
		foreach (var targetRb in targets)
        {
			var stat = _ragDollRigidbody.First(x=>x.name == targetRb.name);
			if (targetRb.isRoot)
				continue;
			// bool shouldDebug = targetRb.name == "articulation:mixamorig:RightArm";
			// bool didDebug = false;

			if (targetRb.jointType == ArticulationJointType.SphericalJoint)
			{
				float stiffness = 0f;
				float damping = float.MaxValue;
				float forceLimit = 0f;
				// if (shouldDebug)
				// 	didDebug = true;
				Vector3 decomposedRotation = DecomposeQuanterium(stat.transform.localRotation);
				int j=0;
				List<float> thisJointPosition = Enumerable.Range(0,targetRb.dofCount).Select(x=>0f).ToList(); 
				
				if (targetRb.twistLock == ArticulationDofLock.LimitedMotion)
				{
					var drive = targetRb.xDrive;
					var deg = decomposedRotation.x;
					thisJointPosition[j++] = deg * Mathf.Deg2Rad;
					drive.stiffness = stiffness;
					drive.damping = damping;
					drive.forceLimit = forceLimit;
					drive.target = deg;
					targetRb.xDrive = drive;
				}
				if (targetRb.swingYLock == ArticulationDofLock.LimitedMotion)
				{
					var drive = targetRb.yDrive;
					var deg = decomposedRotation.y;
					thisJointPosition[j++] = deg * Mathf.Deg2Rad;
					drive.stiffness = stiffness;
					drive.damping = damping;
					drive.forceLimit = forceLimit;
					drive.target = deg;
					targetRb.yDrive = drive;
				}
				if (targetRb.swingZLock == ArticulationDofLock.LimitedMotion)
				{
					var drive = targetRb.zDrive;
					var deg = decomposedRotation.z;
					thisJointPosition[j++] = deg * Mathf.Deg2Rad;
					drive.stiffness = stiffness;
					drive.damping = damping;
					drive.forceLimit = forceLimit;
					drive.target = deg;
					targetRb.zDrive = drive;
				}
				switch (targetRb.dofCount)
				{
					case 1:
						targetRb.jointPosition = new ArticulationReducedSpace(thisJointPosition[0]);
						break;
					case 2:
						targetRb.jointPosition = new ArticulationReducedSpace(
							thisJointPosition[0],
							thisJointPosition[1]);
						break;
					case 3:
						targetRb.jointPosition = new ArticulationReducedSpace(
							thisJointPosition[0],
							thisJointPosition[1],
							thisJointPosition[2]);
						break;
					default:
						break;
				}
			}
        }
		foreach (var childAb in root.GetComponentsInChildren<ArticulationBody>())
		{
			// // childAb.transform.localPosition = Vector3.zero;
			// // childAb.transform.localEulerAngles = Vector3.zero;
			childAb.angularVelocity = Vector3.zero;
			childAb.velocity = Vector3.zero;
		}
		// var curJointPositions = new List<float>();
		// root.GetJointPositions(curJointPositions);
		// var dofCount = curJointPositions.Count();
		// // root.SetDriveTargets(localRotationsRad);
		// root.SetDriveTargetVelocities(Enumerable.Range(0,dofCount)
        //             .Select(x=>0f).ToList());
		// root.SetJointAccelerations(Enumerable.Range(0,dofCount)
        //             .Select(x=>0f).ToList());
		// root.SetJointForces(Enumerable.Range(0,dofCount)
        //             .Select(x=>0f).ToList());
		// // root.SetJointPositions(localRotationsRad);
		// root.SetJointVelocities(Enumerable.Range(0,dofCount)
        //             .Select(x=>0f).ToList());
    }
	Vector3 DecomposeQuanterium(Quaternion localRotation)
	{
		//the decomposition in swing-twist, typically works like this:
		Quaternion swing = new Quaternion(0.0f, localRotation.y, localRotation.z, localRotation.w);
		// Quaternion swing = new Quaternion(localRotation.x, localRotation.y, localRotation.z, localRotation.w);
		swing = swing.normalized;

		Quaternion twist = Quaternion.Inverse(swing) * localRotation;

		Vector3 decomposition = new Vector3(twist.eulerAngles.x, swing.eulerAngles.y, swing.eulerAngles.z);

		//we make sure we keep the values nearest to 0 (with a modulus)
		if (Mathf.Abs(decomposition.x - 360) < Mathf.Abs(decomposition.x))
			decomposition.x = (decomposition.x - 360);
		if (Mathf.Abs(decomposition.y - 360) < Mathf.Abs(decomposition.y))
			decomposition.y = (decomposition.y - 360);
		if (Mathf.Abs(decomposition.z - 360) < Mathf.Abs(decomposition.z))
			decomposition.z = (decomposition.z - 360);
		return decomposition;
	}

	public void CopyVelocityTo(GameObject targetGameObject, Vector3 velocity)
    {
		LazyInitialize();

        var targets = targetGameObject.GetComponentsInChildren<ArticulationBody>().ToList();
		if (targets?.Count == 0)
			return;
        var root = targets.First(x=>x.isRoot);
		
		if (Velocities == null || Velocities.Count == 0)
			return;

		// basic
		// foreach (var target in targets)
		// {
		// 	target.velocity = velocity;
		// }

		Vector3 aveVelocity = Vector3.zero;
		Vector3 aveAngularVelocity = Vector3.zero;
		for (int i = 0; i < _ragDollRigidbody.Count; i++)
		{
			var source = _ragDollRigidbody[i];
			var target = targets.First(x=>x.name == source.name);
			var vel = Velocities[i];
			// var vel = velocity;
			vel += velocity;
			var angVel = AngularVelocities[i];
			// angVel = Vector3.zero;
            foreach (var childAb in target.GetComponentsInChildren<ArticulationBody>())
            {
                if (childAb == target)
                    continue;
                childAb.transform.localPosition = Vector3.zero;
                childAb.transform.localEulerAngles = Vector3.zero;
                childAb.angularVelocity = Vector3.zero;
                childAb.velocity = Vector3.zero;
            }
            target.velocity = vel;
            target.angularVelocity = angVel;
			aveVelocity += Velocities[i];
			aveAngularVelocity += AngularVelocities[i];
        }
		var c = (float)_ragDollRigidbody.Count;
		aveVelocity = aveVelocity / c;
		aveAngularVelocity = aveAngularVelocity / c;
		c = c;
	}
	public Vector3 SnapTo(Vector3 snapPosition)
	{
		snapPosition.y = transform.position.y;
		var snapDistance = snapPosition-transform.position;
		transform.position = snapPosition;
		_snapOffset += snapDistance;
		return snapDistance;
	}
	public List<Rigidbody> GetRigidBodies()
	{
		LazyInitialize();
		return GetComponentsInChildren<Rigidbody>().ToList();
	}
}