using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using System;

using Unity.MLAgents;//for the DecisionRequester

public class Muscles : MonoBehaviour
{



    [SerializeField]
    public MotorMode MotorUpdateMode;



    [System.Serializable]
    public class MusclePower
    {
        public string Muscle;
        public Vector3 PowerVector;
    }



    [Header("Parameters for Legacy and PD:")]
    public List<MusclePower> MusclePowers;


    public float Stiffness = 100f;
    public float Damping = 50f;
    public float ForceLimit = float.MaxValue;
    public float DampingRatio = 1.0f;


    [Header("Extra Parameters for PD:")]

    public float NaturalFrequency = 40f;
    public float ForceScale = .3f;


    [Header("Debug Collisions")]
    [SerializeField]
    bool skipCollisionSetup;



    [Header("Debug Values, Read Only")]
    public bool updateDebugValues;


    [SerializeField]
    Vector3[] jointVelocityInReducedSpace;
    List<ArticulationBody> _motors;



    private class LastPos
    {
        public string name;
        //public ArticulationReducedSpace pos;
        public ArticulationReducedSpace vel;
    }

    List<LastPos> _lastPos = new List<LastPos>();


    public enum MotorMode { 
    
        legacy,
        PD,
        LSPD,
        force,
        PDopenloop //this is a PD combined with the kinematic input processed as an openloop, see in DReCon
   
    }

    //for the PDopenloop case:
    public List<Transform> _referenceTransforms;


    public delegate void MotorDelegate(ArticulationBody joint, Vector3 targetNormalizedRotation, float actionTimeDelta);

    MotorDelegate UpdateMotor;



#if USE_LSPD
    //only used for LinearPD:
    private LSPDHierarchy _lpd;
#endif
   



    //only used in PDopenloop
    public void SetKinematicReference(IKinematicReference kinematicRoot) 
    {

        _referenceTransforms = kinematicRoot.RagdollTransforms;    
    
    
    }




    //This function is to debug the motor update modes, mimicking a reference animation.
    /// To be used only with the root frozen, "hand of god" mode, it will not work on a 
    /// proper physics character

    public void MimicRigidBodies(List<Rigidbody> targets, float deltaTime)
    {
        Vector3[] targetRotations = new Vector3[_motors.Count];

        int im = 0;
        foreach (var a in targets)
        {
            targetRotations[im] = Mathf.Deg2Rad * Utils.GetSwingTwist(a.transform.localRotation);
            im++;
        }

        switch (MotorUpdateMode)
        {

            case (Muscles.MotorMode.LSPD):
#if USE_LSPD
                _lpd.LaunchMimicry(targetRotations);
#else
                   Debug.LogError("To use this functionality you need to import the Artanim LSPD package");
#endif
                break;


            default:

                int j = 0;
                foreach (var m in _motors)
                {

                    if (m.isRoot)
                    {
                        continue; //never happens because excluded from list
                    }
                    else
                    {
                        //we find the normalized rotation that corresponds to the target rotation (the inverse of what happens inside UpdateMotorWithPD
                        ArticulationBody joint = m;                       
                        
                        Vector3 normalizedRotation = new Vector3();




                        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
                        {
                            var drive = joint.xDrive;
                            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                            var midpoint = drive.lowerLimit + scale;
                            normalizedRotation.x = ( Mathf.Rad2Deg *  targetRotations[j].x - midpoint) / scale;
                            //target.x = midpoint + (targetNormalizedRotation.x * scale);

                        }

                        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
                        {
                            var drive = joint.yDrive;
                            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                            var midpoint = drive.lowerLimit + scale;
                            normalizedRotation.y = (Mathf.Rad2Deg * targetRotations[j].y - midpoint) / scale;
                            //target.y = midpoint + (targetNormalizedRotation.y * scale);


                        }

                        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
                        {
                            var drive = joint.zDrive;
                            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                            var midpoint = drive.lowerLimit + scale;
                            normalizedRotation.z = (Mathf.Rad2Deg * targetRotations[j].z - midpoint) / scale;
                            //target.z = midpoint + (targetNormalizedRotation.z * scale);

                        }



                        UpdateMotor(m, normalizedRotation, Time.fixedDeltaTime);
                    }

                    j++;

                }
                break;
        }



    }



   

