using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BehaviorTree
{
    public enum TreeState
    {
        Succes,
        Running,
        Failed,
        comboFail
    }

    //list of all conditions sequances each holding their actions and sequances
    private List<ITreeNode> _behaviorTree = new List<ITreeNode>();

    public List<ITreeNode> Nodes
    {
        get { return _behaviorTree; }
    }


    private bool _isTreeActive = false;
    private bool _addedUpdater = false;
    public bool IsTreeActive => _isTreeActive;
    private BlackBoard _blackBoard = null;

    public BehaviorTree(List<ITreeNode> nodes)
    {
        _behaviorTree = nodes;
    }

    public void startTree()
    {
        if (!_addedUpdater)
        {
            UpdateCaller.OnUpdate.AddListener(Update);
            _addedUpdater = true;
        }

        _isTreeActive = true;
    }

    public void PauseTree()
    {
        _isTreeActive = false;
    }

    public void SetBlackBoard(BlackBoard board)
    {
        _blackBoard = board;
    }

    private void Update()
    {
        if (_isTreeActive)
        {
            foreach (ITreeNode node in _behaviorTree)
            {
                node.RunNode(_blackBoard);
            }
        }
    }
}
