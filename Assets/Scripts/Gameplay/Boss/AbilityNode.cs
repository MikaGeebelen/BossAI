using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityNode : ITreeNode
{
    private bool _isCombo = false;

    private BaseAbility _ability = null;
    private BaseAbility _abilityBackup = null;


    public bool IsCombo => _isCombo;

    public void SetAbility(BaseAbility ability)
    {
        _ability = ability;
    }

    public void SetupCombo(BaseAbility ability, BaseAbility abilityBackup)
    {
        _isCombo = true;
        _ability = ability;
        _abilityBackup = abilityBackup;
    }


    public KeyValuePair<BehaviorTree.TreeState, ITreeNode> RunNode(BlackBoard board)
    {
        if (!_isCombo)
        {
            return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(_ability.CastAbility(), this);
        }
        else
        {
            BehaviorTree.TreeState state =_ability.CastAbility();
            if (state == BehaviorTree.TreeState.comboFail)
            {
                if (_abilityBackup != null)
                {
                    return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(_abilityBackup.CastAbility(), this);
                }
            }

            return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(state, this);
        }
    }
}
