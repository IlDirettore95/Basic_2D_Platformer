using GMDG.NoProduct.Utility;
using EventManager = GMDG.Basic2DPlatformer.System.EventManager;
using Event = GMDG.Basic2DPlatformer.System.Event;
using System;
using UnityEngine;


namespace GMDG.Basic2DPlatformer.PlayerMovement
{
    public class Movement : MonoBehaviour
    {
        public Vector2 Velocity { get { return _velocity; } }
        public Sensors Sensors { get; private set; }

        [SerializeField] MovementData _data;

        private Kinematic2D _kinematicStatus;
        private StateMachine _stateMachine;

        private Vector2 _velocity;


        private void Awake()
        {
            _kinematicStatus = new Kinematic2D(transform);
            Sensors = new Sensors(_kinematicStatus, GetComponent<CapsuleCollider2D>(), _data);
            _stateMachine = new StateMachine();
            
            BuildStateMachine();
        }

        private void BuildStateMachine()
        {
            Idle idle = new Idle(this);
            Walking walking = new Walking(this);
            Falling falling = new Falling(this);

            _stateMachine.AddTransition(idle, walking, () => Sensors.HorizontalInput != 0 && Sensors.IsGrounded);
            _stateMachine.AddTransition(walking, idle, () => Sensors.HorizontalInput == 0 && Sensors.IsGrounded);
            _stateMachine.AddTransition(idle, falling, () => Sensors.HasJustJumped || !Sensors.IsGrounded);
            _stateMachine.AddTransition(falling, idle, () => Sensors.HorizontalInput == 0 && Sensors.IsGrounded);
            _stateMachine.AddTransition(walking, falling, () => Sensors.HasJustJumped || !Sensors.IsGrounded);
            _stateMachine.AddTransition(falling, walking, () => Sensors.HorizontalInput != 0 && Sensors.IsGrounded);

            _stateMachine.SetState(idle);
        }

        private void Update()
        {
            UpdateSensors();
            UpdateStateMachine();
            CheckForCollisions();
            ExecuteMovement();
        }

        //private void OnGUI()
        //{
        //    GUIStyle style = new GUIStyle();
        //    style.fontSize = 24;
        //    style.normal.textColor = Color.white;
        //    GUI.Box(new Rect(10, 20, 300, 40), "State: " + _stateMachine.GetState().GetType().Name, style);
        //    GUI.Box(new Rect(10, 60, 300, 40), string.Format("IsGrounded: {0} ({1:F2}) Land: {2} TakenOff: {3}", _sensors.IsGrounded, _sensors.DistanceFromGround, _sensors.HasJustLanded, _sensors.HasJustTakenOff), style);
        //    GUI.Box(new Rect(10, 100, 300, 40), string.Format("HasJumped: {0}", _sensors.HasJustJumped), style);
        //    GUI.Box(new Rect(10, 140, 300, 40), string.Format("LongJump: {0}", _sensors.IsPerformingALongJump), style);
        //    GUI.Box(new Rect(10, 180, 300, 40), string.Format("HasJumpInBuffer: {0}", _sensors.HasJumpInBuffer), style);
        //    GUI.Box(new Rect(10, 220, 300, 40), string.Format("CanCoyoteJump: {0}", _sensors.CanPerformCoyoteJump), style);
        //    GUI.Box(new Rect(10, 260, 300, 40), string.Format("FallDistance: {0}", _sensors.FallDistance), style);
        //    GUI.Box(new Rect(10, 300, 300, 40), string.Format("MaxYReached: {0}", _sensors.MaxYReached), style);
        //    GUI.Box(new Rect(10, 340, 300, 40), string.Format("HorizontalInput: {0}", _sensors.HorizontalInput), style);
        //    GUI.Box(new Rect(10, 380, 300, 40), string.Format("VerticalInput: {0}", _sensors.VerticalInput), style);
        //    GUI.Box(new Rect(10, 420, 300, 40), string.Format("DistanceFromCollision: {0}", _sensors.DistanceFromCollision), style);
        //    GUI.Box(new Rect(10, 460, 300, 40), string.Format("Velocity: {0}", _velocity), style);
        //}

