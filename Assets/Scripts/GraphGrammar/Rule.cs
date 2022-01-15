using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Rule
{
    public int PointCost = 100;
    //subgraph
    public List<SavedNode> StartNodes = new List<SavedNode>();
    public List<SavedConnection> StartConnections = new List<SavedConnection>();
    //replacement subgraph
    public List<SavedNode> NewNodes = new List<SavedNode>();
    public List<SavedConnection> NewConnections = new List<SavedConnection>();

    public Rule(int pointCost,List<SavedNode> startNodes, List<SavedNode> newNodes, List<SavedConnection> startConnections, List<SavedConnection> newConnections)
    {
        PointCost = pointCost;
        StartNodes.AddRange(startNodes);
        NewNodes.AddRange(newNodes);
        StartConnections.AddRange(startConnections);
        NewConnections.AddRange(newConnections);
    }
}
