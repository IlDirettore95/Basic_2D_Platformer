using GMDG.NoProduct.Utility;
using System;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PlayerMovement
{
    public class CharacterStatus
    {
        private Kinematic2D _kinematicStatus;
        private StateMachine _stateMachine;

        public CharacterStatus(Transform transform) 
        {
            _kinematicStatus = new Kinematic2D(transform);
            _stateMachine = new StateMachine();

            Idle idle = new Idle(_kinematicStatus);
            Walking walking = new Walking(_kinematicStatus);
            Falling falling = new Falling(_kinematicStatus);

            Func<bool> IsMoving = () => _kinematicStatus.Velocity.x != 0;
            Func<bool> IsIdle = () => _kinematicStatus.Velocity.x == 0;

            _stateMachine.AddTransition(idle, walking, IsMoving);
            _stateMachine.AddTransition(walking, idle, IsMoving);

            _stateMachine.SetState(idle);
        }

        public void UpdateState()
        {
            _stateMachine.Tick();
        }

        private class Idle : IState
        {
            public Idle(Kinematic2D kinematicStatus) { }

            public void OnEnter() { }

            public void OnExit() { }

            public void Tick() { }
        }

        private class Walking : IState
        {
            private float _walkingSpeed = 3f;
            private Kinematic2D _kinematicStatus;

            public Walking(Kinematic2D kinematicStatus) 
            { 
                _kinematicStatus = kinematicStatus;
            }

            public void OnEnter() { }

            public void OnExit() { }

            public void Tick()
            { 
                Vector2 velocity = Vector2.zero;

                if(Input.GetKey(KeyCode.A))
                {
                    velocity += Vector2.left;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    velocity += Vector2.right;
                }

                velocity *= _walkingSpeed;

                Kinematic2D.Update(_kinematicStatus, new KinematicSteeringOutput2D(velocity, 0));
            }
        }

        private class Falling : IState
        {
            public Falling(Kinematic2D kinematicStatus) { }

            public void OnEnter() { }

            public void OnExit() { }

            public void Tick() { }
        }
    }
}


