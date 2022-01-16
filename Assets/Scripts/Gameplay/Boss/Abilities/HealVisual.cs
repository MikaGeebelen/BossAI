using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealVisual : MonoBehaviour
{
    [SerializeField] private LineRenderer _renderer = null;
     private float _lifeTime = 0.1f;
    [SerializeField] private float _startWidth = 0.1f;
    [SerializeField] private float _endtWidth = 0.0f;

    private float _currentTime = 0.0f;
    private bool _isSetup = false;
    public void SetLine(Vector3 pos1, Vector3 pos2, float lifeTime)
    {
        _lifeTime = lifeTime;
        _renderer.positionCount = 2;
        _renderer.SetPositions(new Vector3[]{ pos1 , pos2});
        _isSetup = true;
        Destroy(gameObject, _lifeTime);
    }

    private void Update()
    {
        if (_isSetup)
        {
            _currentTime += Time.deltaTime;
            _renderer.widthMultiplier = Mathf.Lerp(_startWidth, _endtWidth, _currentTime / _lifeTime);
        }
    }
}
