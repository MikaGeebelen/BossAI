using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public enum NodeFill
{
    MainMovement,
    SecundaryMovement,
    Ability,
    Condition,
    Health
}

public struct NodeInfo
{
    public bool IsHealth;
    public Health HealthComp;
    public int CreationIndex;
    public int MainPiece;
    public int SecundaryPiece;
    public int PhaseNumber;
    public NodeFill Type;
    public int CurrentValue;
    public Type ObjectType;

    public bool IsOwnedByPiece(int main, int secundary)
    {
        if (main == MainPiece && secundary == SecundaryPiece)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public NodeInfo(SavedNodeInfo info)
    {
        IsHealth = info.IsHealth;
        HealthComp = null;
        CreationIndex = info.CreationIndex;
        MainPiece = info.MainPiece;
        SecundaryPiece = info.SecundaryPiece;
        PhaseNumber = info.PhaseNumber;
        Type = info.Type;
        CurrentValue = info.CurrentValue;
        ObjectType = System.Type.GetType(info.ObjectType);
    }
}
[Serializable]
public class SavedNodeInfo
{
    public bool IsHealth = false;
    public int CreationIndex = 0;
    public int MainPiece = 0;
    public int SecundaryPiece = 0;
    public int PhaseNumber = 0;
    public NodeFill Type = 0;
    public int CurrentValue = 0;
    public string ObjectType = "";

    public SavedNodeInfo(NodeInfo info)
    {
        IsHealth = info.IsHealth;
        CreationIndex = info.CreationIndex;
        MainPiece = info.MainPiece;
        SecundaryPiece = info.SecundaryPiece;
        PhaseNumber = info.PhaseNumber;
        Type = info.Type;
        CurrentValue = info.CurrentValue;
        if (IsHealth)
        {
            ObjectType = "health";
        }
        else
        {
            ObjectType = info.ObjectType.AssemblyQualifiedName;
        }
    }
}
[Serializable]
public class StructureNode
{
    public int Num = 0;
    public SavedNode Node = null;
}

[Serializable]
public class listWrapper<T>
{
    public List<T> list = new List<T>();
}


public class BossBuilder : MonoBehaviour
{
    [SerializeField] private GraphGrammar _generator = null;
    [SerializeField] private GameObject _player = null;
    private GraphToBehavior _converter = null;

    private BehaviorTree _tree = null;
    private BlackBoard _blackBoard = new BlackBoard();

    [SerializeField] private GameObject _bossCore = null;
    [SerializeField] private GameObject _mainPiece = null;
    [SerializeField] private GameObject _secundaryPiece = null;

    [SerializeField] private List<BaseMovement> _movementTypes = new List<BaseMovement>();
    public List<object> MovementTypes => _movementTypes.Cast<object>().ToList();
    [SerializeField] private List<BaseMovement> _secundaryMovementTypes = new List<BaseMovement>(); 
    public List<object> SecundaryMovementTypes => _secundaryMovementTypes.Cast<object>().ToList();
    [SerializeField] private List<BaseAbility> _abilityTypes = new List<BaseAbility>();
    public List<object> AbilityTypes => _abilityTypes.Cast<object>().ToList();

    [SerializeField] private List<BaseCondition> _conditionTypes = new List<BaseCondition>();
    public List<object> ConditionTypes => _conditionTypes.Cast<object>().ToList();


    [SerializeField] private int _points = 1000;
    private List<int> _pointsPerMain = new List<int>();
    private bool _behaviorTreeReady = false;

    public UnityEvent OnBehaviorComplete = new UnityEvent();
    private List<NodeInfo> _infoList = new List<NodeInfo>();

    [SerializeField] private TMP_InputField _inputField = null;
    private bool _createdBoss = false;

    private List<KeyValuePair<int, Node>> _structure = null;
    private List<TreeNode> _treeStructure = null;

    public List<NodeInfo> InfoList
    {
        get
        {
            return _infoList;
        }
    }

    private void Start()
    {
        _generator.GenerationFinished.AddListener(CreateBoss);
        SaveSystem.Init();
    }