        private void UpdateSensors()
        {
            Sensors.Update();
        }

        private void UpdateStateMachine()
        {
            _stateMachine.Tick();
        }

        private void CheckForCollisions()
        {
            _velocity.x = Sensors.CheckForCollisions(Vector2.right *_velocity.x).x;
            _velocity.y = Sensors.CheckForCollisions(Vector2.up * _velocity.y).y;
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

            public void OnEnter(IState from) 
            {
                _movement._velocity = Vector2.zero;
            }

            public void OnExit(IState to) { }

            public void Tick() { }
        }

        private class Walking : IState
        {
            private Movement _movement;

            private float _currentBuildUp;
            private float _currentSpeed;
            private float _oldSpeed;

            public Walking(Movement movement) 
            { 
                _movement = movement;
            }

            public void OnEnter(IState from) 
            {
                _currentBuildUp = 0;

                if (from is Idle)
                {
                    _oldSpeed = 0;
                }
                else if (from is Falling)
                {
                    _oldSpeed = _movement._data.FallingSpeed;
                }

                EventManager.Instance.Publish(Event.OnPlayerWalking, _movement);
            }

            public void OnExit(IState to) { }

            public void Tick()
            {
                _currentBuildUp += _movement._data.WalkingBuildUpSpeed * Time.deltaTime;
                _currentSpeed = Mathf.Lerp(_oldSpeed, _movement._data.WalkingSpeed, _currentBuildUp);
                _movement._velocity = Vector2.right * _movement.Sensors.HorizontalInput * _currentSpeed;
            }
        }

        private class Falling : IState
        {
            private Movement _movement;

            private bool _waitForInput;

            private float _currentBuildUp;
            private float _currentSpeed;
            private float _oldSpeed;

            public Falling(Movement movement)
            {
                _movement = movement;
            }

            public void OnEnter(IState from) 
            {
                _currentBuildUp = 0;

                if (from is Idle) 
                { 
                    _oldSpeed = _movement._data.WalkingSpeed / 2;
                }
                else if (from is Walking)
                {
                    _oldSpeed = _movement._data.WalkingSpeed;
                }
            }

            public void OnExit(IState to) 
            {
                if (to is Idle || to is Walking)
                {
                    if (_movement.Sensors.FallDistance > _movement._data.FallDamageYThreshold) 
                    {
                        EventManager.Instance.Publish(Event.OnFallDamageTaken);
                    }
                }
            }

            public void Tick() 
            {
                if (_movement.Sensors.HasJustJumped)
                {
                    _movement._velocity = new Vector2(_movement._velocity.x, _movement._data.JumpForce);
                }

                float gravityMultiplier = 0;
                if (_movement.Sensors.IsPerformingALongJump)
                {
                    gravityMultiplier = 1;
                }
                else if (_movement._velocity.y > 0)
                {
                    gravityMultiplier = _movement._data.GravityMultiplier * 0.75f;                
                }
                else if (_movement._velocity.y <= 0)
                {
                    gravityMultiplier = _movement._data.GravityMultiplier;
                }

                _movement._velocity.y -= _movement._data.Gravity * gravityMultiplier * Time.deltaTime;
                _movement._velocity.y = Mathf.Clamp(_movement._velocity.y, -_movement._data.MaxYSpeed, _movement._data.MaxYSpeed);

                _currentBuildUp += _movement._data.FallingBuildUpSpeed * Time.deltaTime;
                _currentSpeed = Mathf.Lerp(_oldSpeed, _movement._data.FallingSpeed, _currentBuildUp);
                _movement._velocity.x = _movement.Sensors.HorizontalInput * _currentSpeed;
            }
        }

        #endregion
    }
}