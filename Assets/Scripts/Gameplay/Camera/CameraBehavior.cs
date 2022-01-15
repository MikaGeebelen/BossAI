using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] private Transform _player = null;
    [SerializeField] private float _cameraSpeed = 0.5f;

    void FixedUpdate()
    {
        Vector2 dir = _player.transform.position - transform.position;
        transform.Translate(dir * _cameraSpeed * Time.fixedDeltaTime);
    }
}
