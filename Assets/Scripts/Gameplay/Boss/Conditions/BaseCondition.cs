using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

abstract public class BaseCondition: ScriptableObject
{
    public static string ConditionAcces = "Condition";

    protected string _personalAcces = "";

    protected BlackBoard _board = null;

    protected int _phaseNum = 0;

    protected int _main = 0;
    protected int _secundary = 0;

    abstract public List<Requirements> GetRequirements();

    public virtual void Setup(BlackBoard board, List<string> initList)
    {
        _board = board;

        if (int.TryParse(initList[0],out int result))
        {
            if (initList.Count > 3 && initList[3] == "IsMovement")
            {
                return;
            }

            AddToBlackBoard(result, int.Parse(initList[1]));
        }
        else
        {
            Debug.LogError("you are required to set add the indices of the main and secundary to the INITLIST do this by adding PIECEINDEX to the requirements",this);
        }


    }

    private void AddToBlackBoard(int main, int secundary)
    {
        _main = main;
        _secundary = secundary;

        while (_board.DoesValueExist(ConditionAcces + _main + _secundary + _phaseNum)) 
        {
            _phaseNum++;
        } 

        _personalAcces = ConditionAcces + _main + _secundary + _phaseNum;
        _board.AddValue(_personalAcces, false);
    }
    abstract public bool TestCondition();
}
