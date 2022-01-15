using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 0.1f;
    [SerializeField] private float _borderRange = 10.0f;

    private float _screenWidth = 0;
    private float _screenHeight = 0;

    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
    }

    private void FixedUpdate()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 moveDir = new Vector2();


        if (mousePos.x < _borderRange)
        {
            moveDir.x += -1;
        }

        if (mousePos.x > _screenWidth - _borderRange)
        {
            moveDir.x += 1;
        }

        if (mousePos.y < _borderRange)
        {
            moveDir.y += -1;
        }

        if (mousePos.y > _screenHeight - _borderRange)
        {
            moveDir.y += 1;
        }

        transform.Translate(moveDir * Time.fixedDeltaTime * _sensitivity);
    }
}