    private void CreateBoss()
    {
        if (_generator == null)
        {
            return;
        }

        _createdBoss = true;
        _blackBoard.AddValue("Player", _player);

        _converter = new GraphToBehavior(_generator.Graph);

        //create structure
        _structure = _converter.FindBodies();
        CreateBossBody();

        //create behavior tree
        KeyValuePair<BehaviorTree,List<TreeNode>> treeResult = _converter.CreateTree();
        _tree = treeResult.Key;
        _tree.SetBlackBoard(_blackBoard);

        _treeStructure = treeResult.Value;

        //simple random
        for (int i = 0; i < _converter.MovementNodes.Count; i++)
        {
            BaseMovement moveType = null;

            bool canPayCost = false;

            int randomIndex = 0;

            bool isSecundary = false;

            if (_converter.MovementNodes[i].SecundaryIndex > 0)
            {
                int mainindex = _converter.MovementNodes[i].MainIndex;
                do
                {
                    randomIndex = Random.Range(0, _secundaryMovementTypes.Count);
                    moveType = _secundaryMovementTypes[randomIndex];
                    if (_pointsPerMain[mainindex] > moveType.PointCost || moveType.PointCost < 0)
                    {
                        _pointsPerMain[mainindex] -= moveType.PointCost;
                        canPayCost = true;
                    }
                } while (!canPayCost);

                isSecundary = true;
            }
            else
            {
                int mainindex = _converter.MovementNodes[i].MainIndex;
                do
                {
                    randomIndex = Random.Range(0, _movementTypes.Count);
                    moveType = _movementTypes[randomIndex];
                    if (_pointsPerMain[mainindex] > moveType.PointCost || moveType.PointCost < 0)
                    {
                        _pointsPerMain[mainindex] -= moveType.PointCost;
                        canPayCost = true;
                    }
                } while (!canPayCost);
            }

            (_converter.MovementNodes[i].Node as MovementNode).SetMovementBehavior(CreateMovement(randomIndex, moveType.GetType(), _converter.MovementNodes[i].MainIndex, _converter.MovementNodes[i].SecundaryIndex));

            if (!isSecundary)
            {
                _infoList.Add(new NodeInfo() 
                    { CreationIndex = i, CurrentValue = randomIndex,
                        MainPiece = _converter.MovementNodes[i].MainIndex, SecundaryPiece = _converter.MovementNodes[i].SecundaryIndex,
                        Type = NodeFill.MainMovement, ObjectType = moveType.GetType() });
            }
            else
            {
                _infoList.Add(new NodeInfo() 
                    { CreationIndex = i, CurrentValue = randomIndex,
                        MainPiece = _converter.MovementNodes[i].MainIndex, SecundaryPiece = _converter.MovementNodes[i].SecundaryIndex,
                        Type = NodeFill.SecundaryMovement, ObjectType = moveType.GetType() });
            }
        } //movement

        foreach (NodeContext condition in _converter.MovementConditions) //cancel movement when piece is dead
        {
            string boardLoc = "mainPieceHealth";
            int index = 0;
            if (condition.SecundaryIndex > 0)
            {
                boardLoc = "secundaryHealth";
                index = condition.SecundaryIndex - 1;
            }

            (condition.Node as Condition).Setup(CreateCondition(condition.MainIndex, index, _conditionTypes[0].GetType(),true));
        } // movement condition

        for (int i = 0; i < _converter.MainPieces; i++)
        {
            for (int j = 0; j < _converter.SecondaryPieces[i]; j++)
            {
                List<NodeContext> nodes = _converter.GetPieceAbilities(i, j);
                for (int k = 0; k < nodes.Count; k++)
                {

                    List<KeyValuePair<BaseAbility,int>> abilityType = new List<KeyValuePair<BaseAbility, int>>();
                    int canPayCost = 0;

                    int RandomIndex = 0;

                    bool isDynamicMainPiece = false;

                    if (!(j >= 1))
                    {
                        if (_converter.GetPieceMovement(i, j).Count > 0)
                        {
                            isDynamicMainPiece = true;
                        }
                    }

                    //try and use all remaining points on abilities, and bosses that can't move can't use specific abilities
                    do
                    {
                        RandomIndex = Random.Range(0, _abilityTypes.Count);
                        BaseAbility curretnAbility = _abilityTypes[RandomIndex];
                        if (_pointsPerMain[i] > curretnAbility.PointCost || curretnAbility.PointCost < 0)
                        {
                            if (curretnAbility.ChangesPosition && isDynamicMainPiece)
                            {
                                abilityType.Add(new KeyValuePair<BaseAbility, int>(curretnAbility,RandomIndex));
                                canPayCost++;
                            }
                            else if (!curretnAbility.ChangesPosition)
                            {
                                abilityType.Add(new KeyValuePair<BaseAbility, int>(curretnAbility, RandomIndex));
                                canPayCost++;
                            }
                        }
                    } while (canPayCost < 3);

                    abilityType.Sort((type1, type2) => { return type1.Key.PointCost.CompareTo(type2.Key.PointCost); });
                    abilityType.Reverse();

                    _pointsPerMain[i] -= abilityType[0].Key.PointCost;

                    (nodes[k].Node as AbilityNode).SetAbility(CreateAbility(i,j,abilityType[0].Value,abilityType[0].Key.GetType()));

                    _infoList.Add(new NodeInfo()
                    {
                        CreationIndex = k, CurrentValue = abilityType[0].Value,
                        MainPiece = i, SecundaryPiece = j,
                        Type = NodeFill.Ability, ObjectType = abilityType[0].Key.GetType(),PhaseNumber = nodes[k].PhaseNumber});
                }
            }
        } //abilities

        for (int i = 0; i < _converter.MainPieces; i++)
        {
            for (int j = 0; j < _converter.SecondaryPieces[i]; j++)
            {
                var conList = _converter.GetPieceConditions(i, j);
                for (int k = 0; k < conList.Count; k++)
                {
                    string boardLoc = "mainPieceHealth";
                    int index = 0;
                    if (j > 0)
                    {
                        boardLoc = "secundaryHealth";
                        index = j - 1;
                    }
                    (conList[k].Node as Condition).Setup(CreateCondition(i,index, _conditionTypes[Random.Range(0,_conditionTypes.Count)].GetType()));
                }

            }
        } //conditions

        _behaviorTreeReady = true;
        OnBehaviorComplete.Invoke();
    }

