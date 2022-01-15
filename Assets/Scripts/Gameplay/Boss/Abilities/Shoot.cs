using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "abilities", menuName = "Abilities/Shoot", order = 1)]
public class Shoot : BaseAbility
{
    public GameObject BulletPrefab = null;
    public int NumberOfShots = 1;
    public float BulletCone = 0.0f;

    public float MaxTimeBetweenBullets = 0.0f;
    private float _currentBulletTime = 0.0f;
    private bool _isFiring = true;
    private int _currentShot = 0;

    private Transform _playerPos = null;
    private Transform _bossPos = null;
    private Boss _mainComp = null;

    public override List<Requirements> GetRequirements()
    {
       return new List<Requirements>(){Requirements.Spawner,Requirements.Target,Requirements.Origin};
    }

    public override void Setup(BlackBoard board,List<string> initList)
    {
        base.Setup(board,initList);
        _mainComp = board.GetValue<Boss>(initList[0]);
        _playerPos = board.GetValue<GameObject>(initList[1]).transform;
        _bossPos = board.GetValue<GameObject>(initList[2]).transform;
    }

    public override void SetupScripted(BaseAbility ability)
    {
        Shoot shot = null;  
        if (ability is Shoot)
        {
            shot = (ability as Shoot);
        }

        BulletPrefab = shot.BulletPrefab;
        NumberOfShots = shot.NumberOfShots;
        BulletCone = shot.BulletCone;
        MaxTimeBetweenBullets = shot.MaxTimeBetweenBullets;
        MaxCooldown = shot.MaxCooldown;
    }

    public override BehaviorTree.TreeState CastAbility()
    {
        if (!_isOnCooldown || _isFiring)
        {
            Vector2 bulletDir = _playerPos.position - _bossPos.position;
            float angle = Vector2.SignedAngle(Vector2.right, bulletDir);
            angle -= BulletCone / 2;
            float angleBetweenShots = BulletCone / NumberOfShots;

            _isFiring = true;
            for (int i = _currentShot; i < NumberOfShots; i++)
            {
                if (MaxTimeBetweenBullets <= _currentBulletTime)
                {
                    _currentBulletTime = 0.0f;
                    _mainComp.SpawnProjectle(BulletPrefab, _bossPos.position, new Vector3(0, 0, angle + angleBetweenShots * i));
                }
                else
                {
                    _currentShot = i;
                    return BehaviorTree.TreeState.Running;
                }
            }

            _currentShot = 0;
            _isOnCooldown = true;
            _isFiring = false;
            return BehaviorTree.TreeState.Succes;

        }
        return BehaviorTree.TreeState.Failed;
    }

    protected override void Update()
    {
        base.Update();
        if (_isFiring)
        {
            _currentBulletTime += Time.deltaTime;
        }
    }
}
