using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INodeType
{
    //return bool to test later behavior tree returns -> Succes Failed Running
    public bool ExecuteAction();
}