    public void SaveBoss()
    {
  
        listWrapper<StructureNode> savableStructure = new listWrapper<StructureNode>();
        foreach (var piece in _structure)
        {
            savableStructure.list.Add(new StructureNode() {Node = new SavedNode() {Id = piece.Value.Id, Type = (int) piece.Value.Type}, Num = piece.Key} );
        }

        string structureJson = JsonUtility.ToJson(savableStructure);
        SaveSystem.SaveBossStructure(_inputField.text, structureJson);

        listWrapper<SavedNodeInfo> savableInfo = new listWrapper<SavedNodeInfo>();
        foreach (NodeInfo info in _infoList)
        {
            savableInfo.list.Add(new SavedNodeInfo(info));
        }

        string infoJson = JsonUtility.ToJson(savableInfo);
        SaveSystem.SaveBossInfo(_inputField.text, infoJson);

        TreeNode top = new TreeNode() {NodeType = TreeNodeType.RootSequence, SubNodes = _treeStructure};

        string treeStructureJson = top.ToJson();
        SaveSystem.SaveBossTree(_inputField.text, treeStructureJson);
    }

    public void LoadBoss()
    {
        if (_createdBoss)
        {
            return;
        }

        //SaveSystem.LoadBossStructure();
        listWrapper<StructureNode> loadedStructure = JsonUtility.FromJson<listWrapper<StructureNode>>(SaveSystem.LoadBossStructure(_inputField.text));
        listWrapper<SavedNodeInfo> loadedInfo = JsonUtility.FromJson<listWrapper<SavedNodeInfo>>(SaveSystem.LoadBossInfo(_inputField.text));
        var tree = TreeNode.FromJson(SaveSystem.LoadBossTree(_inputField.text));

        _tree = tree.Key;

        _structure = new List<KeyValuePair<int, Node>>();
        _infoList = new List<NodeInfo>();

        foreach (var info in loadedInfo.list)
        {
            _infoList.Add(new NodeInfo(info));
        }

        foreach (var piece in loadedStructure.list)
        {
            _structure.Add(new KeyValuePair<int, Node>(piece.Num,new Node(piece.Node.Id, (NodeType)piece.Node.Type)));
        }

        LoadBossBody(_infoList.FindAll(info => { return info.IsHealth;}));

        List<ITreeNode> movementNodes = _tree.Nodes.FindAll(node => { return (node is MovementNode); });
        List<NodeInfo> movementInfo = _infoList.FindAll(node =>
        {
            return node.Type == NodeFill.MainMovement || node.Type == NodeFill.SecundaryMovement;
        });

        for (int i = 0; i < movementNodes.Count; i++)
        {
            MovementNode movement = movementNodes[i] as MovementNode;
            movement.SetMovementBehavior(CreateMovement(movementInfo[i].CurrentValue,movementInfo[i].ObjectType, movementInfo[i].MainPiece, movementInfo[i].SecundaryPiece));
        }

        List<ITreeNode> abilityNodes = _tree.Nodes.FindAll(node => { return (node is AbilityNode); });
        List<NodeInfo> abilityInfo = _infoList.FindAll(node =>
        {
            return node.Type == NodeFill.Ability;
        });

        for (int i = 0; i < abilityNodes.Count; i++)
        {
            AbilityNode ability = abilityNodes[i] as AbilityNode;
            ability.SetAbility(CreateAbility(abilityInfo[i].MainPiece, abilityInfo[i].SecundaryPiece, abilityInfo[i].CurrentValue, abilityInfo[i].ObjectType));
        }

        List<ITreeNode> conditionNodes = _tree.Nodes.FindAll(node => { return (node is Condition); });

        _behaviorTreeReady = true;
        OnBehaviorComplete.Invoke();
    }

