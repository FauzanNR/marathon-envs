using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Unity.Mathematics;
using Unity.MLAgents;

namespace MotorUpdate
{


#if USE_LSPD

    [CreateAssetMenu(fileName = "LSPD", menuName = "ScriptableObjects/LSPD", order = 1)]
    public class LSPDUpdateRule : MotorUpdateRule
    {


        [SerializeField]
        protected float dT = 1 / 60;




        private LSPDHierarchy _lpd;

        List<IArticulation> _motors;

        public override float GetTorque(float[] curState, float[] targetState)
        {

            return 0f;

        }

        public override float GetTorque(IState curState, IState targetState)
        {
            return 0f;
        }


        public override void Initialize(ModularMuscles muscles = null, float dT = 1 / 60  ) {

           

            this.dT = dT;



           _lpd = muscles.gameObject.AddComponent<LSPDHierarchy>();



            DecisionRequester _decisionRequester = _lpd.gameObject.GetComponent<DecisionRequester>();

        

            _motors = new List<IArticulation>();

            //TODO, change the initialization so it returns directly ArticulationBodyAdapter things, and not this
            foreach(ArticulationBody ab in _lpd.Init(1000, GetActionTimeDelta(muscles.gameObject)) )
            {

                _motors.Add(new ArticulationBodyAdapter(ab));
                
            }




        }



        /*
        public override float3 GetTorque(IArticulation joint, float3[] targetStates)
        {

              _lpd.CompleteMimicry();
        //TODO: the addRelativeTorque should be done OUTSIDE from the LSPD controller

            // e.g. foreach( var curState in curStates){} etc

        }
        */



    }


#endif

}