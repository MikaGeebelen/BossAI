using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : ITreeNode
{
    private BaseCondition _condition = null;

    public void Setup(BaseCondition condition)
    {
        _condition = condition;
    }

    public KeyValuePair<BehaviorTree.TreeState, ITreeNode> RunNode(BlackBoard board)
    {
        if (_condition.TestCondition())
        {
            return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(BehaviorTree.TreeState.Succes, this);
        }
        else
        {
            return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(BehaviorTree.TreeState.Failed, this);
        }
    }
}
