using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;

namespace MotorUpdate
{
    [CreateAssetMenu(fileName = "ClassicPD", menuName = "ScriptableObjects/ClassicPD", order = 1)]
    public class ClassicPDUpdateRule : MotorUpdateRule
    {

        /*
        [SerializeField]
        protected float dT = 1 / 60;

        public override float GetTorque(float[] curState, float[] targetState)
        {

            return -gains[0] * (curState[0] + dT * curState[1] - targetState[0]) - gains[1] * (curState[1] + dT * curState[2] - targetState[1]);

        }

        public override float GetTorque(IState curState, IState targetState)
        {
            return -gains[0] * (curState.Position + curState.Velocity * dT - targetState.Position) - gains[1] * (curState.Velocity + curState.Acceleration * dT - targetState.Velocity);
        }
        */

       
        protected float dT = 1 / 60;

        List<IArticulation> _motors;


       // public List<MusclePower> MusclePowers;

        //  public float MotorScale = 1f;
        public float Stiffness = 50f;
        public float Damping = 100f;
        public float ForceLimit = float.MaxValue;
        public float DampingRatio = 1.0f;
        public float NaturalFrequency = 40f;
        public float ForceScale = .3f;



        public override void Initialize(Agent agent = null, float dT = 1 / 60)
        {


            dT = GetActionTimeDelta(agent.gameObject);

            _motors = GetMotors(agent.gameObject);




        }





        void UpdateMotorPDWithVelocity(ArticulationBody joint, Vector3 targetNormalizedRotation, float actionTimeDelta)
        {

            var m = joint.mass;
            var d = DampingRatio; // d should be 0..1.
            var n = NaturalFrequency; // n should be in the range 1..20
            var k = Mathf.Pow(n, 2) * m;
            var c = d * (2 * Mathf.Sqrt(k * m));
            var stiffness = k;
            var damping = c;


            //This should be redone
           // Vector3 power =  MusclePowers.First(x => x.Muscle == joint.name).PowerVector;





            //we set up the targetVelocity

            Vector3 targetVel = GetTargetVelocity(joint, targetNormalizedRotation, actionTimeDelta);


            //you want: (IArticulation joint, Vector3 targetRotation, float timeDelta)
            //  Utils.AngularVelocityInReducedCoordinates(joint.JointPosition, targetRotation, timeDelta);

            //we calculate the force:
            // F = stiffness * (currentPosition - target) - damping * (currentVelocity - targetVelocity)



            /*
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
            */


        }






    }
}