using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityNode : ITreeNode
{
    private BaseAbility _ability = null;

    public void SetAbility(BaseAbility ability)
    {
        _ability = ability;
    }
    public KeyValuePair<BehaviorTree.TreeState, ITreeNode> RunNode(BlackBoard board)
    {
        return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(_ability.CastAbility(), this);
    }
}
