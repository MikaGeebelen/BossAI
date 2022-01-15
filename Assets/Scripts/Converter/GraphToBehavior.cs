using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public enum TreeNodeType
{
    Sequence,
    Selector,
    RootSequence,
    Condition,
    Ability,
    Movement
}

[SerializeField]
public class TreeNode
{
    public TreeNodeType NodeType = 0;
    public List<TreeNode> SubNodes = new List<TreeNode>();

    public string ToJson()
    {
        string json = "";
        json += "{";
        json += "\"NodeType\"" + ":" + ((int)NodeType).ToString() + ",";
        json += "\"SubNodes\"" + ":" + "[";
        for (int i = 0; i < SubNodes.Count; i++)
        {
            json += SubNodes[i].ToJson();
            if (i != SubNodes.Count-1)
            {
                json += ",";
            }
        }

        json += "]";

        json += "}";
        return json;
    }

    public static KeyValuePair<BehaviorTree, List<ITreeNode>> FromJson(string json)
    {
        List<ITreeNode> nodes = new List<ITreeNode>();
        List<ITreeNode> dataRequired = new List<ITreeNode>();
        BehaviorTree tree = new BehaviorTree(nodes);

        List<List<ITreeNode>> creationList = new List<List<ITreeNode>>();

        creationList.Add(nodes);

        int currentList = 0;

        int currentIndex = 0;
        string current = json;
        do
        {
            currentIndex = current.IndexOf("NodeType");
            currentIndex += 10;
            int nodeType = -1;
            if (int.TryParse(current.Substring(currentIndex, 1), out int result))
            {
                nodeType = result;
            }
            else
            {
                break;
            }
            currentIndex++;

            switch ((TreeNodeType) nodeType)
            {
                case TreeNodeType.Sequence:
                    List<ITreeNode> newNodes1 = new List<ITreeNode>();
                    Sequence sequence = new Sequence(newNodes1);
                    creationList[currentList].Add(sequence);
                    creationList.Insert(currentList + 1, newNodes1);
                    currentList++;
                    break;
                case TreeNodeType.Selector:
                    List<ITreeNode> newNodes2 = new List<ITreeNode>();
                    Selector selector = new Selector(newNodes2);
                    creationList[currentList].Add(selector);
                    creationList.Insert(currentList + 1, newNodes2);
                    currentList++;
                    break;
                case TreeNodeType.RootSequence:
                    List<ITreeNode> newNodes3 = new List<ITreeNode>();
                    RootSequence rootSequence = new RootSequence(newNodes3);
                    creationList[currentList].Add(rootSequence);
                    creationList.Insert(currentList +1, newNodes3 );
                    currentList++;
                    break;
                case TreeNodeType.Condition:
                    Condition condition = new Condition();
                    creationList[currentList].Add(condition);
                    dataRequired.Add(condition);
                    currentList++;
                    break;
                case TreeNodeType.Ability:
                    AbilityNode ability = new AbilityNode();
                    creationList[currentList].Add(ability);
                    dataRequired.Add(ability);
                    currentList++;
                    break;
                case TreeNodeType.Movement:
                    MovementNode movement = new MovementNode();
                    creationList[currentList].Add(movement);
                    dataRequired.Add(movement);
                    currentList++;
                    break;
            }

            current = current.Substring(currentIndex);
            currentIndex = 0;
            while (current.IndexOf("]") < current.IndexOf("NodeType") && current.IndexOf("NodeType") != -1)
            {
                currentList--;
                current = current.Substring(current.IndexOf("]") + 1);
            }



        } while (currentIndex < current.Length);

        return new KeyValuePair<BehaviorTree, List<ITreeNode>>(tree,dataRequired);
    }
 }

public struct NodeContext
{
    public ITreeNode Node;
    public int MainIndex; 
    public int SecundaryIndex;
    public int PhaseNumber;
}

public class GraphToBehavior
{
    private Graph _graph = null;

    private List<NodeContext> _moveNodes = new List<NodeContext>();

    public List<NodeContext> GetPieceMovement(int mainPiece, int secundaryPiece)
    {
        return _moveNodes.FindAll((node) =>
        {
            return node.MainIndex == mainPiece && node.SecundaryIndex == secundaryPiece;
        });
    }
    public List<NodeContext> MovementNodes
    {
        get { return _moveNodes; }
    }

    private List<NodeContext> _moveConditions = new List<NodeContext>();
    public List<NodeContext> MovementConditions
    {
        get { return _moveConditions; }
    }

