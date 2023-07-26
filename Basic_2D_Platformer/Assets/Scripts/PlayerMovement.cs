using System;
using UnityEngine;

[RequireComponent (typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    public float WalkingSpeed = 3;
    public float Gravity = 10;

    private const float COLLIDER_THREASHOLD = 0.01f;

    private Transform _transform;
    private CapsuleCollider2D _collider;
    private Vector3 _input;
    private Vector3 _movement;
    private Boolean _isGrounded = false;

    private void Awake()
    {
        _transform = transform;
        _collider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        TakeInput();
        CollisionDetection();
        GravityHandling();
        Move();
    }

    private void TakeInput()
    {
        _input = Vector3.zero;

        if (UnityEngine.Input.GetKey(KeyCode.A))
        {
            _input += Vector3.left;
        }
        if (UnityEngine.Input.GetKey(KeyCode.D))
        {
            _input += Vector3.right;
        }

        _movement = _input * WalkingSpeed * Time.deltaTime;
    }

    private void GravityHandling()
    {
        if (_isGrounded) return;

        _movement += Vector3.down * Gravity * Time.deltaTime; 
    }

    private void CollisionDetection()
    {
        RaycastHit2D[] hits;
        hits = Physics2D.CapsuleCastAll(_transform.position, _collider.size, _collider.direction, 0, _movement.normalized);
        for(int i = 0; i < hits.Length; i++) 
        {
            if (hits[i].collider == _collider) continue;
            if (hits[i].distance - COLLIDER_THREASHOLD >= _movement.magnitude) continue;
            Debug.DrawLine(_transform.position, hits[i].point);
            _movement = _movement.normalized * (hits[i].distance - COLLIDER_THREASHOLD);
        }

        hits = Physics2D.RaycastAll(_transform.position + Vector3.down * (_collider.size.y / 2 + COLLIDER_THREASHOLD), Vector3.down, 0);
        for(int i = 0; i < hits.Length; i++)
        {
            Debug.DrawLine(_transform.position + Vector3.down * _collider.size.y / 2, hits[i].point);
            _movement += Vector3.up * (hits[i].point.y + _collider.size.y / 2 + COLLIDER_THREASHOLD);
        }
        _isGrounded = hits.Length > 0;
    }

    private void Move()
    {
        _transform.position += _movement;
    }


}
