using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BaseMovement : ScriptableObject
{
    public int PointCost = 0;
    abstract public List<Requirements> GetRequirements();
    abstract public void Setup(BlackBoard board, List<string> initList);
    abstract public void SetupScripted(BaseMovement move);
    abstract public void Move();
}
