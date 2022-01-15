using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GraphGrammar : MonoBehaviour
{
    //to be able to save a graph
    [SerializeField] private TMP_InputField _graphName = null;
    //visulas
    public UnityEvent GenerationFinished = new UnityEvent();
    [SerializeField] private GraphVisualiser _visual = null;
    //data
    private Graph _graph = new Graph();

    public Graph Graph
    {
        get { return _graph; }
    }
    private List<Rule> _rules = new List<Rule>();
    [SerializeField] private int _points = 300;

    //auto graph 
    private bool _isGenerating = false;
    [SerializeField] private float _timeBetweenRules = 1.0f;
    private float _currentTime = 0.0f;
    private int _ruleAtempts = 0;

    [SerializeField] private bool _isPlayMode = false;
    private bool _isLoadedGraph = false;
    private bool _isTypingInField = false;
    private void Start()
    {
        if (_visual != null)
        {
            _visual.registerGraph(_graph, _graph.RuleAppliedEvent);
        }

        if (_graphName != null)
        {
            _graphName.onSelect.AddListener((text) => { _isTypingInField = true; });
            _graphName.onDeselect.AddListener((text) => { _isTypingInField = false; });
        }

        _rules = JsonUtility.FromJson<RuleList>(SaveSystem.LoadRule()).Rules;
        _graph.AddNode(NodeType.Start); //add a start node
    }
    private void Update()
    {
        if (!_isPlayMode)
        {
            if (Input.GetButtonDown("AdvanceSim"))
            {
                ApplyRule();
                Debug.Log("Advanced sim");
            }
            else if (Input.GetButtonDown("Reset"))
            {
                _rules.Clear();
                SaveSystem.SaveRule("{}");
                Debug.Log("cleared out old rules");
            }
        }
        else if(Input.GetButtonDown("AutoGraph"))
        {
            if (!_isLoadedGraph && !_isTypingInField)
            {
                _isGenerating = !_isGenerating;
            }
        }

        if (_isGenerating)
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > _timeBetweenRules)
            {
                if (ApplyRule())
                {
                   _currentTime = 0;
                   Debug.Log("rule succesfull after" + _ruleAtempts.ToString() + "atempts");
                   if (_points == 0)
                   {
                       GenerationFinished.Invoke();
                       _isGenerating = false;
                   }
                }
                _ruleAtempts++;
            }
        }
    }
    private bool ApplyRule()
    {
        int ruleIndex = Random.Range(0, _rules.Count);
        _points -= _rules[ruleIndex].PointCost;
        if (_points < 0)
        {
            _points += _rules[ruleIndex].PointCost;
            return false;
        }

        if (_graph.ApplyRule(_rules[ruleIndex]))
        {
            return true;
        }
        else
        {
            _points += _rules[ruleIndex].PointCost;
            return false;
        }
    }
    public void AddRule(Rule newRule)
    {
        _rules.Add(newRule);

        RuleList ruleList = new RuleList() {Rules = _rules};

        SaveSystem.Init();
        SaveSystem.SaveRule(JsonUtility.ToJson(ruleList));
    }
    public void SaveGraph()
    {
        SavedGraph graph = new SavedGraph(_graph);
        string boss = JsonUtility.ToJson(graph);

        Debug.Log("Saved: " + boss + "to a json file named: " + _graphName.text);

        SaveSystem.SaveBossGraph(_graphName.text, boss);
    }
    public void LoadGraph()
    {
      SavedGraph graph = JsonUtility.FromJson<SavedGraph>(SaveSystem.LoadBossGraph(_graphName.text));
      _graph = graph.ConvertToGraph();

      if (_visual != null)
      {
          _visual.registerGraph(_graph, _graph.RuleAppliedEvent);
      }

      Debug.Log("loaded: " + _graphName.text + ".json" );

      _graph.UpdateVisual();
      GenerationFinished.Invoke();
    }
    [Serializable]
    public class RuleList
    {
        public List<Rule> Rules = new List<Rule>();
    }
}