    public void UpdateMuscles(float[] vectorAction, float actionTimeDelta)
    {
        //        Debug.Log("vector action values: " + vectorAction[0] + " " + vectorAction[1] + " " + vectorAction[2] + " " + vectorAction[3] + " " + vectorAction[4] + " " + vectorAction[5]);
        int i = 0;//keeps track of the number of action
        switch (MotorUpdateMode)
        {

            case (Muscles.MotorMode.LSPD):


#if USE_LSPD

                Vector3[] targetRotations = new Vector3[_motors.Count];

                int im = 0; //keeps track of the number of motor
                foreach (var m in _motors)
                {
                    if (m.isRoot)
                        continue;

                  
                    targetRotations[im] = Vector3.zero;
                    if (m.jointType != ArticulationJointType.SphericalJoint)
                        continue;
                    if (m.twistLock != ArticulationDofLock.LockedMotion)
                        targetRotations[im].x = vectorAction[i++];
                    if (m.swingYLock != ArticulationDofLock.LockedMotion)
                        targetRotations[im].y = vectorAction[i++];
                    if (m.swingZLock != ArticulationDofLock.LockedMotion)
                        targetRotations[im].z = vectorAction[i++];

                    im++;

                }



                //_lpd.OnUpdate( targetRotations);
                _lpd.LaunchMimicry(targetRotations);


#else
                Debug.LogError("To use this functionality you need to import the Artanim LSPD package");


#endif



                break;




            default:

                foreach (var m in _motors)
                {
                    if (m.isRoot)
                        continue;

                    Vector3 targetNormalizedRotation = Vector3.zero;
                    if (m.jointType != ArticulationJointType.SphericalJoint)
                        continue;
                    if (m.twistLock != ArticulationDofLock.LockedMotion)
                        targetNormalizedRotation.x = vectorAction[i++];
                    if (m.swingYLock != ArticulationDofLock.LockedMotion)
                        targetNormalizedRotation.y = vectorAction[i++];
                    if (m.swingZLock != ArticulationDofLock.LockedMotion)
                        targetNormalizedRotation.z = vectorAction[i++];



                    UpdateMotor(m, targetNormalizedRotation, actionTimeDelta);
                }

                break;
        }

    }

    /*
    public void UpdateMuscles(float[] vectorAction, float actionTimeDelta)
    {
        //        Debug.Log("vector action values: " + vectorAction[0] + " " + vectorAction[1] + " " + vectorAction[2] + " " + vectorAction[3] + " " + vectorAction[4] + " " + vectorAction[5]);
        int i = 0;//keeps track of the number of action


#if USE_LSPD

            Vector3[] targetRotations = new Vector3[_motors.Count];
                 int im = 0; //keeps track of the number of motor

#else


        if (MotorUpdateMode == MotorMode.LSPD)
                Debug.LogError("To use this functionality you need to import the Artanim LSPD package");


#endif


        foreach (var m in _motors)
                {
                    if (m.isRoot)
                        continue;
            Debug.Log("motor number " + im + " action number: " +  i);
                    Vector3 targetNormalizedRotation = Vector3.zero;
                    if (m.jointType != ArticulationJointType.SphericalJoint)
                        continue;
                    if (m.twistLock != ArticulationDofLock.LockedMotion)
                        targetNormalizedRotation.x = vectorAction[i++];
                    if (m.swingYLock != ArticulationDofLock.LockedMotion)
                        targetNormalizedRotation.y = vectorAction[i++];
                    if (m.swingZLock != ArticulationDofLock.LockedMotion)
                        targetNormalizedRotation.z = vectorAction[i++];


                    if (MotorUpdateMode == MotorMode.LSPD)
                    {
                        targetRotations[im] = targetNormalizedRotation;
                        im++;

                    }
                    else {
                        UpdateMotor(m, targetNormalizedRotation, actionTimeDelta);
                    }

                }

#if USE_LSPD
        if (MotorUpdateMode == MotorMode.LSPD)
            _lpd.LaunchMimicry(targetRotations);
#endif


    }
    */

