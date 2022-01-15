using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "conditions", menuName = "Conditions/aliveCon", order = 2)]
public class OtherAliveCondition : BaseCondition
{
    private Boss _boss = null;
    private Health _personalHealth = null;
    private Health _healthComp = null;
    private Transform _defenseLoc = null;

    public GameObject _particlePrefab = null;

    private KeyValuePair<int, int> _otherComp;

    private bool _hasListener = false;

    private bool _addedVisual = false;

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() {Requirements.PieceIndex,Requirements.Spawner,Requirements.Origin,Requirements.OtherRandom};
    }

    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);
        _boss = board.GetValue<Boss>(initList[2]);
         _personalHealth = board.GetValue<GameObject>(initList[3]).GetComponent<Health>();
         GameObject other = board.GetValue<GameObject>(initList[4]);
         _healthComp = other.GetComponent<Health>();
         _defenseLoc = other.transform;

         string text = initList[4];
         int num = text.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
         _otherComp = new KeyValuePair<int, int>(int.Parse(text.Substring(num, 1)), int.Parse(text.Substring(num +1 , 1)));
    }

    private void ShowDefender()
    {
        _boss.SpawnProjectle(_particlePrefab, _defenseLoc.position, _defenseLoc.rotation.eulerAngles);
    }

    public override bool TestCondition()
    {
        if (!_isMovementCondition)
        {
            if (!CheckPrerequisites())
            {
                if (!_addedVisual)
                {
                    _addedVisual = true;
                    _personalHealth.Visual.SetHealthCondition((float)(_allPhaseNum - (_phaseNum + 1)) / (float)_allPhaseNum);
                }

                if (_hasListener)
                {
                    _personalHealth.Damaged.RemoveListener(ShowDefender);
                    _hasListener = false;
                }
            
                _personalHealth.CanTakeDamage(true);
                return false;
            }
            else
            {
                if (!_addedVisual)
                {
                    _addedVisual = true;
                    _personalHealth.Visual.SetHealthCondition((float)(_allPhaseNum - (_phaseNum + 1)) / (float)_allPhaseNum);
                }
            }
        }

        if (_healthComp.CurrentHealth / (float)_healthComp.MaxHealth <= 0.0f)
        {
            if (_personalAcces != "")
            {
                _board.SetValue(_personalAcces, false);
            }

            if (_hasListener)
            {
                _personalHealth.Damaged.RemoveListener(ShowDefender);
                _hasListener = false;
            }

            _personalHealth.CanTakeDamage(true);
            return false;
        }
        else
        {
            if (_personalAcces != "")
            {
                _board.SetValue(_personalAcces, true);
            }

            if (!_hasListener)
            {
                _personalHealth.Damaged.AddListener(ShowDefender);
                _hasListener = true;
            }

            _personalHealth.CanTakeDamage(false);
            return true;
        }
    }

    public override void SetupScripted(BaseCondition condition)
    {
        _particlePrefab = (condition as OtherAliveCondition)._particlePrefab;
    }

    public override KeyValuePair<int, int> GetOther()
    {
        return _otherComp;
    }
}
