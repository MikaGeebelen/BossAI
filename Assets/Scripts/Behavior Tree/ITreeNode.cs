using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITreeNode
{
    public KeyValuePair<BehaviorTree.TreeState, ITreeNode> RunNode(BlackBoard board);
}