    public void FixedUpdate()
    {

#if USE_LSPD
        switch (MotorUpdateMode)
        {

            case (MotorMode.LSPD):

                _lpd.CompleteMimicry();

                break;

        }
#endif

    }



    // Use this for initialization
    void Start()
    {
        SetupCollisions();

        _motors = GetComponentsInChildren<ArticulationBody>()
                .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
                .Where(x => !x.isRoot)
                .Distinct()
                .ToList();

        foreach (ArticulationBody m in _motors)
        {
            LastPos l = new LastPos();

            l.name = m.name;
            //l.pos = m.jointPosition;
            l.vel = m.jointVelocity;
            _lastPos.Add(l);

            /*
            ArticulationDrive xd = m.xDrive;
            xd.stiffness = Stiffness;
            xd.damping = Damping;
            m.xDrive = xd;

            ArticulationDrive yd = m.xDrive;
            yd.stiffness = Stiffness;
            yd.damping = Damping;
            m.yDrive = yd;

            ArticulationDrive zd = m.xDrive;
            zd.stiffness = Stiffness;
            zd.damping = Damping;
            m.yDrive = zd;
            */

        }





        if (updateDebugValues)
        {



            jointVelocityInReducedSpace = new Vector3[_motors.Count];


        }







        switch (MotorUpdateMode) {

            case (MotorMode.force):
                UpdateMotor = DirectForce;
                break;

            case (MotorMode.PD):
                UpdateMotor = UpdateMotorPDWithVelocity;
                break;

            case (MotorMode.legacy):
                UpdateMotor = LegacyUpdateMotor;
                break;

            case (MotorMode.LSPD):
                UpdateMotor = null;





#if USE_LSPD
                // UpdateMotor = UpdateLinearPD;
                _lpd = gameObject.AddComponent<LSPDHierarchy>();
                
                //SetAllArticulationsFree();
                //  SetDOFAsFreeArticulations();
                
                
               


              


                //this ensures the order in which we parse them follows the convention needed for the ABA algo.

                //_motors = _lpd.Init(root, 5000,  Time.fixedDeltaTime);

                
                DecisionRequester _decisionRequester = GetComponent<DecisionRequester>();

                float GetActionTimeDelta()
                {
                    return _decisionRequester.TakeActionsBetweenDecisions ? Time.fixedDeltaTime : Time.fixedDeltaTime * _decisionRequester.DecisionPeriod;
                }

                _motors = _lpd.Init( 1000,  GetActionTimeDelta());
              

#else
                Debug.LogError("To use this functionality you need to import the Artanim LSPD package");

#endif


                break;

            case (MotorMode.PDopenloop):
                UpdateMotor = UpdateMotorPDopenloop;
                break;


        }


         

    }

    public void OnReset() {



#if USE_LSPD

        if(MotorUpdateMode== MotorMode.LSPD)        
            _lpd.OnReset();


#endif


    }



    public void SetDOFAsLargeROMArticulations()
    {


        ArticulationBody[] articulationBodies =
            GetComponentsInChildren<ArticulationBody>(true)
            .Where(x => x.name.StartsWith("articulation:"))
            .ToArray();





        foreach (ArticulationBody body in articulationBodies)
        {
            if (body.isRoot)
                continue;

            body.jointType = ArticulationJointType.SphericalJoint;




            body.anchorRotation = Quaternion.identity; //we make sure the anchor has no Rotation, otherwise the constraints do not make any sense



            body.twistLock = ArticulationDofLock.LimitedMotion;
            var driveX = body.xDrive;
            driveX.lowerLimit = -170;
            driveX.lowerLimit = -170;
            driveX.upperLimit = +170;

            body.xDrive = driveX;



            body.swingYLock = ArticulationDofLock.LimitedMotion;
            var driveY = body.yDrive;

            driveY.lowerLimit = -170;
            driveY.upperLimit = +170;
            body.yDrive = driveY;


            body.swingZLock = ArticulationDofLock.LimitedMotion;
            var driveZ = body.zDrive;
            driveZ.lowerLimit = -170;
            driveZ.upperLimit = +170;

            body.zDrive = driveZ;


        }



    }



