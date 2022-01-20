using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "abilities", menuName = "Abilities/FireThrow", order = 5)]
public class FireThrow : BaseComboAbility
{
    public GameObject BulletPrefab = null;
    public float ChargeTime = 0.0f;

    private float _currentChargeTime = 0.0f;

    private bool _isCharging = false;

    private Boss _boss = null;

    private Health _tarHealth = null;
    private GameObject _tar = null;
    private Tar _tarScript = null;

    private Transform _body = null;

    private bool _gottenTar = false;

    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);

        _tar = board.GetValue<GameObject>(_personalAcces);

        _boss = board.GetValue<Boss>(initList[0]);
        _body = board.GetValue<GameObject>(initList[1]).transform;
    }

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() {Requirements.Spawner,Requirements.Origin};
    }

    public override void SetupScripted(BaseAbility ability)
    {
        FireThrow fire = null;
        if (ability is FireThrow)
        {
            fire = (ability as FireThrow);
        }

        BulletPrefab = fire.BulletPrefab;
        ChargeTime = fire.ChargeTime;
        MaxCooldown = fire.MaxCooldown;
    }

    public override BehaviorTree.TreeState CastAbility()
    {
        if (!_gottenTar)
        {
            _gottenTar = true;
            _tarHealth = _tar.GetComponent<Health>();
            _tarScript = _tar.GetComponent<Tar>();
        }

        if (_tarScript.IsOnFire)
        {
            return BehaviorTree.TreeState.Failed;
        }

        if (!_isOnCooldown || _isCharging)
        {
            _isCharging = true;

            if (_currentChargeTime > ChargeTime)
            {
                if (_tarHealth.CurrentHealth > 0)
                {
                    Vector2 bulletDir = _tar.transform.position - _body.position;
                    float angle = Vector2.SignedAngle(Vector2.right, bulletDir);
                    _boss.SpawnProjectle(BulletPrefab, _body.position, new Vector3(0, 0, angle));

                    _isCharging = false;
                    _isOnCooldown = true;
                    _currentChargeTime = 0.0f;
                    return BehaviorTree.TreeState.Succes;
                }
            }
            else
            {
                if (_tarHealth.CurrentHealth <= 0)
                {

                    _isCharging = false;
                    _isOnCooldown = true;
                    _currentChargeTime = 0.0f;
                    return BehaviorTree.TreeState.comboFail;
                }
                return BehaviorTree.TreeState.Running;
            }

        }
        return BehaviorTree.TreeState.Failed;
    }

    protected override void Update()
    {
        base.Update();
        if (_isCharging)
        {
            _currentChargeTime += Time.deltaTime;
        }
    }
}
