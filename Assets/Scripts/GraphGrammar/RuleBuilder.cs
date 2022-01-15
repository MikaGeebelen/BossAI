using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RuleBuilder : MonoBehaviour
{
    //visuals
    [SerializeField] private GameObject _canvas = null;
    [SerializeField] private GameObject _nodePrefab = null;
    [SerializeField] private GameObject _connectionPrefab = null;

    private List<GraphLine> _conVisuals = new List<GraphLine>();
    //data
    [SerializeField] private GraphGrammar _graph = null;

    [SerializeField] private int _pointCost = 50;
    //subgraph
    private List<Transform> _visualStartNodes = new List<Transform>();
    private List<SavedNode> _startNodes = new List<SavedNode>();
    private List<SavedConnection> _startConnections = new List<SavedConnection>();
    //replacement subgraph
    private List<Transform> _visualNewNodes = new List<Transform>();
    private List<SavedNode> _newNodes = new List<SavedNode>();
    private List<SavedConnection> _newConnections = new List<SavedConnection>();

    private bool _isStartGraph = true;
    private bool _isConnecting = true;

    private int _startConNodeIndex = -1;

    private int _BaseId = 0;
    private int _NewId = 0;

    [SerializeField] private bool _isPlayMode = false;

    void Update()
    {
        if (!_isPlayMode)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            if (Input.GetButtonDown("PlaceNode"))
            {
                PlaceNode(mousePos);
            }
            else if (Input.GetButtonDown("ConChaNode"))
            {
                UpdateConnections();
            }
  
            else if (Input.GetButtonDown("ToggleRuleBuilder"))
            {
                _isStartGraph = !_isStartGraph;
                _startConNodeIndex = -1;

            }
            else if (Input.GetButtonDown("ConCha"))
            {
                _isConnecting = !_isConnecting;
                _startConNodeIndex = -1;
            }
            else if (Input.GetButtonDown("SaveRule"))
            {
                Rule newRule = new Rule(_pointCost, _startNodes, _newNodes, _startConnections, _newConnections);

                _graph.AddRule(newRule);
                ClearCurrentRule();
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

        foreach (var connection in _startConnections)
        {
            GameObject con = Instantiate(_connectionPrefab);
            GraphLine line = con.GetComponent<GraphLine>();
            _conVisuals.Add(line);

            int index = _startNodes.FindIndex(item => { return connection.StartNode == item; });
            Transform start = _visualStartNodes[index];
            index = _startNodes.FindIndex(item => { return connection.EndNode == item; });
            Transform end = _visualStartNodes[index];

            line.SetPos(start, end);
        }

        foreach (var connection in _newConnections)
        {
            GameObject con = Instantiate(_connectionPrefab);
            GraphLine line = con.GetComponent<GraphLine>();
            _conVisuals.Add(line);

            int index = _newNodes.FindIndex(item => { return connection.StartNode == item; });
            Transform start = _visualNewNodes[index];
            index = _newNodes.FindIndex(item => { return connection.EndNode == item; });
            Transform end = _visualNewNodes[index];

            line.SetPos(start, end);
        }
    }
    private void RemoveNode(int index, bool isStartNode)
    {
        if (isStartNode)
        {
            Destroy(_visualStartNodes[index].gameObject);
            _visualStartNodes.RemoveAt(index);
            _startNodes.RemoveAt(index);
        }
        else
        {
            Destroy(_visualNewNodes[index].gameObject);
            _visualNewNodes.RemoveAt(index);
            _newNodes.RemoveAt(index);
        }
    }
    private void ConChaNode(int index,bool isStartNode)
    {
        if (_isConnecting)
        {
            //connect nodes
            if (isStartNode)
            {
                if (_startConNodeIndex < 0)
                {
                    _startConNodeIndex = index;
                }
                else
                {
                    _startConnections.Add(new SavedConnection()
                    {
                        EndNode = _startNodes[index],
                        StartNode = _startNodes[_startConNodeIndex],
                    });
                    _startConNodeIndex = -1;
                }
            }
            else if (!isStartNode)
            {
                if (_startConNodeIndex < 0)
                {
                    _startConNodeIndex = index;
                }
                else
                {
                    _newConnections.Add(new SavedConnection()
                    {
                        EndNode = _newNodes[index],
                        StartNode = _newNodes[_startConNodeIndex],
                    });
                    _startConNodeIndex = -1;
                }

            }
        }
        else
        {
            if (isStartNode)
            {
                TextMeshPro textMesh = _visualStartNodes[index].gameObject
                    .GetComponentInChildren<TextMeshPro>();
                textMesh.text = "node " + _startNodes[index].CycleType();
            }
            else
            {
                TextMeshPro textMesh = _visualNewNodes[index].gameObject
                    .GetComponentInChildren<TextMeshPro>();
                textMesh.text = "node " + _newNodes[index].CycleType();
            }
        }
    }
    private void PlaceNode(Vector2 mousePos)
    {
        GameObject node = Instantiate(_nodePrefab);
        node.transform.position = mousePos;
        GraphNode events = node.GetComponent<GraphNode>();
        if (_isStartGraph)
        {
            _visualStartNodes.Add(node.transform);
            _startNodes.Add(new SavedNode(){Id = _BaseId});
            int temp = _BaseId;
            events.OnRightClick.AddListener(delegate { ConChaNode(temp, true);});
            events.OnMiddleClick.AddListener(delegate { RemoveNode(temp, true); });
            _BaseId++;
        }
        else
        {
            _visualNewNodes.Add(node.transform);
            _newNodes.Add(new SavedNode(){Id = _NewId });
            int temp = _NewId;
            events.OnRightClick.AddListener(delegate { ConChaNode(temp, false); });
            events.OnMiddleClick.AddListener(delegate { RemoveNode(temp, false); });
            _NewId++;
        }
    }
    private void ClearCurrentRule()
    {
        _startNodes.Clear();
        _startConnections.Clear();
        _newNodes.Clear();
        _newConnections.Clear();

        foreach (var node in _visualStartNodes)
        {
            Destroy(node.gameObject);
        }
        _visualStartNodes.Clear();

        foreach (var node in _visualNewNodes)
        {
            Destroy(node.gameObject);
        }
        _visualNewNodes.Clear();

        UpdateConnections();
    }
}
