using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Start,
    Event,
    Enrage,
    RoomEvent,
    Ability,
    ComboAbility,
    Modifier,
    PhaseStart,
    Death,
    BossCore,
    BossCoreDynamic,
    SecundaryCore,
    SecundaryCoreDynamic,
    Movement,
    MoveBooster,
    final
}
public class Node
{
    private int _id = 0;
    public int Id
    {
        get
        {
            return _id;
        }
    }

    private NodeType _type = NodeType.Start;

    public NodeType Type
    {
        get
        {
        return _type;
        }
        set
        {
            _type = value;
        }
    }
    public Node(int id,NodeType type)
    {
        _id = id;
        _type = type;
    }
}

[Serializable]
public class SavedNode
{
    public int Id = 0;
    public int Type = 0;

    public string CycleType()
    {
        Type++;
        if (Type > (int)NodeType.final)
        {
            Type = 0;
        }

        return ((NodeType)Type).ToString();
    }
}
