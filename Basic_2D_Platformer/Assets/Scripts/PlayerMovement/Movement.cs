using GMDG.NoProduct.Utility;
using System;
using UnityEngine;


namespace GMDG.Basic_2D_Platformer.PlayerMovement
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] MovementData _data;

        private Kinematic2D _kinematicStatus;
        private Sensors _sensors;
        private StateMachine _stateMachine;

        private Vector2 _velocity;

        private void Awake()
        {
            _kinematicStatus = new Kinematic2D(transform);
            _sensors = new Sensors(_kinematicStatus, GetComponent<CapsuleCollider2D>(), _data);
            _stateMachine = new StateMachine();
            
            BuildStateMachine();
        }

        private void BuildStateMachine()
        {
            Idle idle = new Idle(this);
            Walking walking = new Walking(this);
            Falling falling = new Falling(this);

            _stateMachine.AddTransition(idle, walking, () => _sensors.IsWalking && _sensors.IsGrounded);
            _stateMachine.AddTransition(walking, idle, () => !_sensors.IsWalking && _sensors.IsGrounded);
            _stateMachine.AddTransition(idle, falling, () => _sensors.IsJumping || !_sensors.IsGrounded);
            _stateMachine.AddTransition(falling, idle, () => !_sensors.IsWalking && _sensors.IsGrounded);
            _stateMachine.AddTransition(walking, falling, () => _sensors.IsJumping || !_sensors.IsGrounded);
            _stateMachine.AddTransition(falling, walking, () => _sensors.IsWalking && _sensors.IsGrounded);

            _stateMachine.SetState(idle);
        }

        private void Update()
        {
            UpdateSensors();
            UpdateStateMachine();
            CheckForCollisions();
            ExecuteMovement();
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            GUI.Box(new Rect(10, 10, 300, 40), "State: " + _stateMachine.GetState().GetType().Name, style);
            GUI.Box(new Rect(10, 50, 300, 60), string.Format("IsGrounded: {0} {1}\tDistance: {2}", _sensors.IsGrounded, Environment.NewLine, _sensors.DistanceFromGround), style);
            GUI.Box(new Rect(10, 110, 300, 40), "IsOnEdge: " + _sensors.IsOnEdge, style);
            GUI.Box(new Rect(10, 150, 300, 60), string.Format("DistanceFromCollision: {0}", _sensors.DistanceFromCollision), style);
        }

        private void UpdateSensors()
        {
            _sensors.Update();
        }

        private void UpdateStateMachine()
        {
            _stateMachine.Tick();
        }

        private void CheckForCollisions()
        {
            _velocity.x = _sensors.CheckForCollisions(Vector2.right *_velocity.x).x;
            _velocity.y = _sensors.CheckForCollisions(Vector2.up * _velocity.y).y;
        }

        private void ExecuteMovement()
        {
            Kinematic2D.Update(_kinematicStatus, new KinematicSteeringOutput2D(_velocity, 0));
        }

        #region States

        private class Idle : IState
        {
            private Movement _movement;

            public Idle(Movement movement) 
            { 
                _movement = movement;
            }

            public void OnEnter() 
            {
                _movement._velocity = Vector2.zero;
            }

            public void OnExit() { }

            public void Tick() { }
        }

        private class Walking : IState
        {
            private Movement _movement;

            public Walking(Movement movement) 
            { 
                _movement = movement;
            }

            public void OnEnter() { }

            public void OnExit() { }

            public void Tick()
            {
                _movement._velocity = Vector2.right * _movement._sensors.HorizontalInput * _movement._data.WalkingSpeed;
            }
        }

        private class Falling : IState
        {
            private Movement _movement;

            public Falling(Movement movement)
            {
                _movement = movement;
            }

            public void OnEnter() 
            {
                if (!_movement._sensors.IsJumping) return;
                _movement._velocity = new Vector2(_movement._velocity.x, _movement._sensors.VerticalInput * _movement._data.JumpForce);
            }

            public void OnExit() { }

            public void Tick() 
            {
                _movement._velocity.y -= _movement._velocity.y > 0 ? _movement._data.Gravity * Time.deltaTime : _movement._data.Gravity * 2 * Time.deltaTime;
                _movement._velocity.x = _movement._sensors.HorizontalInput * _movement._data.WalkingSpeed;
            }
        }

        #endregion
    }
}
