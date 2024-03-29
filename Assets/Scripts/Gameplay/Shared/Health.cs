using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent DiedEvent = new UnityEvent();
    public UnityEvent Damaged = new UnityEvent();
    public UnityEvent FullEvent = new UnityEvent();
    public UnityEvent Healed = new UnityEvent();

    [SerializeField]private int _maxHealth = 100;
    private int _currentHealth = 0;
    private bool _died = false;

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
    }
    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
    }

    private HealthVisual _visual = null;
    public HealthVisual Visual
    {
        get { return _visual; }
    }

    private bool _canTakeDamage = true;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void Setup(int maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Damaged.Invoke();
        if (_canTakeDamage)
        {
            _currentHealth -= damage;


            if (_currentHealth <= 0)
            {
                DiedEvent.Invoke();
                _died = true;
            }
        }
    }

    public void Heal(int health)
    {
        if (!_died)
        {
    
            _currentHealth += health;
      

            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
                FullEvent.Invoke();
            }
            Healed.Invoke();
        }
    }

    public void AttachVisual(HealthVisual visual)
    {
        _visual = visual;
    }

    public void CanTakeDamage(bool CanTakeDamage)
    {
        _canTakeDamage = CanTakeDamage;
    }
}
