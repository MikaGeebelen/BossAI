using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : ITreeNode
{
    private List<ITreeNode> _subNodes = new List<ITreeNode>();
    public List<ITreeNode> Nodes
    {
        get { return _subNodes; }
    }

    private bool _isRunning = false;
    private ITreeNode _runningNode = null;

    public Sequence(List<ITreeNode> nodes)
    {
        _subNodes = nodes;
    }

    public KeyValuePair<BehaviorTree.TreeState, ITreeNode> RunNode(BlackBoard board)
    {
        KeyValuePair<BehaviorTree.TreeState, ITreeNode> state = new KeyValuePair<BehaviorTree.TreeState, ITreeNode>();
        if (_isRunning)
        {
            state = _runningNode.RunNode(board);
            if (state.Key == BehaviorTree.TreeState.Running)
            {
                return state;
            }
            else
            {
                _isRunning = false;
                _runningNode = null;
                return state;
            }
        }

        foreach (ITreeNode nodes in _subNodes)
        {
            state = nodes.RunNode(board);
            if (state.Key == BehaviorTree.TreeState.Failed)
            {
                break;
            }
            else if (state.Key == BehaviorTree.TreeState.Running)
            {
                _isRunning = true;
                _runningNode = state.Value;
                return state;
            }
        }
        return state;
    }
}
