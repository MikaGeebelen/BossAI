using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    private Node _startNode = null;

    public Node StartNode
    {
        get
        {
            return _startNode;
        }
    }
    private Node _endNode = null;
    public Node EndNode
    {
        get
        {
            return _endNode;
        }
    }

    public Connection(Node startNode, Node endNode)
    {
        _startNode = startNode;
        _endNode = endNode;
    }

    public static bool operator ==(Connection x, SavedConnection y)
    {
        if (x == (Connection)null && y == null)
        {
            return true;
        }
        else if (x == (Connection)null || y == null)
        {
            return false;
        }



        return ((int) x._startNode.Type == y.StartNode.Type) && ((int) x._endNode.Type == y.EndNode.Type)
               || ((int)x._startNode.Type == y.EndNode.Type) && ((int)x._endNode.Type == y.StartNode.Type);
    }

    public static bool operator !=(Connection x, SavedConnection y)
    {
        return !(x == y);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

[Serializable]
public class SavedConnection
{
    public SavedNode StartNode = null;
    public SavedNode EndNode = null;
}