    public void ToggleBoss()
    {
        if (_behaviorTreeReady)
        {
            if (_tree.IsTreeActive)
            {
                _tree.PauseTree();
            }
            else
            {
                _tree.startTree();
            }

        }
        else
        {
            Debug.Log("Nothing loaded or generated");
        }
    }

    private void CreateBossBody()
    {
        GameObject core = Instantiate(_bossCore);
        _blackBoard.AddValue("Boss",core.GetComponent<Boss>());
        int currentMain = 0;

        List<KeyValuePair<int,Node>> splitMain = null;
        List<int> pieceCount = new List<int>();
        int totalPieces = _structure.Count;
        for (int i = 0; i < (_structure[_structure.Count-1].Key+1); i++)
        {
            List<KeyValuePair<int,Node>> pieces = _structure.FindAll(node =>
            {
                return node.Key == i;
            });
            pieceCount.Add(pieces.Count);
        }

        foreach (int count in pieceCount)
        {
            _pointsPerMain.Add(count * (_points/totalPieces));
        }

        foreach (var bossPiece in _structure)
        {
            if (bossPiece.Key == currentMain && (bossPiece.Value.Type == NodeType.BossCore || bossPiece.Value.Type == NodeType.BossCoreDynamic))
            {
                GameObject mainPiece = Instantiate(_mainPiece, core.transform);
                _blackBoard.AddValue("main" + currentMain + "0", mainPiece); //save all core pieces to use their bodies for movement
                Health health = mainPiece.GetComponent<Health>();

                int totalHP = Random.Range(_pointsPerMain[currentMain]/10, _pointsPerMain[currentMain] / 2);
                _pointsPerMain[currentMain] -= totalHP;
                health.Setup(totalHP);

                _infoList.Add(new NodeInfo()
                {
                    IsHealth = true, HealthComp = health, Type = NodeFill.Health, MainPiece = currentMain,SecundaryPiece = 0, CurrentValue = totalHP
                });


                _blackBoard.AddValue("mainPieceHealth" + currentMain + "0", health);
                Transform center = mainPiece.transform.Find("Center");
                splitMain = _structure.FindAll(node =>
                {
                    return node.Key == currentMain && (node.Value.Type != NodeType.BossCore && node.Value.Type != NodeType.BossCoreDynamic);
                });

                float anglePerPiece = 0;
                if (splitMain.Count > 0)
                {
                    anglePerPiece = 360.0f / splitMain.Count;
                }

                int secundaryHP = totalHP/2;


                for (int i = 0; i < splitMain.Count; i++)
                {
                    GameObject secundaryPiece = Instantiate(_secundaryPiece, center.GetChild(0).position, Quaternion.identity, mainPiece.transform);
                    _blackBoard.AddValue("secundary" + currentMain + i, secundaryPiece);
                    Health healthSec = secundaryPiece.GetComponent<Health>();
                    healthSec.Setup(secundaryHP / splitMain.Count);

                    _infoList.Add(new NodeInfo()
                    {
                        IsHealth = true, HealthComp = healthSec,Type = NodeFill.Health, MainPiece = currentMain, SecundaryPiece = i+1, CurrentValue = secundaryHP / splitMain.Count
                    });

                    _blackBoard.AddValue("secundaryHealth" + currentMain + i, healthSec.GetComponent<Health>());
                    center.Rotate(new Vector3(0, 0, anglePerPiece));
                }
                currentMain++;
            }
        }
    }

