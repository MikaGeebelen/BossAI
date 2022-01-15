using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

[CreateAssetMenu(fileName = "Movement", menuName = "Movement/Orbit", order = 2)]
public class OrbitingMovement : BaseMovement
{
    public float OrbitingSpeed = 0.0f;
    public float OrbitingRange = 0.0f;
    public float SpeedBasedOnPlayer = 0.0f;

    private Transform _playerPos = null;
    private Transform _bossPos = null;
    private Transform _orbitTarget = null;

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() { Requirements.Target, Requirements.Origin,Requirements.MovementOrigin };
    }

    public override void Setup(BlackBoard board, List<string> initList)
    {
        _playerPos = board.GetValue<GameObject>(initList[0]).transform;
        _bossPos = board.GetValue<GameObject>(initList[1]).transform;
        _orbitTarget = board.GetValue<GameObject>(initList[2]).transform;
    }

    public override void SetupScripted(BaseMovement move)
    {
        OrbitingMovement movement = (move as OrbitingMovement);

        OrbitingSpeed = movement.OrbitingSpeed;
        OrbitingRange = movement.OrbitingRange;
        SpeedBasedOnPlayer = movement.SpeedBasedOnPlayer;

        _bossPos.position = new Vector3(_orbitTarget.position.x, _orbitTarget.position.y + OrbitingRange, _orbitTarget.position.z);
    }

    public override void Move()
    {

        float distanceDiff = ((Vector3.Distance(_bossPos.position, _playerPos.position) -
            Vector3.Distance(_orbitTarget.position, _playerPos.position))) / OrbitingRange;//get a value between -1 and 1 where -1 is the closest to the player and 1 is the furthest away

        float speedModifier = 0;
        if (distanceDiff < 0)
        {
            speedModifier = Mathf.Abs(distanceDiff) * (SpeedBasedOnPlayer * OrbitingSpeed);
        }
        _bossPos.RotateAround(_orbitTarget.position, Vector3.forward, (OrbitingSpeed + speedModifier)* Time.deltaTime);
    }
}
