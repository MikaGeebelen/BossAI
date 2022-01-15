using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]private float _moveSpeed = 10.0f;
    private Vector2 _moveDir = new Vector2();

    private Rigidbody2D _rigidbody = null;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        _moveDir = new Vector2(Input.GetAxis("YAxis"),Input.GetAxis("XAxis"));
        _moveDir.Normalize();
    }

    private void FixedUpdate()
    {
       _rigidbody.velocity = _moveDir * _moveSpeed;
    }
}