    private void LoadBossBody(List<NodeInfo> healthInfo)
    {
        GameObject core = Instantiate(_bossCore);
        _blackBoard.AddValue("Boss", core.GetComponent<Boss>());
        int currentMain = 0;

        List<KeyValuePair<int, Node>> splitMain = null;
        List<int> pieceCount = new List<int>();
        int totalPieces = _structure.Count;
        for (int i = 0; i < (_structure[_structure.Count - 1].Key + 1); i++)
        {
            List<KeyValuePair<int, Node>> pieces = _structure.FindAll(node =>
            {
                return node.Key == i;
            });
            pieceCount.Add(pieces.Count);
        }

        foreach (int count in pieceCount)
        {
            _pointsPerMain.Add(count * (_points / totalPieces));
        }

        foreach (var bossPiece in _structure)
        {
            int currentNodeInfo = 0;
            if (bossPiece.Key == currentMain && (bossPiece.Value.Type == NodeType.BossCore || bossPiece.Value.Type == NodeType.BossCoreDynamic))
            {
                GameObject mainPiece = Instantiate(_mainPiece, core.transform);
                _blackBoard.AddValue("main" + currentMain + "0", mainPiece); //save all core pieces to use their bodies for movement
                Health health = mainPiece.GetComponent<Health>();

                NodeInfo thisPiece = healthInfo[currentNodeInfo];
                thisPiece.HealthComp = health;
                health.Setup(thisPiece.CurrentValue);
                currentNodeInfo++;

                _blackBoard.AddValue("mainPieceHealth" + currentMain + "0", health);
                Transform center = mainPiece.transform.Find("Center");
                splitMain = _structure.FindAll(node =>
                {
                    return node.Key == currentMain && (node.Value.Type != NodeType.BossCore && node.Value.Type != NodeType.BossCoreDynamic);
                });

                float anglePerPiece = 0;
                if (splitMain.Count > 0)
                {
                    anglePerPiece = 360.0f / splitMain.Count;
                }

                for (int i = 0; i < splitMain.Count; i++)
                {
                    GameObject secundaryPiece = Instantiate(_secundaryPiece, center.GetChild(0).position, Quaternion.identity, mainPiece.transform);
                    _blackBoard.AddValue("secundary" + currentMain + i, secundaryPiece);
                    
                    Health healthSec = secundaryPiece.GetComponent<Health>();
                    NodeInfo secundary = healthInfo[currentNodeInfo];
                    secundary.HealthComp = healthSec;
                    healthSec.Setup(secundary.CurrentValue);
                    currentNodeInfo++;

                    _blackBoard.AddValue("secundaryHealth" + currentMain + i, healthSec.GetComponent<Health>());
                    center.Rotate(new Vector3(0, 0, anglePerPiece));
                }
                currentMain++;
            }
        }
    }

    public void ChangeNodeContents(NodeInfo newValue)
    {
        switch (newValue.Type)
        {
            case NodeFill.MainMovement:
            case NodeFill.SecundaryMovement:
                List<NodeContext> context1 = _converter.GetPieceMovement(newValue.MainPiece, newValue.SecundaryPiece); //always 1
                (context1[0].Node as MovementNode).SetMovementBehavior(null); // clear
                (context1[0].Node as MovementNode).SetMovementBehavior(CreateMovement(newValue.CurrentValue,newValue.ObjectType, newValue.MainPiece, newValue.SecundaryPiece)); //set new movement system
                break;
            case NodeFill.Ability:
                List<NodeContext> context2 = _converter.GetPieceAbilities(newValue.MainPiece, newValue.SecundaryPiece); //always 1
                (context2[newValue.CreationIndex].Node as AbilityNode).SetAbility(null); // clear
                (context2[newValue.CreationIndex].Node as AbilityNode).SetAbility(CreateAbility(newValue.MainPiece, newValue.SecundaryPiece,newValue.CurrentValue, newValue.ObjectType)); //set new movement system
                break;
            case NodeFill.Condition:
                break;
        }
    }