    // Update is called once per frame
    void Update()
    {

        if (updateDebugValues) {

            int i = 0;
            foreach(ArticulationBody m in _motors) { 
            //DEBUG: to keep track of the values, and see if they seem reasonable
               

                Vector3 temp = Utils.GetArticulationReducedSpaceInVector3(m.jointVelocity);

                jointVelocityInReducedSpace[i] = temp;
                i++;
            }


        }


    }

    void SetupCollisions()
    {

        if (!skipCollisionSetup)
        {

            // handle collision overlaps
            IgnoreCollision("articulation:Spine2", new[] { "LeftArm", "RightArm" });
            IgnoreCollision("articulation:Hips", new[] { "RightUpLeg", "LeftUpLeg" });

            IgnoreCollision("LeftForeArm", new[] { "LeftArm" });
            IgnoreCollision("RightForeArm", new[] { "RightArm" });
            IgnoreCollision("RightLeg", new[] { "RightUpLeg" });
            IgnoreCollision("LeftLeg", new[] { "LeftUpLeg" });

            IgnoreCollision("RightLeg", new[] { "RightFoot" });
            IgnoreCollision("LeftLeg", new[] { "LeftFoot" });

        }

    }
    void IgnoreCollision(string first, string[] seconds)
    {
        foreach (var second in seconds)
        {
            IgnoreCollision(first, second);
        }
    }
    void IgnoreCollision(string first, string second)
    {
        var rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();
        var colliderOnes = rigidbodies.FirstOrDefault(x => x.name.Contains(first))?.GetComponents<Collider>();
        var colliderTwos = rigidbodies.FirstOrDefault(x => x.name.Contains(second))?.GetComponents<Collider>();
        if (colliderOnes == null || colliderTwos == null)
            return;
        foreach (var c1 in colliderOnes)
            foreach (var c2 in colliderTwos)
                Physics.IgnoreCollision(c1, c2);
    }

    //this is a simple way to center the masses
    /*public void CenterABMasses()
    { 
        ArticulationBody[] abs = GetComponentsInChildren<ArticulationBody>();
        foreach (ArticulationBody ab in abs)
        {
            if (!ab.isRoot)
            { 
                Vector3 currentCoF = ab.centerOfMass;

                if (ab.transform.childCount > 0)
                {
                    Vector3 newCoF = Vector3.zero;
                    //generally 1, sometimes 2:

                    foreach (Transform child in ab.transform)
                    {
                        newCoF += child.localPosition;

                    }
                    newCoF /= ab.transform.childCount;

                    newCoF = (ab.transform.parent.localPosition + newCoF) / 2.0f;
                    ab.centerOfMass = newCoF;

                    //Debug.Log("AB: " + ab.name + " old CoF: " + currentCoF + " new CoF: " + ab.centerOfMass);
                }
                else 
                {

                    //Debug.Log(ab.name + " has not changed CoF, it is still: " + currentCoF);

                
                }
            }
        }

    }*/



    private static Vector3 GetTargetVelocity(ArticulationBody joint, Vector3 targetNormalizedRotation, float timeDelta)
    {

        Vector3 targetVelocity = new Vector3(0, 0, 0);

        Vector3 currentRotationValues = Utils.GetSwingTwist(joint.transform.localRotation);


        Vector3 target = new Vector3();
        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            target.x = midpoint + (targetNormalizedRotation.x * scale);

        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            target.y = midpoint + (targetNormalizedRotation.y * scale);


        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            target.z = midpoint + (targetNormalizedRotation.z * scale);

        }


        targetVelocity = Utils.AngularVelocityInReducedCoordinates(Utils.GetSwingTwist(joint.transform.localRotation), target, timeDelta);

        targetVelocity = Vector3.ClampMagnitude(targetVelocity, joint.maxAngularVelocity);


