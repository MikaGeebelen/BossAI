using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Movement", menuName = "Movement/Chase", order = 1)]
public class ChasePlayer : BaseMovement
{
    public float MoveSpeed = 0.0f;

    private Transform _playerPos = null;
    private Transform _bossPos = null;

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() {Requirements.Target,Requirements.Origin};
    }

    public override void Setup(BlackBoard board, List<string> initList)
    {
        _playerPos = board.GetValue<GameObject>(initList[0]).transform;
        _bossPos = board.GetValue<GameObject>(initList[1]).transform;
    }

    public override void SetupScripted(BaseMovement move)
    {
        MoveSpeed = (move as ChasePlayer).MoveSpeed;
    }

    public override void Move()
    {
        Vector2 moveDir = _playerPos.position - _bossPos.position;
        moveDir.Normalize();

        _bossPos.Translate( moveDir * MoveSpeed * Time.deltaTime);
    }
}
