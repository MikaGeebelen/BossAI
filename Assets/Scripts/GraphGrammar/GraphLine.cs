using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphLine : MonoBehaviour
{
    private Transform _startPos = null;
    private Transform _endPos = null;

    private LineRenderer _renderer = null;

    private void Start()
    {
        _renderer = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        if (_startPos == null || _endPos == null)
        {
            return;
        }

        List<Vector3> posList = new List<Vector3>();
        posList.Add(_startPos.position);
        posList.Add(_endPos.position);


        _renderer.positionCount = 2;
        _renderer.SetPositions(posList.ToArray());
    }

    public void SetPos(Transform start, Transform end)
    {
        _startPos = start;
        _endPos = end;
    }
}