        return targetVelocity;



    }


    public void UpdateMotorPDWithVelocity(ArticulationBody joint, Vector3 targetNormalizedRotation, float actionTimeDelta)
    {

        var m = joint.mass;
        var d = DampingRatio; // d should be 0..1.
        var n = NaturalFrequency; // n should be in the range 1..20
        var k = Mathf.Pow(n, 2) * m;
        var c = d * (2 * Mathf.Sqrt(k * m));
        var stiffness = k;
        var damping = c;



        Vector3 power = Vector3.zero;
        try
        {
            power = MusclePowers.First(x => x.Muscle == joint.name).PowerVector;

        }
        catch (Exception e)
        {
            Debug.Log("there is no muscle for joint " + joint.name);

        }

        classicPD(joint, targetNormalizedRotation, actionTimeDelta, power);

    }

    void classicPD(ArticulationBody joint, Vector3 targetNormalizedRotation, float actionTimeDelta, Vector3 power) {


        var m = joint.mass;
        var d = DampingRatio; // d should be 0..1.
        var n = NaturalFrequency; // n should be in the range 1..20
        var k = Mathf.Pow(n, 2) * m;
        var c = d * (2 * Mathf.Sqrt(k * m));
        var stiffness = k;
        var damping = c;


        //why do you never set up the targetVelocity?
        // F = stiffness * (currentPosition - target) - damping * (currentVelocity - targetVelocity)


        Vector3 targetVel = GetTargetVelocity(joint, targetNormalizedRotation, actionTimeDelta);



        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.x * scale);
            drive.target = target;

            drive.targetVelocity = targetVel.x;


            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = power.x * ForceScale;
            joint.xDrive = drive;
        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.y * scale);
            drive.target = target;
            // drive.targetVelocity = (target - currentRotationValues.y) / (_decisionPeriod * Time.fixedDeltaTime);
            drive.targetVelocity = targetVel.y;


            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = power.y * ForceScale;
            joint.yDrive = drive;
        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.z * scale);

            drive.target = target;
            //drive.targetVelocity = (target - currentRotationValues.z) / (_decisionPeriod * Time.fixedDeltaTime);
            drive.targetVelocity = targetVel.z;

            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = power.z * ForceScale;
            joint.zDrive = drive;
        }



    }



    void UpdateMotorPDopenloop(ArticulationBody joint, Vector3 targetRot, float actionTimeDelta)
    {


        Vector3 refRot = Mathf.Deg2Rad * Utils.GetSwingTwist( _referenceTransforms.First(x => x.name == joint.name).localRotation);

        Vector3 power = 40* Vector3.one;


        Vector3 targetNormalizedRotation = refRot + targetRot;

    //From the DReCon paper: (not implemented)
    //    Velocity basedconstraints are used to simulate PD servo motors at the joints,
    //    withmotor constraint torques clamped to 200 Nm.All coeicients of fric-tion
    //    are given a value of 1, except rolling friction which is disabled.


       classicPD(joint, targetNormalizedRotation, actionTimeDelta, power);
    }






    void LegacyUpdateMotor(ArticulationBody joint, Vector3 targetNormalizedRotation,  float actionTimeDelta)
    {



        Vector3 power = Vector3.zero;
        try
        {
            power = MusclePowers.First(x => x.Muscle == joint.name).PowerVector;

        }
        catch (Exception e)
        {
            Debug.Log("there is no muscle for joint " + joint.name);

        }



        power *= Stiffness;
        float damping = Damping;
        float forceLimit = ForceLimit;

        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.x * scale);
            drive.target = target;
            drive.stiffness = power.x;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            joint.xDrive = drive;
        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.y * scale);
            drive.target = target;
            drive.stiffness = power.y;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            joint.yDrive = drive;
        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.z * scale);
            drive.target = target;
            drive.stiffness = power.z;
            drive.damping = damping;
            drive.forceLimit = forceLimit;
            joint.zDrive = drive;
        }
    }



    //NOT TESTED
    void DirectForce(ArticulationBody joint, Vector3 targetNormalizedRotation, float actionTimeDelta)
    {

        
        Vector3 result = 0.05f * targetNormalizedRotation;
        joint.AddRelativeTorque(result);

    }



    static ArticulationReducedSpace AccelerationInReducedSpace(ArticulationReducedSpace currentVel, ArticulationReducedSpace lastVel, float deltaTime)
    {
        ArticulationReducedSpace result = new ArticulationReducedSpace();


        result.dofCount = currentVel.dofCount;

        for(int i = 0; i< result.dofCount; i++)
            result[i] =  (currentVel[i] - lastVel[i]) / deltaTime;

        return result;

    }






}
