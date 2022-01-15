using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementNode : ITreeNode
{
    //will be chosen during conversion
    private BaseMovement _movement = null;

    public void SetMovementBehavior(BaseMovement movement)
    {
        _movement = movement;
    }

    public KeyValuePair<BehaviorTree.TreeState, ITreeNode> RunNode(BlackBoard board)
    {
        _movement.Move();
        return new KeyValuePair<BehaviorTree.TreeState, ITreeNode>(BehaviorTree.TreeState.Succes, this);
    }

}
