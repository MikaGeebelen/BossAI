using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent DiedEvent = new UnityEvent();
    public UnityEvent Damaged = new UnityEvent();

    [SerializeField]private int _maxHealth = 100;
    private int _currentHealth = 0;

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
        _currentHealth -= damage;
        Damaged.Invoke();

        if (_currentHealth <= 0)
        {
            DiedEvent.Invoke();
        }
    }

    public void AttachVisual(HealthVisual visual)
    {
        _visual = visual;
    }
}
