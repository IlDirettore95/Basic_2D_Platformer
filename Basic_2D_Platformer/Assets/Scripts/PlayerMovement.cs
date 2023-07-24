using UnityEngine;

[RequireComponent (typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    public float WalkingSpeed = 3;

    private Transform _transform;
    private CapsuleCollider2D _collider;
    private Vector3 _input;
    private Vector3 _movement;

    private void Awake()
    {
        _transform = transform;
        _collider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        TakeInput();
        CollisionDetection();
        Move();
    }

    private void TakeInput()
    {
        _input = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
        {
            _input += Vector3.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            _input += Vector3.right;
        }
    }

    private void CollisionDetection()
    {
        _movement = _input * WalkingSpeed * Time.deltaTime;
        RaycastHit2D[] hits = Physics2D.RaycastAll(_transform.position, _movement.normalized);
        for(int i = 0; i < hits.Length; i++) 
        {
            if (hits[i].collider == _collider) continue;

            float distanceToMove = hits[i].distance - _collider.size.x / 2;
            if (distanceToMove >= _movement.magnitude) continue;

            _movement = _movement.normalized * distanceToMove;
        }
    }

    private void Move()
    {
        _transform.position += _movement;
    }
}
