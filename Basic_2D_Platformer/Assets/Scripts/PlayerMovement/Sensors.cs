using GMDG.NoProduct.Utility;
using UnityEngine;

namespace GMDG.Basic_2D_Platformer.PlayerMovement
{
    public class Sensors
    {
        private Kinematic2D _kinematicStatus;
        private CapsuleCollider2D _collider;
        private MovementData _data;

        public bool IsGrounded { get; private set; }
        public float DistanceFromGround { get; private set; }
        public bool IsWalking { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsOnEdge { get; private set; }

        public float HorizontalInput { get; private set; }
        public float VerticalInput { get; private set; }

        public float DistanceFromCollision { get; private set; }

        public Sensors(Kinematic2D kinematicStatus, CapsuleCollider2D collider, MovementData data) 
        { 
            _kinematicStatus = kinematicStatus;
            _collider = collider;
            _data = data;
        }

        public void Update()
        {
            IsGrounded = CheckGrounded();
            HorizontalInput = GetHorizontalMovement();
            VerticalInput = GetJumpMovement();
            IsWalking = HorizontalInput != 0;
            IsJumping = VerticalInput != 0;
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
            if(Input.GetKeyDown(KeyCode.Space))
            {
                input = 1f;
            }
            return input;
        }

        private bool CheckGrounded()
        {
            Transform transform = _kinematicStatus.Transform;

            RaycastHit2D[] hits;
            hits = Physics2D.RaycastAll(transform.position, Vector2.down);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == _collider) continue;
                Debug.DrawLine(transform.position, hits[i].point, Color.blue);
                DistanceFromGround = hits[i].distance;
                if (_kinematicStatus.Velocity.y > 0) return false;
                if (hits[i].distance > _collider.size.y / 2 + _data.CollisionThreashold) return false;
                transform.position += Vector3.up * (_collider.size.y / 2 + _data.CollisionThreashold / 2 - hits[i].distance);
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
                Debug.DrawLine(transform.position, hits[i].point, Color.red);
                DistanceFromCollision = hits[i].distance;
                if (_kinematicStatus.Velocity.y < 0 && !IsGrounded && hits[i].distance <= _data.CollisionThreashold)
                {
                    IsOnEdge = true;
                }
                else
                {
                    IsOnEdge = false;
                }
                if (hits[i].distance > velocity.magnitude * Time.deltaTime + _data.CollisionThreashold) break;
                transform.position += (Vector3)velocity.normalized * (hits[i].distance - _data.CollisionThreashold / 2);
                velocity = Vector2.zero;
                break;
            }

            return velocity;
        }
    }
}