    private int _mainPieces = 0;
    public int MainPieces
    {
        get { return _mainPieces; }
    }
    private List<int> _secondaryPieces = new List<int>();
    public List<int> SecondaryPieces
    {
        get { return _secondaryPieces; }
    }

    private List<NodeContext> _abilityNodes = new List<NodeContext>();
    public List<NodeContext> GetPieceAbilities(int mainPiece, int secundaryPiece)
    {
        return _abilityNodes.FindAll((node) =>
        {
            return node.MainIndex == mainPiece && node.SecundaryIndex == secundaryPiece;
        });
    }

    private List<NodeContext> _conditionNodes = new List<NodeContext>();
    public List<NodeContext> ConditionList
    {
        get { return _conditionNodes; }
    }
    public List<NodeContext> GetPieceConditions(int mainPiece, int secundaryPiece)
    {
        return _conditionNodes.FindAll((node) =>
        {
            return node.MainIndex == mainPiece && node.SecundaryIndex == secundaryPiece;
        });
    }
    public GraphToBehavior(Graph graph)
    {
        _graph = graph;
    }
    public List<KeyValuePair<int,Node>> FindBodies()
    {
        List<KeyValuePair<int, Node>> bodies = new List<KeyValuePair<int, Node>>();
        List<Connection> cons = _graph.GetAllNodeConnections(_graph.Nodes[0]);

        //find all main and side piece conditional nodes 
        int index = 0;
        foreach (Connection connection in cons)
        {
            bool foundCore = false;
            Node main = null;
            if (connection.EndNode.Type == NodeType.BossCore || connection.EndNode.Type == NodeType.BossCoreDynamic)
            {
                main = connection.EndNode;
                bodies.Add(new KeyValuePair<int, Node>(index, main));
                foundCore = true;
            }
            else if (connection.StartNode.Type == NodeType.BossCore || connection.StartNode.Type == NodeType.BossCoreDynamic)
            {
                main = connection.StartNode;
                bodies.Add(new KeyValuePair<int, Node>(index, main));
                foundCore = true;
            }

            if (foundCore)
            {
                List<Connection> sidePieces = FindConsOfType(_graph.GetAllNodeConnections(main), NodeType.SecundaryCore);
                foreach (Connection con in sidePieces)
                {
                    if (con.StartNode.Type == NodeType.SecundaryCore || con.StartNode.Type == NodeType.SecundaryCoreDynamic)
                    {
                        bodies.Add(new KeyValuePair<int, Node>(index, con.StartNode));
                    }
                    else if(con.EndNode.Type == NodeType.SecundaryCore || con.EndNode.Type == NodeType.SecundaryCoreDynamic)
                    {
                        bodies.Add(new KeyValuePair<int, Node>(index, con.EndNode));
                  
                    }
                }
                index++;
            }
        }

        return bodies;
    }
    public KeyValuePair<BehaviorTree,List<TreeNode>> CreateTree()
    {
        BehaviorTree tree = new BehaviorTree(new List<ITreeNode>());

        List<TreeNode> savableTree = new List<TreeNode>();

        int mainIndex = 0;
        List<Connection> cons = _graph.GetAllNodeConnections(_graph.Nodes[0]);

        //find all main and side piece conditional nodes 
        foreach (Connection connection in cons)
        {
            if (connection.EndNode.Type == NodeType.BossCore || connection.EndNode.Type == NodeType.BossCoreDynamic)
            {
                Debug.Log("" + mainIndex + 0 + connection.EndNode.Type);
                RootSequence coreSequence = new RootSequence(new List<ITreeNode>());//the core of a main boss piece
                tree.Nodes.Add(coreSequence);
                savableTree.Add(new TreeNode(){NodeType = TreeNodeType.RootSequence, SubNodes = new List<TreeNode>()});
                CheckTillEnd(connection.EndNode, mainIndex, 0, coreSequence, savableTree[savableTree.Count - 1].SubNodes);
                mainIndex++;
            }
            else if (connection.StartNode.Type == NodeType.BossCore || connection.StartNode.Type == NodeType.BossCoreDynamic)
            {
                Debug.Log("" + mainIndex + 0 + connection.StartNode.Type);
                RootSequence coreSequence = new RootSequence(new List<ITreeNode>());//the core of a main boss piece
                tree.Nodes.Add(coreSequence);
                savableTree.Add(new TreeNode() { NodeType = TreeNodeType.RootSequence, SubNodes = new List<TreeNode>() });
                CheckTillEnd(connection.StartNode, mainIndex, 0, coreSequence, savableTree[savableTree.Count -1].SubNodes);
                mainIndex++;
            }
        }

        _mainPieces = mainIndex;

        return new KeyValuePair<BehaviorTree, List<TreeNode>>(tree,savableTree);
    }
    private void CheckTillEnd(Node mainNode, int mainIndex,int secundaryIndex, RootSequence bossSequence, List<TreeNode> nodes)
    {
        List<Connection> cons = _graph.GetAllNodeConnections(mainNode);

        Connection con = null;

        con = FindConOfType(cons, NodeType.Movement); //find movement
        if (con != null)
        {
            Sequence moveSequance = new Sequence(new List<ITreeNode>());
            List<TreeNode> moveSequanceNodes = new List<TreeNode>();
            nodes.Add(new TreeNode(){NodeType = TreeNodeType.Sequence, SubNodes = moveSequanceNodes });
            Condition condition = new Condition();
            _moveConditions.Add(new NodeContext() { MainIndex = mainIndex, SecundaryIndex = secundaryIndex, Node = condition });
            moveSequance.Nodes.Add(condition);
            moveSequanceNodes.Add(new TreeNode(){NodeType = TreeNodeType.Condition});
            MovementNode node = new MovementNode();
            _moveNodes.Add(new NodeContext(){MainIndex = mainIndex,SecundaryIndex = secundaryIndex,Node = node});
            moveSequance.Nodes.Add(node);
            moveSequanceNodes.Add(new TreeNode() { NodeType = TreeNodeType.Movement });
            bossSequence.Nodes.Add(moveSequance);
        }

        //find side pieces

        if (mainNode.Type != NodeType.SecundaryCore && mainNode.Type != NodeType.SecundaryCoreDynamic)
        {
            List<Connection> sidePieces = FindConsOfType(cons, NodeType.SecundaryCore);//find secundary pieces
            int Index = 1;
            foreach (Connection connection in sidePieces)
            {
                if (connection.StartNode.Type == NodeType.SecundaryCore || connection.StartNode.Type == NodeType.SecundaryCoreDynamic)
                {
                    Debug.Log("" + mainIndex + Index + connection.StartNode.Type);
                    RootSequence sideSequence = new RootSequence(new List<ITreeNode>());//the core of a side boss piece
                    bossSequence.Nodes.Add(sideSequence);

                    List< TreeNode> rootNodes = new List<TreeNode>();
                    nodes.Add(new TreeNode(){NodeType = TreeNodeType.RootSequence, SubNodes  = rootNodes });

                    CheckTillEnd(connection.StartNode, mainIndex, Index, sideSequence, rootNodes);
                    Index++;
                }
                else
                {
                    Debug.Log("" + mainIndex + Index + connection.EndNode.Type);
                    RootSequence sideSequence = new RootSequence(new List<ITreeNode>());//the core of a side boss piece
                    bossSequence.Nodes.Add(sideSequence);

                    List<TreeNode> rootNodes = new List<TreeNode>();
                    nodes.Add(new TreeNode() { NodeType = TreeNodeType.RootSequence, SubNodes = rootNodes });

                    CheckTillEnd(connection.EndNode, mainIndex, Index, sideSequence, rootNodes);
                    Index++;
                }
            }

            _secondaryPieces.Add(Index);
        }

        //walk through phases

        List<TreeNode> phaseNodes = new List<TreeNode>();
    
        Selector phaseTracker = new Selector(new List<ITreeNode>());
        bossSequence.Nodes.Add(phaseTracker);

        con = FindConOfType(cons, NodeType.PhaseStart);  // find phaseStart node connected to this
        Node current = GetOtherNodeFromCon(con, mainNode.Id);

        List<Node> checkedNodes = new List<Node>();
        checkedNodes.Add(mainNode);

        List<TreeNode> phaseXNodes = new List<TreeNode>();
        List<TreeNode> seqNodes = new List<TreeNode>();

        Selector phaseX = null;
        Sequence seq = new Sequence(new List<ITreeNode>());

        int phaseNum = 0;

        //loop till we find final node
        while (current.Type != NodeType.Death)
        {
            cons = _graph.GetAllNodeConnections(current);
            cons = cons.FindAll(connection =>
            {
                return !(checkedNodes.Contains(connection.StartNode) || checkedNodes.Contains(connection.EndNode));
            });
            checkedNodes.Add(current);

            switch (current.Type)
            {
                case NodeType.Ability:
                    AbilityNode node = new AbilityNode();
                    _abilityNodes.Add(new NodeContext() { MainIndex = mainIndex, SecundaryIndex = secundaryIndex, Node = node, PhaseNumber = phaseNum });
                    phaseX.Nodes.Add(node);

                    phaseXNodes.Add(new TreeNode(){NodeType = TreeNodeType.Ability});

                    Debug.Log("ability for: " + mainIndex + " " + secundaryIndex);
                    break;
                case NodeType.PhaseStart:
                    if (phaseX != null)
                    {
                        Condition condition = new Condition();
                        _conditionNodes.Add(new NodeContext() { MainIndex = mainIndex, SecundaryIndex = secundaryIndex, Node = condition });

                        Debug.Log("condition for: " + mainIndex + " " + secundaryIndex);

                        seq.Nodes.Add(condition);
                        seq.Nodes.Add(phaseX);
                        phaseTracker.Nodes.Add(seq);
                        phaseNum++;


                        seqNodes.Add(new TreeNode(){NodeType = TreeNodeType.Condition});
                        seqNodes.Add(new TreeNode() { NodeType = TreeNodeType.Selector, SubNodes = phaseXNodes});
                        phaseNodes.Add(new TreeNode(){NodeType = TreeNodeType.Sequence, SubNodes = seqNodes});
                    }
                    phaseX = new Selector(new List<ITreeNode>());
                    seq = new Sequence(new List<ITreeNode>());

                    seqNodes = new List<TreeNode>();
                    phaseNodes = new List<TreeNode>();
                    break;
            }

            if (cons.Count > 1)
            {
                //set current node to the not condition node
                for (int i = 0; i < cons.Count; i++)
                {
                    Node conditionNode = GetNodeOfType(cons[i], NodeType.Event);
                    Node modifierNode = GetNodeOfType(cons[i], NodeType.Modifier);
                    if (conditionNode == null && modifierNode == null)
                    {
                        current = GetOtherNodeFromCon(cons[i], current.Id);
                        if (current.Type == NodeType.Death)
                        {
                            Condition condition = new Condition();

                            _conditionNodes.Add(new NodeContext() { MainIndex = mainIndex, SecundaryIndex = secundaryIndex, Node = condition });

                            Debug.Log("end condition for: " + mainIndex + " " + secundaryIndex);

                            seq = new Sequence(new List<ITreeNode>());
                            seq.Nodes.Add(condition);
                            seq.Nodes.Add(phaseX);
                            phaseTracker.Nodes.Add(seq);
                        }
                    }
                }
            }
            else
            {
                current = GetOtherNodeFromCon(cons[0], current.Id);
                if (current.Type == NodeType.Death)
                {
                    Condition condition = new Condition();

                    _conditionNodes.Add(new NodeContext() { MainIndex = mainIndex, SecundaryIndex = secundaryIndex, Node = condition });

                    Debug.Log("end condition for: " + mainIndex + " " + secundaryIndex);

                    seq = new Sequence(new List<ITreeNode>());
                    seq.Nodes.Add(condition);
                    seq.Nodes.Add(phaseX);
                    phaseTracker.Nodes.Add(seq);

                    seqNodes.Add(new TreeNode() { NodeType = TreeNodeType.Condition });
                    seqNodes.Add(new TreeNode() { NodeType = TreeNodeType.Selector, SubNodes = phaseXNodes });
                    phaseNodes.Add(new TreeNode() { NodeType = TreeNodeType.Sequence, SubNodes = seqNodes });
                }
            }
        }

        nodes.Add(new TreeNode() { NodeType = TreeNodeType.Selector, SubNodes = phaseNodes });
    }
    private Connection FindConOfType(List<Connection> cons, NodeType type)
    {
        return cons.Find(connection =>
        {
            return connection.StartNode.Type == type ||
                   connection.EndNode.Type == type;
        });
    }
    private List<Connection> FindConsOfType(List<Connection> cons, NodeType type)
    {
        if (type == NodeType.SecundaryCore)
        {
            return cons.FindAll(connection =>
            {
                return connection.StartNode.Type == type ||
                       connection.EndNode.Type == type ||
                       connection.StartNode.Type == NodeType.SecundaryCoreDynamic ||
                       connection.EndNode.Type == NodeType.SecundaryCoreDynamic;
            });
        }

        return cons.FindAll(connection =>
        {
            return connection.StartNode.Type == type ||
                   connection.EndNode.Type == type;
        });
    }
    private Node GetOtherNodeFromCon(Connection con, int id)
    {
        if (con != null)
        {
            if (con.StartNode.Id == id)
            {
                return con.EndNode;
            }
            else
            {
                return con.StartNode;
            }//add stat node
        }

        return null;
    }
    private Node GetNodeOfType(Connection con, NodeType type)
    {
        if (con.StartNode.Type == type)
        {
            return con.StartNode;
        }
        else if (con.EndNode.Type == type)
        {
            return con.EndNode;
        }

        return null;
    }
}
