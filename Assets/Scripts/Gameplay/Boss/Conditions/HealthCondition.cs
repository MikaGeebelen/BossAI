using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "conditions", menuName = "Conditions/HealthCon", order = 1)]
public class HealthCondition : BaseCondition
{
    private Health _healthComp = null;
    private int _allPhaseNum = 0;
    private bool _hasAllPhaseNum = false;
    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() {Requirements.PieceIndex,Requirements.Origin};
    }

    public override void Setup(BlackBoard board, List<string> initList)
    { 
        base.Setup(board,initList);
        _healthComp = board.GetValue<GameObject>(initList[2]).GetComponent<Health>();
        if (initList.Count > 3)
        {
            _phaseNum = 0;
        }
    }

    public override bool TestCondition()
    {
        if (!_hasAllPhaseNum)
        {
            int AllPhaseNum = 0;
            do
            {
                AllPhaseNum++;
            } while (_board.DoesValueExist(ConditionAcces + _main + _secundary + AllPhaseNum));

            _hasAllPhaseNum = true;
        }

        if ((_healthComp.CurrentHealth / _healthComp.MaxHealth) > (_phaseNum / (_allPhaseNum + 1)))
        {
            if (_personalAcces != "")
            {
                _board.SetValue(_personalAcces, true);
            }

            return true;
        }
        else
        {
            if (_personalAcces != "")
            {
                _board.SetValue(_personalAcces, false);
            }
            return false;
        }
    }
}
