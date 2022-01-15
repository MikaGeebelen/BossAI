using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class Graph
{
    //event
    public UnityEvent RuleAppliedEvent = new UnityEvent();
    //set data
    private List<Node> _nodes = new List<Node>();
    public List<Node> Nodes {
        get
        {
            return _nodes;
        }
    }
    private List<Connection> _connections = new List<Connection>();
    public List<Connection> Connections
    {
        get
        {
            return _connections;
        }
    }
    //helper functions for graph
    private int _id = 0;
    public Node AddNode(NodeType type)
    {
        Node newNode = new Node(_id, type);
        Nodes.Add(newNode);
        _id++;

        return newNode;
    }
    public List<Connection> GetAllNodeConnections(Node node)
    {
        if (node == null)
        {
            return new List<Connection>();
        }
        return Connections.FindAll(con => { return ((con.StartNode.Id == node.Id) || (con.EndNode.Id == node.Id)); });
    }
    public void Connect(Node startNode, Node endNode)
    {
        Connection con = new Connection(startNode, endNode);
        Connections.Add(con);
    }
    public bool ApplyRule(Rule rule) 
    { 
        var subgraph = FindSubgraph(rule.StartNodes, rule.StartConnections);
        if (subgraph.Count <= 0 || subgraph.Count < rule.StartNodes.Count)
        {
            return false;
        }
        ReplaceSubgraph(ref subgraph,rule);
        RuleAppliedEvent.Invoke();
        return true;
    }
    private void ShuffleList<T>(List<T> nodes)
    {
        int n = nodes.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0,n+1);
            T value = nodes[k];
            nodes[k] = nodes[n];
            nodes[n] = value;
        }
    }
    public void UpdateVisual()
    {
        RuleAppliedEvent.Invoke();
    }
    //algorithms - graph rewriting
    private List<Node> FindSubgraph(List<SavedNode> nodes, List<SavedConnection> cons)
    {
        if (_nodes.Count < nodes.Count)
        {
            return new List<Node>(); // not enough nodes in main graph 
        }

        List<Node> subGraph = new List<Node>();
        List<Node> startNodes = Nodes.FindAll(node => {return (int)node.Type == nodes[0].Type; }); //find the first node in the graph

        ShuffleList(startNodes);

        foreach (Node node in startNodes)
        {
            if (ConfirmNode(node, nodes, cons, ref subGraph))
            {
                break;
            }
            subGraph.Clear();
        }

        return subGraph;
    }
    private bool ConfirmNode(Node node,List<SavedNode> nodes, List<SavedConnection> cons, ref List<Node> subGraph)
    {
        if (subGraph.Contains(node))//prevents endless loops
        {
            return true; // already checked this node
        }

        subGraph.Add(node);
        int id = subGraph.Count-1;
        List<SavedConnection> reqCons = new List<SavedConnection>();
        foreach (var con in cons)
        {
            if ((id < 0) && (con.StartNode.Type == (int)node.Type))
            {
                id = con.StartNode.Id;
                reqCons.Add(con);
            }
            else if ((con.StartNode.Id == id) || (con.EndNode.Id == node.Id))
            {
                reqCons.Add(con);
            }
        } //find all required connections to be a valid subgraph

        var connections = _connections.FindAll(con => { return (con.StartNode.Id == node.Id) || (con.EndNode.Id == node.Id); }); //find all connection to this node in the graph

        bool foundNeighbor = false;

        List<Connection> testedCons = new List<Connection>();
        foreach (var con in reqCons)
        {
            var testCons = connections.FindAll(eCon =>
            {
                foreach (var testedCon in testedCons)
                {
                    if (testedCon == eCon)
                    {
                        return false;//don't reuse confirmed connections prevents finding same connection twice
                    }
                }
                return eCon == con;
            });
            foundNeighbor = false;
            foreach (var reqCon in testCons)
            {
                if (reqCon.StartNode == node)
                {
                    if (ConfirmNode(reqCon.EndNode, nodes, cons, ref subGraph))
                    {
                        testedCons.Add(reqCon);
                        foundNeighbor = true;
                        break;
                    }
                }
                else
                {
                    if (ConfirmNode(reqCon.StartNode, nodes, cons, ref subGraph))
                    {
                        testedCons.Add(reqCon);
                        foundNeighbor = true;
                        break;
                    }
                }

            }

            if (!foundNeighbor)
            {
                subGraph.Remove(node);
                return false;
            }
        }

        return true;
    }
    private void ReplaceSubgraph(ref List<Node> nodes, Rule rule)
    {
        List<int> nodeConnections = new List<int>();
        for (int i = 0; i < rule.StartNodes.Count; i++)
        {
            int conAmount = 0;
            foreach (var cons in rule.StartConnections)
            {
                if (cons.StartNode.Id == rule.StartNodes[i].Id || cons.EndNode.Id == rule.StartNodes[i].Id)
                {
                    conAmount++;
                }
            }
            nodeConnections.Add(conAmount);
        }


        List<Node> spawnedNodes = new List<Node>();
        bool hasToRepeat = false;
        do
        {
            hasToRepeat = false;
            //add new connections if space available
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Type = (NodeType) rule.NewNodes[i].Type;

                var cons = rule.NewConnections.FindAll(con =>
                {
                    return (rule.NewNodes[i].Id == con.StartNode.Id) ||
                           (rule.NewNodes[i].Id == con.EndNode.Id);
                });

                while (nodeConnections[i] < cons.Count)
                {
                    if (nodes.Count < rule.NewNodes.Count)
                    {
                        Node node = AddNode((NodeType) rule.NewNodes[nodes.Count].Type);
                        nodes.Add(node);
                        Connect(nodes[nodes.Count - 1], nodes[i]);
                        spawnedNodes.Add(node);
                        nodeConnections.Add(1);
                        nodeConnections[i]++;
                    }
                    else
                    {
                        for (int j = 0; j < rule.NewNodes.Count; j++)
                        {
                            var consNodej = rule.NewConnections.FindAll(con =>
                            {
                                return (rule.NewNodes[j].Id == con.StartNode.Id) ||
                                       (rule.NewNodes[j].Id == con.EndNode.Id);
                            });

                            if (i != j && nodeConnections[j] < consNodej.Count)
                            {
                                Connect(nodes[i], nodes[j]);
                                nodeConnections[i]++;
                                nodeConnections[j]++;
                            }
                        }
                    }
                }
            }
            //break connection and create new node if no opening
            if (nodes.Count < rule.NewNodes.Count)
            {
                Node node = AddNode((NodeType)rule.NewNodes[nodes.Count].Type);
                nodes.Add(node);
                nodeConnections.Add(0);
                Connection con = GetAllNodeConnections(nodes[nodes.Count - 2])[0];

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (con.StartNode == nodes[i])
                    {
                        nodeConnections[i]--;
                    }
                    if (con.EndNode == nodes[i])
                    {
                        nodeConnections[i]--;
                    }
                }

                Connections.Remove(con); //remove random connection to make space
                Connect(nodes[nodes.Count - 1], nodes[nodes.Count-2]); //connect to last node
                nodeConnections[nodes.Count - 1]++;
                nodeConnections[nodes.Count - 2]++;

                spawnedNodes.Add(node);
                hasToRepeat = true;
            }
        } while (nodes.Count < rule.NewNodes.Count || hasToRepeat);
    }

    /*after gen connections
     - find all condition nodes -> they lock next phase abilities 
     - find all stat nodes -> they hold health which is the most common condition
     - find all secundary piece nodes -> follow to end save phase end nodes
     - save each phaseStart node in layers till the phase end node
    */

}

