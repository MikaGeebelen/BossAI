using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "abilities", menuName = "Abilities/Dash", order = 2)]
public class Dash : BaseAbility
{
    public float DashDistance = 5.0f;
    public float DashSpeed = 7.0f;

    public bool DashToTarget = false;
    public float BlindConeToTarget = 0.0f;

    private bool _isDashing = false;
    private Vector2 _originalPos = new Vector2();
    private Vector2 _dashDir = new Vector2();

    private Transform _playerPos = null;
    private Transform _bossPos = null;

    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);

        _playerPos = board.GetValue<GameObject>(initList[0]).transform;
        GameObject boss = board.GetValue<GameObject>(initList[1]);
        _bossPos = boss.transform;

    }

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() { Requirements.Target, Requirements.Origin };
    }

    public override void SetupScripted(BaseAbility ability)
    {
        Dash dash = null;
        if (ability is Dash)
        {
            dash = (ability as Dash);
        }

        DashDistance = dash.DashDistance;
        DashSpeed = dash.DashSpeed;
        MaxCooldown = dash.MaxCooldown;
    }

    public override BehaviorTree.TreeState CastAbility()
    {
        if (!_isOnCooldown || _isDashing)
        {
            if (!_isDashing)
            {
                _isDashing = true;
                _originalPos = _bossPos.position;
                if (!DashToTarget)
                {
                    _dashDir = _bossPos.position - _playerPos.position;
                    _dashDir = Quaternion.Euler(0, 0, Random.Range(-180.0f + (BlindConeToTarget/2), 180.0f - (BlindConeToTarget/2))) * _dashDir;
                    _dashDir.Normalize();
                }
                else
                {
                    _dashDir = _playerPos.position - _bossPos.position;
                    _dashDir.Normalize();
                }
            }

            _bossPos.Translate(_dashDir * DashSpeed * Time.deltaTime);

            if (Vector2.Distance(_originalPos,_bossPos.position) > DashDistance)
            {
                _isOnCooldown = true;
                _isDashing = false;
                return BehaviorTree.TreeState.Succes;
            }
            else
            {
                return BehaviorTree.TreeState.Running;
            }

        }

        return BehaviorTree.TreeState.Failed;
    }
}
