using GMDG.NoProduct.Utility;
using System.Collections;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PlayerMovement
{
    public class Sensors
    {
        private Kinematic2D _kinematicStatus;
        private CapsuleCollider2D _collider;
        private MovementData _data;

        public bool IsGrounded { get; private set; }
        public float DistanceFromGround { get; private set; }
        public bool IsWalking { get; private set; }
        public bool HasJumped { get; private set; }
        public bool IsPressingJumping { get; private set; }
        public bool IsJumpingWithTollerance { get; private set; }
        public float HorizontalInput { get; private set; }
        public float VerticalInput { get; private set; }
        public float DistanceFromCollision { get; private set; }
        public float MaxYReached { get; private set; }

        private MonoBehaviour _caller;
        private IEnumerator JumpWithTolleranceTimerCoroutine;

        public Sensors(Kinematic2D kinematicStatus, CapsuleCollider2D collider, MovementData data, MonoBehaviour caller) 
        { 
            _kinematicStatus = kinematicStatus;
            _collider = collider;
            _data = data;

            _caller = caller;
            JumpWithTolleranceTimerCoroutine = JumpWithTolleranceTimer();
        }

        public void Update()
        {
            IsGrounded = CheckGrounded();
            
            HorizontalInput = GetHorizontalMovement();
            IsWalking = HorizontalInput != 0;

            VerticalInput = GetJumpMovement();
            IsPressingJumping = IsJumpButtonPressed();         
            HasJumped = VerticalInput != 0 && IsGrounded;
            MaxYReached = GetMaxYReached();
        }

        private float GetHorizontalMovement()
        {
            float input = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                input -= 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                input += 1f;
            }
            return input;
        }

        private float GetJumpMovement()
        {
            float input = 0f;
            if(Input.GetKeyDown(KeyCode.Space) || IsJumpingWithTollerance)
            {
                if (IsGrounded)
                {
                    _caller.StopCoroutine(JumpWithTolleranceTimerCoroutine);
                    IsJumpingWithTollerance = false;
                }
                else if (!IsJumpingWithTollerance)
                {
                    _caller.StopCoroutine(JumpWithTolleranceTimerCoroutine);
                    JumpWithTolleranceTimerCoroutine = JumpWithTolleranceTimer();
                    _caller.StartCoroutine(JumpWithTolleranceTimerCoroutine);
                }

                input = 1f;
            }
            return input;
        }

        private IEnumerator JumpWithTolleranceTimer()
        {
            IsJumpingWithTollerance = true;
            yield return new WaitForSeconds(_data.JumpBufferTime);
            IsJumpingWithTollerance = false;
        }

        private bool IsJumpButtonPressed()
        {
            return Input.GetKey(KeyCode.Space);
        }

        private float GetMaxYReached()
        {
            if (!IsGrounded && _kinematicStatus.Velocity.y > 0) 
            { 
                return _kinematicStatus.Position.y;
            }

            return MaxYReached;
        }

        public void ResetMaxYReached()
        {
            MaxYReached = _kinematicStatus.Position.y;
        }

        private bool CheckGrounded()
        {
            Transform transform = _kinematicStatus.Transform;

            RaycastHit2D[] hits;
            hits = Physics2D.RaycastAll(transform.position, Vector2.down);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == _collider) continue;
                if (hits[i].collider.isTrigger) continue;
                Debug.DrawLine(transform.position, hits[i].point, Color.blue);
                DistanceFromGround = hits[i].distance;
                if (_kinematicStatus.Velocity.y > 0) return false;
                if (DistanceFromGround > _collider.size.y / 2 + _data.CollisionThreashold) return false;
                transform.position += Vector3.up * (_collider.size.y / 2 + _data.CollisionThreashold / 2 - DistanceFromGround);
                return true;
            }
            DistanceFromGround = float.PositiveInfinity;
            return false;
        }

        public Vector2 CheckForCollisions(Vector2 velocity)
        {
            Transform transform = _kinematicStatus.Transform;

            RaycastHit2D[] hits;
            hits = Physics2D.CapsuleCastAll(transform.position, _collider.size, _collider.direction, 0, velocity.normalized);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == _collider) continue;
                if (hits[i].collider.isTrigger) continue;
                Debug.DrawLine(transform.position, hits[i].point, Color.red);
                DistanceFromCollision = hits[i].distance;
                if (DistanceFromCollision > velocity.magnitude * Time.deltaTime + _data.CollisionThreashold) break;
                transform.position += (Vector3)velocity.normalized * (DistanceFromCollision - _data.CollisionThreashold / 2);
                velocity = Vector2.zero;
                break;
            }

            return velocity;
        }
    }
}

