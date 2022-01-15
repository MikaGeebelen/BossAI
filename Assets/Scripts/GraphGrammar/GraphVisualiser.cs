using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GraphVisualiser : MonoBehaviour
{
    //visuals
    [SerializeField] private Transform _treeStartLoc = null;
    [SerializeField] private GameObject _nodePrefab = null;
    [SerializeField] private GameObject _connectionPrefab = null;

    private List<GraphLine> _conVisuals = new List<GraphLine>();
    private List<Transform> _visualNodes = new List<Transform>();

    private List<Vector2> _previousPos = new List<Vector2>();
    [SerializeField] private float _NodeDistance = 100;
    //data
    private Graph _graph = null;

    public void registerGraph(Graph graph, UnityEvent ruleApplied)
    {
        _previousPos.Clear();
        _previousPos.Add(new Vector2(_treeStartLoc.position.x, _treeStartLoc.position.y));
        _graph = graph;
        ruleApplied.AddListener(UpdateNodes);
    }

    private void UpdateNodes()
    {
        foreach (var node in _visualNodes)
        {
            Destroy(node.gameObject);
        }
        _visualNodes.Clear();
        for (int i = 0; i < _graph.Nodes.Count; i++)
        {
            GameObject node = Instantiate(_nodePrefab);
            node.transform.position = GetNodePos(i);
            _visualNodes.Add(node.transform);

            node.GetComponentInChildren<TextMeshPro>().text = "Node " + _graph.Nodes[i].Type.ToString();
        }

        UpdateConnections();
    }

    private Vector2 GetNodePos(int index)
    {
        if (_previousPos.Count > index)
        {
            return _previousPos[index];
        }
        else
        {
            //calc new pos
            Node currentNode = _graph.Nodes[index];
            List<Connection> nodeCons = _graph.GetAllNodeConnections(currentNode);
            List<Connection> prevCons = nodeCons.FindAll(con => { return (con.EndNode.Id < currentNode.Id) || (con.StartNode.Id < currentNode.Id); });

            int id = Int32.MaxValue; //lowest connected node id

            foreach (Connection con in prevCons)
            {
                if (con.StartNode.Id < con.EndNode.Id && con.StartNode.Id < id)
                {
                    id = con.StartNode.Id;
                }
                else if (con.EndNode.Id < con.StartNode.Id && con.EndNode.Id < id)
                {
                    id = con.EndNode.Id;
                }
            }

            Node node = _graph.Nodes.Find(node => { return id == node.Id; });//find node with id
            List<Connection> parentCons = _graph.GetAllNodeConnections(node);
            if (parentCons.Count > 2 || (parentCons.Count > 1 && id == 0))//parent node has a lot of connections -> split tree
            {
                int nodeCon = 0;
                for (int i = 0; i < parentCons.Count; i++)
                {
                    if (parentCons[i].StartNode.Id == currentNode.Id || parentCons[i].EndNode.Id == currentNode.Id)
                    {
                        nodeCon = i;
                        break;
                    }
                }

                Vector2 newPos = _previousPos[id] +
                                 new Vector2(_NodeDistance, _NodeDistance/2 * Mathf.Pow(1.5f , (nodeCon -1)) * Mathf.Sign(nodeCon%2 -0.5f));
                _previousPos.Add(newPos);
                return newPos;
            }
            else
            {
                Vector2 newPos = _previousPos[id] +
                                 new Vector2(_NodeDistance, 0);
                _previousPos.Add(newPos);
                return newPos;
            }
        }
    }

    private void UpdateConnections()
    {
        foreach (var cons in _conVisuals)
        {
            Destroy(cons.gameObject);
        }
        _conVisuals.Clear();

        foreach (var connection in _graph.Connections)
        {
            GameObject con = Instantiate(_connectionPrefab);
            GraphLine line = con.GetComponent<GraphLine>();
            _conVisuals.Add(line);

            int index = _graph.Nodes.FindIndex(item => { return connection.StartNode == item; });
            Transform start = _visualNodes[index];
            index = _graph.Nodes.FindIndex(item => { return connection.EndNode == item; });
            Transform end = _visualNodes[index];

            line.SetPos(start,end);
        }
    }
}
