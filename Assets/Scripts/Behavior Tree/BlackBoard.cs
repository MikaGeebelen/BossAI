using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BlackBoard
{
    private Dictionary<string, object> _blackBoardValues = new Dictionary<string, object>();

    public void AddValue(string name, object newValue)
    {
        _blackBoardValues.Add(name, newValue);
    }
    public bool RemoveValue(string valueName)
    {
        return _blackBoardValues.Remove(valueName);
    }
    public Type GetType(string name)
    {
        return _blackBoardValues[name].GetType();
    }
    public T GetValue<T>(string name)
    {
        if (!_blackBoardValues.ContainsKey(name))
        {
            Debug.Log("does not contain" + name);
            return default(T);
        }

        if (_blackBoardValues[name] is T)
        {
            return (T)_blackBoardValues[name];
        }
        else
        {
            Debug.Log("Illegal coversion");
            return default(T);
        }
    }
    public bool SetValue<T>(string name, T newValue)
    {
        if (_blackBoardValues[name] is T)
        {
            _blackBoardValues[name] = newValue;
            return true;
        }
        else
        {
            Debug.Log("deifferent type expected");
            return false;
        }
    }
    public bool DoesValueExist(string name)
    {
        return _blackBoardValues.ContainsKey(name);
    }
}
