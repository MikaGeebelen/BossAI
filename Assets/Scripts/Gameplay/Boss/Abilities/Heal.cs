using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "abilities", menuName = "Abilities/Heal", order = 3)]
public class Heal : BaseAbility
{
    public bool OnlyHealSelf = false;
    public float MaxTimeForHeal = 0.0f;
    public int HealIntervals = 0;
    public int HealingAmount = 10;

    public GameObject ParticlePrefab = null;

    private float _maxTimePerInterval = 0;
    private float _currentHealTime = 0.0f;
    private bool _isHealing = false;
    private int _currentHeal = 0;

    private List<KeyValuePair<int, int>> _allPieces = new List<KeyValuePair<int, int>>();
    private List<KeyValuePair<int, int>> _healablePieces = new List<KeyValuePair<int, int>>();

    private Transform _healOrigin = null;

    private List<Health> _healths = new List<Health>();

    private Transform _currentTarget = null;
    private Health _currentHealthTarget = null;

    private BlackBoard _board = null;

    private Boss _boss = null;

    public override List<Requirements> GetRequirements()
    {
        return new List<Requirements>() { Requirements.MovementOrigin,Requirements.Origin, Requirements.PieceIndex,Requirements.Spawner };
    }

    public override void Setup(BlackBoard board, List<string> initList)
    {
        base.Setup(board, initList);
        _board = board;
        _healOrigin = board.GetValue<GameObject>(initList[1]).transform;

        _boss = board.GetValue<Boss>(initList[4]);

        string main = "mainPieceHealth";
        string secundary = "secundaryHealth";

        int mainIndex = 0;
        int secundaryIndex = 0;

        if (OnlyHealSelf)
        {
            int ownMain = int.Parse(initList[2]);
            _healths.Add(board.GetValue<Health>(main + ownMain + 0));
            _allPieces.Add(new KeyValuePair<int, int>(ownMain, 0));
            while (board.DoesValueExist(secundary + ownMain + secundaryIndex))
            {
                _healths.Add(board.GetValue<Health>(secundary + ownMain + secundaryIndex));
                _allPieces.Add(new KeyValuePair<int, int>(ownMain, secundaryIndex + 1));
                secundaryIndex++;
            }  //find all secundaryPieces pieces
        }
        else
        {
            do
            {
                _healths.Add(board.GetValue<Health>(main + mainIndex + secundaryIndex));
                _allPieces.Add(new KeyValuePair<int, int>(mainIndex,secundaryIndex));
                mainIndex++;
            } while (board.DoesValueExist(main + mainIndex + secundaryIndex)); //find all main pieces

            int mainPieceCount = _allPieces.Count;

            for (int i = 0; i < mainPieceCount; i++)
            {
                while (board.DoesValueExist(secundary + i + secundaryIndex))
                {
                    _healths.Add(board.GetValue<Health>(secundary + i + secundaryIndex));
                    _allPieces.Add(new KeyValuePair<int, int>(i, secundaryIndex+1));
                    secundaryIndex++;
                }  //find all secundaryPieces pieces
            }
        }


        for (int i = 0; i < _healths.Count; i++)
        {
            int index = i;

            _healths[index].FullEvent.AddListener(delegate {RemoveFullPiece(_healths[index], _allPieces[index].Key,_allPieces[index].Value); });
            _healths[index].Damaged.AddListener(delegate { AddDamagedPiece(_healths[index],_allPieces[index].Key, _allPieces[index].Value); });
        }
    }

    public override BehaviorTree.TreeState CastAbility()
    {
        string main = "mainPieceHealth";
        string secundary = "secundaryHealth";
        if (!_isOnCooldown || _isHealing)
        {
            _isHealing = true;
            if (_currentHealthTarget == null && _currentTarget == null)
            {
                if (_healablePieces.Count <=0)
                {
                    return BehaviorTree.TreeState.Failed;
                }

                KeyValuePair<int,int> piece = _healablePieces[ Random.Range(0, _healablePieces.Count)];

                if (piece.Value > 0)
                {
                    _currentHealthTarget =
                        _board.GetValue<Health>(secundary + piece.Key + (piece.Value - 1).ToString());
                    _currentTarget = _currentHealthTarget.transform;
                }
                else
                {
                    _currentHealthTarget =
                        _board.GetValue<Health>(main + piece.Key + 0);
                    _currentTarget = _currentHealthTarget.transform;
                }

            }//set target to be healed

            for (int i = _currentHeal; i < HealIntervals; i++)
            {
                if (_maxTimePerInterval <= _currentHealTime)
                {
                    _currentHealTime = 0.0f;
                    _currentHealthTarget.Heal(HealingAmount/HealIntervals);
                    _boss.SpawnProjectle(ParticlePrefab, _currentTarget.position, Vector3.zero);
                }
                else
                {
                    _currentHeal = i;
                    return BehaviorTree.TreeState.Running;
                }
            }

            _currentHeal = 0;
            _currentHealTime = 0.0f;
            _isHealing = false;
            _isOnCooldown = true;
            return BehaviorTree.TreeState.Succes;
        }

        return BehaviorTree.TreeState.Failed;
    }

    protected override void Update()
    {
        base.Update();
        if (_isHealing)
        {
            _currentHealTime += Time.deltaTime;
        }
    }

    public override void SetupScripted(BaseAbility ability)
    {
        Heal heal = null;
        if (ability is Heal)
        {
            heal = (ability as Heal);
        }

        OnlyHealSelf = heal.OnlyHealSelf;
        MaxTimeForHeal = heal.MaxTimeForHeal;
        HealIntervals = heal.HealIntervals;
        HealingAmount = heal.HealingAmount;

        ParticlePrefab = heal.ParticlePrefab;
        _maxTimePerInterval = HealIntervals / MaxTimeForHeal;
    }

    private void RemoveFullPiece(Health thisHealth, int main, int secundary)
    {
        _healablePieces.Remove(new KeyValuePair<int, int>(main, secundary));
    }

    private void AddDamagedPiece(Health thisHealth, int main, int secundary)
    {
        if (_allPieces.Contains(new KeyValuePair<int, int>(main,secundary)))
        {
            _healablePieces.Add(new KeyValuePair<int, int>(main, secundary));
        }
    }

    //private void RemoveDeadPiece(Health thisHealth, int main, int secundary)
    //{
    //    _allPieces.Remove(new KeyValuePair<int, int>(main, secundary));
    //    thisHealth.FullEvent.RemoveListener(delegate { RemoveFullPiece(thisHealth, main, secundary); });
    //    thisHealth.Damaged.RemoveListener(delegate { AddDamagedPiece(thisHealth, main, secundary); });
    //    thisHealth.DiedEvent.RemoveListener(delegate { RemoveDeadPiece(thisHealth, main, secundary); });
    //}
}
