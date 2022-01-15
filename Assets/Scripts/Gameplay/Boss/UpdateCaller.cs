using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UpdateCaller : MonoBehaviour
{
    public static UnityEvent OnUpdate = new UnityEvent();
    private void Update()
    {
        OnUpdate.Invoke();
    }
}
