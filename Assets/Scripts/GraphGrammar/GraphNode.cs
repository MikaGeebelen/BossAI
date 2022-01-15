using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GraphNode : MonoBehaviour
{
    public UnityEvent OnRightClick = new UnityEvent();
    public UnityEvent OnMiddleClick = new UnityEvent();
    private void OnMouseOver()
    {
        if (Input.GetButtonDown("ConChaNode")) OnRightClick.Invoke();
        if (Input.GetButtonDown("RemoveNode")) OnMiddleClick.Invoke();
    }
}
