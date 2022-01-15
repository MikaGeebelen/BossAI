using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "conditions", menuName = "Conditions/HealthCon", order = 1)]
public class HealthCondition : BaseCondition
{
    private Health _healthComp = null;
    private bool _addedVisual = false;

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() {Requirements.PieceIndex,Requirements.Origin};
    }

    public override void Setup(BlackBoard board, List<string> initList)
    { 
        base.Setup(board,initList);
        _healthComp = board.GetValue<GameObject>(initList[2]).GetComponent<Health>();
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
                    _healthComp.Visual.SetHealthCondition((float)(_allPhaseNum - (_phaseNum + 1)) / (float)_allPhaseNum);
                }
                return false;
            }
            else
            {
                if (!_addedVisual)
                {
                    _addedVisual = true;
                    _healthComp.Visual.SetHealthCondition((float)(_allPhaseNum - (_phaseNum + 1)) / (float)_allPhaseNum);
                }
            }
        }



        float healthThreshold = ((float) (_allPhaseNum - (_phaseNum + 1)) / (float) _allPhaseNum);
        if (_isMovementCondition)
        {
            healthThreshold = 0;
        }

        if (((float)_healthComp.CurrentHealth / (float)_healthComp.MaxHealth) > healthThreshold)
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

    public override void SetupScripted(BaseCondition condition)
    {
        //nothing to setup from scriptable object
    }
}
