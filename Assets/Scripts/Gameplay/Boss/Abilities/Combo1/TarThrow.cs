using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "abilities", menuName = "Abilities/TarThrow", order = 4)]
public class TarThrow : BaseComboAbility
{
    public Sprite ProjectileTar = null;

    public GameObject FireParticle = null;

    private Transform _player = null;
    private Transform _body = null;
    private GameObject _tar = null;

    private BlackBoard _board = null;

    private Tar _tarScript = null;
    
    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);
        _board = board;
        _player = _board.GetValue<GameObject>(initList[0]).transform;
        _body = _board.GetValue<GameObject>(initList[1]).transform;

        _tar = _board.GetValue<GameObject>(_personalAcces);

        _tarScript = _tar.AddComponent<Tar>();
        _tar.tag = _body.tag;

        _tar.SetActive(false);
    }

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() { Requirements.Target, Requirements.Origin };
    }

    public override void SetupScripted(BaseAbility ability)
    {
        TarThrow tar = null;
        if (ability is TarThrow)
        {
            tar = (ability as TarThrow);
        }

        ProjectileTar = tar.ProjectileTar;
        FireParticle = tar.FireParticle;
        MaxCooldown = tar.MaxCooldown;

        _tarScript.AddVisuals(ProjectileTar,FireParticle);
    }

    public override BehaviorTree.TreeState CastAbility()
    {
        if (!_isOnCooldown)
        {
            _tar.SetActive(true);
            _tar.transform.position = _body.position;
            _tarScript.Fire(_player.position);
            _isOnCooldown = true;
            return BehaviorTree.TreeState.Succes;
        }

        return BehaviorTree.TreeState.Failed;
    }
}
