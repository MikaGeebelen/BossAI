using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarThrow : BaseComboAbility
{
    private Transform _player = null;
    private Transform _body = null;
    private GameObject _tar = null;

    private BlackBoard _board = null;
    private bool _isTarSetup = false;

    private CircleCollider2D _tarCollider = null;
    private SpriteRenderer _renderer = null;    
    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);
        _board = board;
        _player = _board.GetValue<GameObject>(initList[0]).transform;
        _body = _board.GetValue<GameObject>(initList[1]).transform;

        _tar = _board.GetValue<GameObject>(_personalAcces);
        _tar.SetActive(false);
    }

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() { Requirements.Target, Requirements.Origin };
    }

    public override void SetupScripted(BaseAbility ability)
    {
        throw new System.NotImplementedException();
    }

    public override BehaviorTree.TreeState CastAbility()
    {
        if (!_isOnCooldown)
        {
            if (!_isTarSetup)
            {
                _tar.SetActive(true);
                _tarCollider = _tar.AddComponent<CircleCollider2D>();
                _tarCollider.radius = 0.15f;

                _renderer = _tar.AddComponent<SpriteRenderer>();
                _renderer.sprite = null;
            }
        }

        return BehaviorTree.TreeState.Failed;
    }
}
