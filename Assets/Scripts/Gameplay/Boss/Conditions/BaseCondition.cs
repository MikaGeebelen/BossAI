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
    protected int _allPhaseNum = 0;
    protected bool _hasAllPhaseNum = false;

    protected int _main = 0;
    protected int _secundary = 0;

    protected bool _isMovementCondition = false;

    abstract public List<Requirements> GetRequirements();

    public virtual void Setup(BlackBoard board, List<string> initList)
    {
        _board = board;

        if (int.TryParse(initList[0],out int result))
        {
            if (initList[initList.Count-1] == "IsMovement")
            {
                _isMovementCondition = true;
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

    abstract public void SetupScripted(BaseCondition condition);

    protected virtual bool CheckPrerequisites()
    {
        if (!_hasAllPhaseNum)
        {
            do
            {
                _allPhaseNum++;
            } while (_board.DoesValueExist(ConditionAcces + _main + _secundary + _allPhaseNum));

            _hasAllPhaseNum = true;
        }

        for (int i = 0; i < _phaseNum; i++)
        {
            if (_board.GetValue<bool>(ConditionAcces + _main + _secundary + i))
            {
                return false;
            }
        }

        return true;
    }

    public virtual KeyValuePair<int, int> GetOther()
    {
        return new KeyValuePair<int, int>(-1, -1);
    } 

}
