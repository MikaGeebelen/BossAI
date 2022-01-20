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
    MoveCondition,
    Health,
    Combo
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
    public int OtherMain;//a node might point to other info than itself
    public int OtherSecundary;
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
        OtherMain = info.OtherMain;
        OtherSecundary = info.OtherSecundary;
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
    public int OtherMain = -1;
    public int OtherSecundary = -1;

    public SavedNodeInfo(NodeInfo info)
    {
        IsHealth = info.IsHealth;
        CreationIndex = info.CreationIndex;
        MainPiece = info.MainPiece;
        SecundaryPiece = info.SecundaryPiece;
        PhaseNumber = info.PhaseNumber;
        Type = info.Type;
        CurrentValue = info.CurrentValue;
        OtherMain = info.OtherMain;
        OtherSecundary = info.OtherSecundary;
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

    [SerializeField] private List<BaseAbility> _comboStarter = new List<BaseAbility>();
    public List<object> ComboStarter => _comboStarter.Cast<object>().ToList();
    [SerializeField] private List<BaseAbility> _comboFinisher = new List<BaseAbility>();
    public List<object> ComboFinisher => _comboFinisher.Cast<object>().ToList();

    [SerializeField] private int _points = 1000;
    private List<int> _pointsPerMain = new List<int>();
    private bool _behaviorTreeReady = false;

    public UnityEvent OnBehaviorComplete = new UnityEvent();
    private List<NodeInfo> _infoList = new List<NodeInfo>();

    [SerializeField] private TMP_InputField _inputField = null;
    private bool _createdBoss = false;

    private List<KeyValuePair<int, Node>> _structure = null;
    private List<TreeNode> _treeStructure = null;

    private bool _loadBoss = false;

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

        bool isCombo = false;
        int otherComboIndex = 0;

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

        for (int i = 0; i < _converter.MovementConditions.Count; i++ )//cancel movement when piece is dead
        {
            NodeContext con = _converter.MovementConditions[i];
            Type type = _conditionTypes[0].GetType();
            (con.Node as Condition).Setup(CreateCondition(0,con.MainIndex, con.SecundaryIndex, type, true));

            _infoList.Add(new NodeInfo()
            {
                CreationIndex = i, CurrentValue = 0,
                MainPiece = con.MainIndex, SecundaryPiece = con.SecundaryIndex,
                Type = NodeFill.MoveCondition, ObjectType = type,
                OtherMain = -1,OtherSecundary = -1
            });
        } // movement condition

        for (int i = 0; i < _converter.MainPieces; i++)
        {
            for (int j = 0; j < _converter.SecondaryPieces[i]; j++)
            {
                List<NodeContext> nodes = _converter.GetPieceAbilities(i, j);
                for (int k = 0; k < nodes.Count; k++)
                {
                    if (nodes[k].OtherNode != null)
                    {
                        if (isCombo)
                        {
                            (nodes[k].Node as AbilityNode).SetupCombo(CreateAbility(i, j, otherComboIndex,
                                _comboFinisher[otherComboIndex].GetType(),true,true),CreateAbility(i,j,0,_abilityTypes[0].GetType()));


                            _infoList.Add(new NodeInfo()
                            {
                                CreationIndex = k,
                                CurrentValue = otherComboIndex,
                                MainPiece = i,
                                SecundaryPiece = j,
                                Type = NodeFill.Combo,
                                ObjectType = _comboFinisher[otherComboIndex].GetType(),
                                PhaseNumber = nodes[k].PhaseNumber
                            });
                            isCombo = false;
                        }
                        else
                        {

                            List<KeyValuePair<BaseAbility, int>> abilityType =
                                new List<KeyValuePair<BaseAbility, int>>();
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
                                RandomIndex = Random.Range(0, _comboStarter.Count);
                                BaseAbility curretnAbility = _comboStarter[RandomIndex];
                                if (_pointsPerMain[i] > curretnAbility.PointCost || curretnAbility.PointCost < 0)
                                {
                                    if (curretnAbility.ChangesPosition && isDynamicMainPiece)
                                    {
                                        abilityType.Add(
                                            new KeyValuePair<BaseAbility, int>(curretnAbility, RandomIndex));
                                        canPayCost++;
                                    }
                                    else if (!curretnAbility.ChangesPosition)
                                    {
                                        abilityType.Add(
                                            new KeyValuePair<BaseAbility, int>(curretnAbility, RandomIndex));
                                        canPayCost++;
                                    }
                                }
                            } while (canPayCost < 3);

                            abilityType.Sort((type1, type2) =>
                            {
                                return type1.Key.PointCost.CompareTo(type2.Key.PointCost);
                            });
                            abilityType.Reverse();

                            _pointsPerMain[i] -= abilityType[0].Key.PointCost;

                            otherComboIndex = abilityType[0].Value;

                            (nodes[k].Node as AbilityNode).SetupCombo(CreateAbility(i, j, abilityType[0].Value,
                                abilityType[0].Key.GetType(),true,false), null);

                            _infoList.Add(new NodeInfo()
                            {
                                CreationIndex = k,
                                CurrentValue = abilityType[0].Value,
                                MainPiece = i,
                                SecundaryPiece = j,
                                Type = NodeFill.Combo,
                                ObjectType = abilityType[0].Key.GetType(),
                                PhaseNumber = nodes[k].PhaseNumber
                            });

                            isCombo = true;
                        }
                    }
                    else
                    {
                        List<KeyValuePair<BaseAbility, int>> abilityType = new List<KeyValuePair<BaseAbility, int>>();
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
                                    abilityType.Add(new KeyValuePair<BaseAbility, int>(curretnAbility, RandomIndex));
                                    canPayCost++;
                                }
                                else if (!curretnAbility.ChangesPosition)
                                {
                                    abilityType.Add(new KeyValuePair<BaseAbility, int>(curretnAbility, RandomIndex));
                                    canPayCost++;
                                }
                            }
                        } while (canPayCost < 3);

                        abilityType.Sort((type1, type2) =>
                        {
                            return type1.Key.PointCost.CompareTo(type2.Key.PointCost);
                        });
                        abilityType.Reverse();

                        _pointsPerMain[i] -= abilityType[0].Key.PointCost;

                        (nodes[k].Node as AbilityNode).SetAbility(CreateAbility(i, j, abilityType[0].Value,
                            abilityType[0].Key.GetType()));

                        _infoList.Add(new NodeInfo()
                        {
                            CreationIndex = k, CurrentValue = abilityType[0].Value,
                            MainPiece = i, SecundaryPiece = j,
                            Type = NodeFill.Ability, ObjectType = abilityType[0].Key.GetType(),
                            PhaseNumber = nodes[k].PhaseNumber
                        });
                    }
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
                    Type type = null;
                    BaseCondition newCon = null;
                    int randomIndex = 0;
                    do
                    {
                        randomIndex = Random.Range(0, _conditionTypes.Count);
                        type = _conditionTypes[randomIndex].GetType();
                        newCon = CreateCondition(randomIndex,i, j, type);
                    } while (newCon == null); //some conditionstype might not be applicable


                    (conList[k].Node as Condition).Setup(newCon);
         

                    _infoList.Add(new NodeInfo()
                    {
                        CreationIndex = k, CurrentValue = randomIndex,
                        MainPiece = i, SecundaryPiece = j,
                        Type = NodeFill.Condition, ObjectType = type,
                        OtherMain = newCon.GetOther().Key, OtherSecundary = newCon.GetOther().Value
                    });
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

        _loadBoss = true;
        _blackBoard.AddValue("Player", _player);

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

        List<ITreeNode> movementNodes = tree.Value.FindAll(node => { return (node is MovementNode); });
        List<NodeInfo> movementInfo = _infoList.FindAll(node =>
        {
            return node.Type == NodeFill.MainMovement || node.Type == NodeFill.SecundaryMovement;
        });

        for (int i = 0; i < movementNodes.Count; i++)
        {
            MovementNode movement = movementNodes[i] as MovementNode;
            movement.SetMovementBehavior(CreateMovement(movementInfo[i].CurrentValue,movementInfo[i].ObjectType, movementInfo[i].MainPiece, movementInfo[i].SecundaryPiece));
        }

        List<ITreeNode> abilityNodes = tree.Value.FindAll(node => { return (node is AbilityNode); });
        List<NodeInfo> abilityInfo = _infoList.FindAll(node =>
        {
            return node.Type == NodeFill.Ability || node.Type == NodeFill.Combo;
        });

        abilityInfo.Sort((node1, node2) =>
        {
            bool isSec1 = false;
            bool isSec2 = false;

            if (node1.SecundaryPiece > 0 )
            {
                isSec1 = true;
            }

            if (node2.SecundaryPiece > 0)
            {
                isSec2 = true;
            }

            if (isSec1 && !isSec2)
            {
                if (node1.MainPiece == node2.MainPiece)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }else if (!isSec1 && isSec2)
            {
                if (node1.MainPiece == node2.MainPiece)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else if (isSec1 && isSec2)
            {
                if (node1.MainPiece == node2.MainPiece)
                {
                    return node1.SecundaryPiece.CompareTo(node2.SecundaryPiece);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        });


        bool isSecondPartCombo = false;

        for (int i = 0; i < abilityNodes.Count; i++)
        {
            AbilityNode ability = abilityNodes[i] as AbilityNode;

            if (abilityInfo[i].Type == NodeFill.Ability)
            {
                ability.SetAbility(CreateAbility(abilityInfo[i].MainPiece, abilityInfo[i].SecundaryPiece, abilityInfo[i].CurrentValue, abilityInfo[i].ObjectType));
            }
            else
            {
                BaseAbility abilityBackup = null;
                if (isSecondPartCombo)
                {
                    abilityBackup = CreateAbility(abilityInfo[i].MainPiece, abilityInfo[i].SecundaryPiece, 0, _abilityTypes[0].GetType());
                }

                ability.SetupCombo(CreateAbility(abilityInfo[i].MainPiece, abilityInfo[i].SecundaryPiece, abilityInfo[i].CurrentValue, abilityInfo[i].ObjectType,true, isSecondPartCombo), abilityBackup);
                isSecondPartCombo = true;
            }
        }

        List<ITreeNode> conditionNodes = tree.Value.FindAll(node => { return (node is Condition); });
        List<NodeInfo> conditionList = _infoList.FindAll(node =>
        {
            return node.Type == NodeFill.Condition;
        });

        List<NodeInfo> moveCons = _infoList.FindAll(node =>
        {
            return node.Type == NodeFill.MoveCondition;
        });

        conditionList.Sort((node1, node2) =>
        {
            bool isSec1 = false;
            bool isSec2 = false;

            if (node1.SecundaryPiece > 0)
            {
                isSec1 = true;
            }

            if (node2.SecundaryPiece > 0)
            {
                isSec2 = true;
            }

            if (isSec1 && !isSec2)
            {
                if (node1.MainPiece == node2.MainPiece)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else if (!isSec1 && isSec2)
            {
                if (node1.MainPiece == node2.MainPiece)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else if (isSec1 && isSec2)
            {
                if (node1.MainPiece == node2.MainPiece)
                {
                    return node1.SecundaryPiece.CompareTo(node2.SecundaryPiece);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        });

        int index = 0;
        for (int i = 0; i < moveCons.Count; i++)
        {
            Condition con = conditionNodes[i] as Condition;
            con.Setup(CreateCondition(moveCons[i].CurrentValue, moveCons[i].MainPiece, moveCons[i].SecundaryPiece, moveCons[i].ObjectType, true));
            index = i;
        }

        if (index != 0)
        {
            index++;

        }

        for (int i = index; i < conditionList.Count + index; i++)
        {
            Condition con = conditionNodes[i] as Condition;
            con.Setup(CreateCondition(conditionList[i - index].CurrentValue, conditionList[i - index].MainPiece, conditionList[i - index].SecundaryPiece, conditionList[i - index].ObjectType));
        }

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

    private BaseAbility CreateAbility(int index, int secundaryIndex, int typeIndex, Type type, bool isCombo = false,bool isComboEnd = false)
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
                    else
                    {
                        initList.Add(boardLocMain + index + "0");
                    }

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

        if (isCombo)
        {
            if (!isComboEnd)
            {
                ability.SetupScripted(_comboStarter[typeIndex]);
            }
            else
            {
                ability.SetupScripted(_comboFinisher[typeIndex]);
            }
        }
        else
        {
            ability.SetupScripted(_abilityTypes[typeIndex]);
        }

        return ability;
    }

    private BaseCondition CreateCondition(int typeIndex,int main, int secundary, Type type, bool isMovement = false)
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
                    else
                    {
                        initList.Add(boardLocMain + main + "0");
                    }

                    break;
                case Requirements.MovementOrigin:
                    initList.Add(boardLocMain + main + "0");
                    break;
                case Requirements.PieceIndex:
                    initList.Add(main.ToString());
                    initList.Add(secundary.ToString());
                    break;
                case Requirements.OtherRandom:
                    string random = GetRandomPiece(main,secundary);
                    if (random == "")
                    {
                        return null;
                    }
                    else
                    {
                        initList.Add(random);
                    }
                    break;
            }
        }

        if (isMovement)
        {
            initList.Add("IsMovement");
        }

        condition.Setup(_blackBoard, initList);
        condition.SetupScripted(_conditionTypes[typeIndex]);
        return condition;
    }

    private string GetRandomPiece(int main,int secundary)
    {
        string boardLocMain = "main";
        string boardLocSecundary = "secundary";
        if (_structure.Count < 1)
        {
            return "";
        }
        else
        {
            List<KeyValuePair<int, int>> allPieces = new List<KeyValuePair<int, int>>();
            int currentMain = -1;
            int currentSecundary = 1;
            foreach (var piece in _structure)
            {
                if (piece.Key == currentMain)
                {
                    allPieces.Add(new KeyValuePair<int, int>(currentMain, currentSecundary));
                    currentSecundary++;
                }
                else
                {
                    currentMain++;
                    currentSecundary = 1;
                    allPieces.Add(new KeyValuePair<int, int>(currentMain, 0));
                }
            }


            List<NodeInfo> conditions = _infoList.FindAll(node =>
            {
                return node.Type == NodeFill.Condition;
            });//all currently created conditions except itself

            if (!_loadBoss)
            {
                foreach (NodeInfo info in conditions)
                {
                    if (info.OtherMain >= 0 && info.OtherSecundary >= 0)
                    {
                        allPieces.Remove(new KeyValuePair<int, int>(info.MainPiece, info.SecundaryPiece));
                    }

                    if (info.MainPiece == main && info.SecundaryPiece == secundary)
                    {
                        if (info.OtherMain >= 0 && info.OtherSecundary >= 0)
                        {
                            return "";
                        }
                    }
                }
            }
            else
            {
                foreach (NodeInfo info in conditions)
                {
                    if (info.MainPiece == main && info.SecundaryPiece == secundary)
                    {
                        if (info.OtherMain >= 0 && info.OtherSecundary >= 0)
                        {
                            if (info.OtherSecundary > 0)
                            {
                                return boardLocSecundary + info.OtherMain + (info.OtherSecundary - 1).ToString();
                            }
                            else
                            {
                                return boardLocMain + info.OtherMain + "0";
                            }
                        }
                    }
                }
            }

            allPieces.Remove(new KeyValuePair<int, int>(main, secundary));

            if (allPieces.Count >= 1)
            {
                KeyValuePair<int, int> random = allPieces[Random.Range(0, allPieces.Count)];
                if (random.Value > 0)
                {
                    return  boardLocSecundary + random.Key + (random.Value - 1).ToString();
                }
                else
                {
                    return boardLocMain + random.Key + "0";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