    private BaseMovement CreateMovement(int typeIndex,Type type,int mainIndex, int secundaryIndex)
    {
        BaseMovement movement = Activator.CreateInstance(type) as BaseMovement;
        List<string> initList = new List<string>();
        foreach (var requirement in movement.GetRequirements())
        {
            switch (requirement)
            {
                case Requirements.Spawner:
                    initList.Add("Boss");
                    break;
                case Requirements.Target:
                    initList.Add("Player");
                    break;
                case Requirements.Origin:

                    if (secundaryIndex > 0)
                    {
                        initList.Add("secundary" + mainIndex + (secundaryIndex - 1));
                    }
                    else
                    { 
                        initList.Add("main" + mainIndex + "0");
                    }
                    

                    break;
                case Requirements.MovementOrigin:
                    initList.Add("main" + mainIndex + "0");
                    break;
                case Requirements.PieceIndex:
                    initList.Add(mainIndex.ToString());
                    initList.Add(secundaryIndex.ToString());
                    break;
            }
        }

        movement.Setup(_blackBoard, initList);
        if (secundaryIndex > 0)
        {
            movement.SetupScripted(_secundaryMovementTypes[typeIndex]);
        }
        else
        {
            movement.SetupScripted(_movementTypes[typeIndex]);
        }

        return movement;
    }

    private BaseAbility CreateAbility(int index, int secundaryIndex, int typeIndex, Type type)
    {
        string boardLocMain = "main";
        string boardLocSecundary = "secundary";
        //Type type = abilityType[0].GetType();
        BaseAbility ability = Activator.CreateInstance(type) as BaseAbility;
        List<string> initList = new List<string>();
        foreach (var requirement in ability.GetRequirements())
        {
            switch (requirement)
            {
                case Requirements.Spawner:
                    initList.Add("Boss");
                    break;
                case Requirements.Target:
                    initList.Add("Player");
                    break;
                case Requirements.Origin:
                    if (secundaryIndex > 0)
                    {
                        initList.Add(boardLocSecundary + index + (secundaryIndex - 1).ToString());
                    }

                    initList.Add(boardLocMain + index + "0");
                    break;
                case Requirements.MovementOrigin:
                    initList.Add(boardLocMain + index + "0");
                    break;
                case Requirements.PieceIndex:
                    initList.Add(index.ToString());
                    initList.Add(secundaryIndex.ToString());
                    break;
            }
        }

        ability.Setup(_blackBoard, initList);
        ability.SetupScripted(_abilityTypes[typeIndex]);

        return ability;
    }

    private BaseCondition CreateCondition(int main, int secundary, Type type, bool isMovement = false)
    {
        string boardLocMain = "main";
        string boardLocSecundary = "secundary";
        //Type type = abilityType[0].GetType();
        BaseCondition condition = Activator.CreateInstance(type) as BaseCondition;
        List<string> initList = new List<string>();
        foreach (var requirement in condition.GetRequirements())
        {
            switch (requirement)
            {
                case Requirements.Spawner:
                    initList.Add("Boss");
                    break;
                case Requirements.Target:
                    initList.Add("Player");
                    break;
                case Requirements.Origin:
                    if (secundary > 0) 
                    {
                        initList.Add(boardLocSecundary + main + (secundary - 1).ToString());
                    }
                    initList.Add(boardLocMain + main + "0");
                    
                    break;
                case Requirements.MovementOrigin:
                    initList.Add(boardLocMain + main + "0");
                    break;
                case Requirements.PieceIndex:
                    initList.Add(main.ToString());
                    initList.Add(secundary.ToString());
                    break;
            }
        }

        if (isMovement)
        {
            initList.Add("IsMovement");
        }

        condition.Setup(_blackBoard, initList);
        //ability.SetupScripted(_abilityTypes[typeIndex]);

        return condition;
    }
}