public class SavedGraph
{
    public List<SavedNode> Nodes = new List<SavedNode>();
    public List<SavedConnection> Connections = new List<SavedConnection>();

    public SavedGraph(Graph graph)
    {
        foreach (Node node in graph.Nodes)
        {
            Nodes.Add(new SavedNode(){Id = node.Id,Type = (int)node.Type});
        }


        foreach (Connection con in graph.Connections)
        {
            int startIndex = graph.Nodes.FindIndex(node => { return con.StartNode == node; });
            SavedNode start = Nodes[startIndex];

            int endIndex = graph.Nodes.FindIndex(node => { return con.EndNode == node; });
            SavedNode end = Nodes[endIndex];

            Connections.Add(new SavedConnection() {StartNode = start,EndNode = end});
        }
    }

    public Graph ConvertToGraph()
    {
        Graph graph = new Graph();
        foreach (SavedNode node in Nodes)
        {
            graph.AddNode((NodeType)node.Type);
        }

        foreach (SavedConnection con in Connections)
        {
            int startIndex = Nodes.FindIndex(node => { return node.Id == con.StartNode.Id; });
            int endIndex = Nodes.FindIndex(node => { return node.Id == con.EndNode.Id; });


            graph.Connect(graph.Nodes[startIndex],graph.Nodes[endIndex]);
        }

        return graph;
    }
}