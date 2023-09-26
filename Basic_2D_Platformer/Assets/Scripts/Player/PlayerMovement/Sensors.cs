using GMDG.NoProduct.Utility;
using UnityEngine;

namespace GMDG.Basic2DPlatformer.PlayerMovement
{
    public class Sensors
    {
        private Kinematic2D _kinematicStatus;
        private CapsuleCollider2D _collider;
        private MovementData _data;

        public bool IsGrounded { get; private set; } 
        public bool WasGrounded { get; private set; }
        public bool HasJustLanded { get; private set; }
        public bool HasJustTakenOff { get; private set; }
        public bool HasJustJumped { get; private set; }
        public bool IsPerformingALongJump { get; private set; }
        public bool HasJumpInBuffer { get; private set; }
        public bool CanPerformCoyoteJump { get; private set; }
        public float HorizontalInput { get; private set; }
        public float VerticalInput { get; private set; }
        public float FallDistance { get; private set; }
        public float MaxYReached { get; private set; }
        public float DistanceFromGround { get; private set; }
        public float DistanceFromCollision { get; private set; }

        private float currentJumpInBufferTime;
        private float currentJumpInBufferNextTime;
        private float currentCoyoteTime;
        private float currentCoyoteNextTime;

        public Sensors(Kinematic2D kinematicStatus, CapsuleCollider2D collider, MovementData data) 
        { 
            _kinematicStatus = kinematicStatus;
            _collider = collider;
            _data = data;
        }

        public void Update()
        {
            WasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();
            HasJustLanded = !WasGrounded && IsGrounded;
            HasJustTakenOff = WasGrounded && !IsGrounded;
            HorizontalInput = GetHorizontalInput();
            VerticalInput = GetVerticalInput();
            HasJustJumped = GetHasJustJumped();
            IsPerformingALongJump = GetIsPerformingALongJump();
            HasJumpInBuffer = GetHasJumpInBuffer();
            CanPerformCoyoteJump = GetCanPerformCoyoteJump();
            FallDistance = GetDistanceOfFall();
            MaxYReached = GetMaxYReached();
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

        private float GetHorizontalInput()
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

        private float GetVerticalInput()
        {
            float input = 0f;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                input = 1f;
            }
            return input;
        }

        private bool GetHasJustJumped()
        {
            if (IsGrounded && !HasJustLanded && HasJumpInBuffer)
            {
                HasJumpInBuffer = false;
                return true;
            }
            
            return IsGrounded && VerticalInput == 1  || !IsGrounded && VerticalInput == 1 && CanPerformCoyoteJump;
        }

        private bool GetIsPerformingALongJump()
        {
            return HasJustJumped || IsPerformingALongJump && Input.GetKey(KeyCode.Space) && _kinematicStatus.Velocity.y > 0;
        }

        private bool GetHasJumpInBuffer()
        {
            if (HasJustLanded && HasJumpInBuffer)
            {
                return true;
            }
            else if (!IsGrounded && VerticalInput == 1)
            {
                currentJumpInBufferTime = Time.time;
                currentJumpInBufferNextTime = currentJumpInBufferTime + _data.JumpBufferTime;
                return true;
            }
            else if (!IsGrounded && HasJumpInBuffer && currentJumpInBufferTime < currentJumpInBufferNextTime)
            {
                currentJumpInBufferTime += Time.deltaTime;
                return true;
            }

            return false;
        }

        private bool GetCanPerformCoyoteJump()
        {
            if (HasJustJumped)
            {
                return false;
            }
            else if (HasJustTakenOff && _kinematicStatus.Velocity.y <= 0)
            {
                currentCoyoteTime = Time.time;
                currentCoyoteNextTime = currentCoyoteTime + _data.JumpCoyoteTime;
                return true;
            }
            else if (!IsGrounded && CanPerformCoyoteJump && currentCoyoteTime < currentCoyoteNextTime)
            {
                currentCoyoteTime += Time.deltaTime;
                return true;
            }

            return false;
        }

        private float GetMaxYReached()
        {
            if (!IsGrounded) 
            { 
                return Mathf.Max(MaxYReached, _kinematicStatus.Position.y);
            }
            else if (!IsGrounded)
            {
                return MaxYReached;
            }

            return 0;
        }

        private float GetDistanceOfFall()
        {
            if(HasJustLanded)
            {
                return MaxYReached - _kinematicStatus.Position.y;
            }

            return 0;
        }
    }
}