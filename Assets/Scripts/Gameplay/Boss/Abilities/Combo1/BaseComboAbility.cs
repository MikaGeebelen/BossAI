using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseComboAbility : BaseAbility
{
    protected string _personalAcces = "combo";
    private bool _hasAcces = false;
    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);
        int accesIndex = 0;

        while (board.DoesValueExist(_personalAcces + accesIndex))
        {
            GameObject obj = board.GetValue<GameObject>(_personalAcces + accesIndex);
            UsedInCombo combo = obj.GetComponent<UsedInCombo>();
            if (combo != null && !combo.Has2Comps())
            {
                _hasAcces = true;
                combo.ClaimBuddy();
                break;
            }
            accesIndex++;
        }

        _personalAcces += accesIndex.ToString();

        if (!_hasAcces)
        {
            board.AddValue(_personalAcces, new GameObject(_personalAcces));
            _hasAcces = true;
        }
    }
}
