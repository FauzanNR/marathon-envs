using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Mujoco;


using Kinematic;

namespace MotorUpdate
{
    public abstract class MotorUpdateRule : ScriptableObject
    {

        protected float dT = 1 / 60;


     



        
        public virtual void Initialize(Muscles muscles = null, float dT = 1 / 60) {




        }

    


        public abstract List<float> GetJointForces(IState[] currentState, IState[] targetState);







    }









    #region Queryable state Adapters
    public interface IState
    {
        public float Acceleration { get; }
        public float Velocity { get; }
        public float Position { get; }

        public float[] stateVector { get; }

        public string Name { get; }

        public GameObject gameObject { get; }
       
    }

   

    public class MjActuatorState : IState
    {
        readonly private MjActuator mjActuator;

        public MjActuatorState(MjActuator mjActuator)
        {
            this.mjActuator = mjActuator;
        }

        public float Velocity => mjActuator.Velocity;

        public float Position => mjActuator.Length;

        public float Acceleration => mjActuator.GetAcceleration(); 

        public string Name => mjActuator.name;



        public GameObject gameObject { get => mjActuator.gameObject; }

        public float[] stateVector => new float[] { Position, Velocity, Acceleration};
    }

    public class MjHingeJointState : IState
    {
        readonly private MjHingeJoint joint;

        public MjHingeJointState(MjHingeJoint joint)
        {
            this.joint = joint;
        }

        public float Velocity => joint.GetVelocityRad();

        public float Position => joint.GetPositionRad();

        public float Acceleration => joint.GetAccelerationRad();

        public string Name => joint.name;

        public GameObject gameObject { get => joint.gameObject; }

        public float[] stateVector => new float[] { Position, Velocity, Acceleration };
    }
    /*
    public class MjPositionState : IState
    {
        readonly private MjActuator mjActuator;

        public MjPositionState(MjActuator mjActuator)
        {
            this.mjActuator = mjActuator;
        }

        public float Velocity => 0f;

        public float Position => mjActuator.Length;

        public float Acceleration => 0f;

        public string Name => mjActuator.name;



        public GameObject gameObject { get => mjActuator.gameObject; }

        public float[] stateVector => new float[] { Position, Velocity, Acceleration };
    }
    */


    public struct StaticState : IState
    {
        readonly float position;
        readonly float velocity;
        readonly float acceleration;

        public StaticState(float position, float velocity, float acceleration)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
        }

        public float Acceleration => acceleration;

        public float Velocity => velocity;

        public float Position => position;

        public string Name => "ManualState";

        public GameObject gameObject => null;

        public float[] stateVector => new float[] { position, velocity, acceleration };
    }
    #endregion


    /*
    #region Articulation Adapters
    

    public interface IReducedState {

        //Here everything is in reduced Coordinates

        public float3 JointAcceleration { get; }
        public float3 JointVelocity { get; }
        public float3 JointPosition { get; }

    }


    public interface IArticulation: IReducedState
    {
        //Here everything is in reduced Coordinates

        
        //public bool isXblocked { get; }
        //public bool isYblocked { get; }
        //public bool isZblocked { get; }
        
        public string Name { get; }

        public float Mass { get; }

        public GameObject gameObject { get; }
       // public abstract void AddRelativeTorque(float3 torque);
    }



   
    public class ArticulationBodyAdapter : IArticulation
    {

        readonly private ArticulationBody _ab;

        public ArticulationBodyAdapter(ArticulationBody ab)
        {
            this._ab = ab;
        }

        public float3 JointPosition => Utils.GetArticulationReducedSpaceInVector3(_ab.jointPosition);
        public float3 JointVelocity => Utils.GetArticulationReducedSpaceInVector3(_ab.jointVelocity);

        public float3 JointAcceleration => Utils.GetArticulationReducedSpaceInVector3(_ab.jointAcceleration);

        
        //public bool isXblocked => ( _ab.twistLock == ArticulationDofLock.LockedMotion);
        //public bool isYblocked => (_ab.swingYLock == ArticulationDofLock.LockedMotion);
        //public bool isZblocked => (_ab.swingZLock == ArticulationDofLock.LockedMotion);
        
        public GameObject gameObject => _ab.gameObject;
        public string Name => _ab.name;
        public float Mass => _ab.mass;


        public  void AddRelativeTorque(float3 torque) {

            _ab.AddRelativeTorque(torque);


        }
    }

    public class RigidbodyAdapter : IReducedState
    {

        readonly private Rigidbody _rb;

        public RigidbodyAdapter(Rigidbody rb)
        {
            this._rb = rb;
        }

        public float3 JointPosition => Mathf.Deg2Rad * Utils.GetSwingTwist(_rb.transform.localRotation);
        public float3 JointVelocity { get {

                Debug.LogWarning("the velocity of the rigidbody should not matter, why are you trying to read it, I am not sure what I am returning is in reduced coord.");
                return _rb.angularVelocity ; } }


        public float3 JointAcceleration
        {
            get
            {

                Debug.LogWarning("the acceleration of the rigidbody should not matter, why are you trying to read it? I am returing null");
                return Vector3.zero;
            }
        }

      
    }

    #endregion
    */
}



