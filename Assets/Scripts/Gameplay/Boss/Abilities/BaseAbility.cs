using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Requirements
{
    Spawner,
    Target,
    Origin,
    MovementOrigin,
    PieceIndex
}

abstract public class BaseAbility : ScriptableObject
{
    public enum AbilityStates
    {
        Firing,
        Fired,
        Recharging
    }

    public int PointCost = 0;
    public bool ChangesPosition = false;

    public float MaxCooldown = 5.0f;
    private float _currentTime = 0.0f;

    protected bool _isOnCooldown = false;
    abstract public List<Requirements> GetRequirements();
    public virtual void Setup(BlackBoard board, List<string> initList)
    {
        UpdateCaller.OnUpdate.AddListener(Update);
    }

    abstract public void SetupScripted(BaseAbility ability);
    protected virtual void Update()
    {
        if (_isOnCooldown)
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > MaxCooldown)
            {
                _currentTime = 0;
                _isOnCooldown = false;
            }
        }
    }
    abstract public BehaviorTree.TreeState CastAbility();
}
