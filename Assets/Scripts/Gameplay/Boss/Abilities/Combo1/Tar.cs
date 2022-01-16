using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tar : MonoBehaviour
{
    [SerializeField] private Sprite _projectileTar = null;
    [SerializeField] private Sprite _floorTar = null;

    private CircleCollider2D _tarCollider = null;
    private SpriteRenderer _renderer = null;
    private Health _health = null;
    void Start()
    {
        _tarCollider = gameObject.AddComponent<CircleCollider2D>();
        _tarCollider.radius = 0.15f;

        _renderer = gameObject.AddComponent<SpriteRenderer>();
        _renderer.sprite = _projectileTar;

        _health = gameObject.AddComponent<Health>();
        _health.Setup(25);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Pop();
    }

    private void Pop()
    {
        _tarCollider.isTrigger = true;
        _tarCollider.radius = 1.5f;
    }
}
