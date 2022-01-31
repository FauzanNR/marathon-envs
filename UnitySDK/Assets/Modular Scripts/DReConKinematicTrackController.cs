using Kinematic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DReCon
{
    public class DReConKinematicTrackController : MonoBehaviour, IAnimationController
    {
        [SerializeField]
        Transform kinematicTransform;

        private BodyChain kinChain;

        private void Awake()
        {
            OnAgentInitialize();
        }

        public void OnReset()
        {
        }

        public Vector3 GetDesiredVelocity()
        {
            return kinChain.CenterOfMassVelocity;
        }

        public void OnAgentInitialize()
        {
            kinChain = new BodyChain(kinematicTransform);
        }
    }
}
