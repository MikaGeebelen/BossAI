using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tar : MonoBehaviour
{
    private Sprite _projectileTar = null;

    private CircleCollider2D _tarCollider = null;
    private SpriteRenderer _renderer = null;
    private Health _health = null;

    private float _speed = 6.0f;
    private Vector3 _target = new Vector3();

    private bool _isFired = false;
    private bool _isOnFire = false;
    public bool IsOnFire => _isOnFire;

    private PlayerMovement _movement = null;
    private Health _burningHealth = null;

    private GameObject _fireParticle = null;

    private const int maxHealth = 10;

    private bool _hasPopped = false;



    private void Awake()
    {
        _tarCollider = gameObject.AddComponent<CircleCollider2D>();
        _tarCollider.radius = 0.1f;

        _renderer = gameObject.AddComponent<SpriteRenderer>();


        _health = gameObject.AddComponent<Health>();
        _health.Setup(maxHealth);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Vector3.Distance(_target,transform.position) < 0.1f && !_hasPopped)
        {
            _hasPopped = true;
            Pop();
        }

        if (_health.CurrentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (_isFired)
        {
            Vector2 moveDir = _target - transform.position;
            moveDir.Normalize();

            transform.Translate(moveDir * _speed * Time.fixedDeltaTime);
        }
    }

    public void Fire(Vector3 target)
    {
        _target = target;
        _isFired = true;
        _health.Setup(maxHealth);

        ResetTar();
    }

    private void Pop()
    {
        _isFired = false;
        _tarCollider.isTrigger = true;
        transform.localScale = new Vector3(30, 30);
        _renderer.color = Color.grey;
    }

    private void SetFire()
    {
        _renderer.color = Color.red;
        _isOnFire = true;
    }

    private void ResetTar()
    {
        _hasPopped = false;
        _isOnFire = false;
        _renderer.color = Color.white;
        transform.localScale = new Vector3(5, 5);
        _tarCollider.isTrigger = false;
        if (_movement != null)
        {
            _movement.SetSlow(false);
            _movement = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            _movement = col.GetComponent<PlayerMovement>();
            _movement.SetSlow(true);
        }

        if (col.CompareTag("Fire"))
        {
            if (!_isOnFire)
            {
                SetFire();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (_movement != null)
            {
                _movement.SetSlow(false);
                _movement = null;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (_isOnFire)
        {
            if (col.CompareTag("Player"))
            {
                if (_burningHealth == null)
                {
                    _burningHealth = col.GetComponent<Health>();
                    StartCoroutine(OnFire());
                }
            }
        }
    }


    private IEnumerator OnFire()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.5f);
            _burningHealth.TakeDamage(5);
            Instantiate(_fireParticle, _burningHealth.transform.position, Quaternion.identity);
        }

        _burningHealth = null;
        yield return 0;
    }

    public void AddVisuals(Sprite projectile, GameObject particle)
    {
        _projectileTar = projectile;
        _renderer.sprite = _projectileTar;
        _fireParticle = particle;
    }
}
