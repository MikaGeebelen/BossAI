using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossDefinition : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropDown = null;
    [SerializeField] private TMP_InputField _inputField = null;
    [SerializeField] private TextMeshProUGUI _text = null; 

    private BossBuilder _builder = null;
    private NodeInfo _data = new NodeInfo();
    private List<object> _types = null;

    public void Setup(BossBuilder builder, NodeInfo data, List<object> types)
    {
        _builder = builder;
        _data = data;
        _types = types;

        //fill in all text and dropdown Menus
        if (_dropDown != null)
        {
            _dropDown.ClearOptions();
            foreach (object type in _types)
            {
                //Type name = type.GetType();

                _dropDown.options.Add(new TMP_Dropdown.OptionData(type.ToString()));
            }

            _dropDown.SetValueWithoutNotify(_data.CurrentValue);

            _dropDown.onValueChanged.AddListener(OnValueChanged);
        }

        if (_inputField != null)
        {
            //it is a health object
            _inputField.onEndEdit.AddListener(OnEndEdit);
            _inputField.text = _data.CurrentValue.ToString();
        }

        if (_data.Type == NodeFill.Ability || _data.Type == NodeFill.Condition)
        {
            _text.text = "node type: " + _data.Type.ToString() + " for piece: " + _data.MainPiece + " " + _data.SecundaryPiece + "\n" + "Used in phase: " + _data.PhaseNumber;
        }
        else
        {
            _text.text = "node type: " + _data.Type.ToString() + " for piece: " + _data.MainPiece + " " + _data.SecundaryPiece;
        }
     
    }

    private void OnValueChanged(int newValue)
    {
        _data.ObjectType = _types[newValue].GetType();
        _data.CurrentValue = newValue;
        _builder.ChangeNodeContents(_data);
    }

    private void OnEndEdit(string number)
    {
        _data.HealthComp.Setup(int.Parse(number));
    }

}
