using GMDG.NoProduct.Utility;
using System;
using UnityEngine;

namespace GMDG.Basic_2D_Platformer.PlayerMovement
{
    public class CharacterStatus
    {
        private Kinematic2D _kinematicStatus;
        private StateMachine _stateMachine;

        public CharacterStatus(Transform transform) 
        {
            _kinematicStatus = new Kinematic2D(transform);
            _stateMachine = new StateMachine();

            Idle idle = new Idle();
            Walking walking = new Walking();
            Falling falling = new Falling();

            _stateMachine.AddTransition(idle, walking, () => { return false; });
        }

        public void UpdateKinematic(KinematicSteeringOutput2D steering)
        {
            Kinematic2D.Update(_kinematicStatus, steering);
        }

        public void UpdateState()
        {
            _stateMachine.Tick();
        }

        private class Idle : IState
        {
            public void OnEnter()
            {
                throw new System.NotImplementedException();
            }

            public void OnExit()
            {
                throw new System.NotImplementedException();
            }

            public void Tick() 
            { }
        }

        private class Walking : IState
        {
            public void OnEnter()
            {
                throw new System.NotImplementedException();
            }

            public void OnExit()
            {
                throw new System.NotImplementedException();
            }

            public void Tick()
            { }
        }

        private class Falling : IState
        {
            public void OnEnter()
            {
                throw new System.NotImplementedException();
            }

            public void OnExit()
            {
                throw new System.NotImplementedException();
            }

            public void Tick()
            { }
        }
    }
}


