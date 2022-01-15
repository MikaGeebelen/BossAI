using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] private float _bulletSpeed = 15.0f;
    [SerializeField] private float _lifeTime = 2.0f;
    private Rigidbody2D _rigidbody = null;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        Destroy(gameObject, _lifeTime);
    }
    private void FixedUpdate()
    {
        _rigidbody.velocity = transform.right * _bulletSpeed;
    }
}
