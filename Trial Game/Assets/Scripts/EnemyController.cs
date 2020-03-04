﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Enums.Player _player;
    PlayerManager _pM;
    UnitController _unitController;

    public bool NextAction()
    {
        UnitController target = null;
        var units = _pM.GetPlayerInfo(Enums.Player.Player1).Units;

        int maxAttackRange = _unitController.MoveDistance + _unitController.MaxAttackDistance;
        int minAttackRange = _unitController.MinAttackDistance;

        List<UnitController> inRangeUnits = units.Where(u => u.Position.GridDistance(_unitController.Position) == _unitController.MaxAttackDistance).ToList();
        units = units.Except(inRangeUnits).ToList();
        List<UnitController> tooCloseUnits = units.Except(inRangeUnits).Where(u => u.Position.GridDistance(_unitController.Position) <= minAttackRange).ToList();
        units = units.Except(tooCloseUnits).ToList();
        List<UnitController> closeUnits = units.Except(inRangeUnits.Union(tooCloseUnits)).Where(u => u.Position.GridDistance(_unitController.Position) <= maxAttackRange).ToList();
        units = units.Except(closeUnits).ToList();
        List<UnitController> targets = units.Except(tooCloseUnits).ToList();
        
        if(inRangeUnits.Count > 0)
        {
            target = inRangeUnits.FirstOrDefault();
        }
        else if(tooCloseUnits.Count > 0)
        {
            target = tooCloseUnits.FirstOrDefault();
        }
        else if(closeUnits.Count > 0)
        {
            target = closeUnits.OrderBy(u => u.Position.GridDistance(_unitController.Position)).FirstOrDefault();
        }
        else if(targets.Count > 0)
        {
            target = targets.OrderBy(u => u.Position.GridDistance(_unitController.Position)).FirstOrDefault();
        }

        if (target != null)
        {
            if (_unitController.CheckAttack(target))
                return true;

            var gbTarget = BestSpaceNextToTarget(target.CurrentGridBlock, _unitController.CurrentGridBlock);

            _unitController.Target = target;
            return MoveToNextSpace(gbTarget, target);
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        _unitController = gameObject.GetComponent<UnitController>();
        _player = _unitController.Player;
        _unitController.EnemyController = this;
    }

    private bool MoveToNextSpace(GridBlock gbTarget, UnitController ucTarget)
    {
        if (_unitController.CurrentGridBlock != null)
        {
            _pM.GetPlayerInfo(_player).ResetNonPlayerGrid = false;
            // Try to create a path to target unit. Path returns 0, then the unit is stuck and can't move, so don't put it on cooldown.
            var _moves = _pM.CreatePath(_unitController.Player, _unitController.CurrentGridBlock, gbTarget).ToList();

            List<GridBlock> backupSpaces = null;
            double dis = 9999;
            if (ucTarget != null)
                dis = _unitController.Position.GridDistance(ucTarget.Position);
            _unitController.Target = ucTarget;


            if (dis <= _unitController.MaxAttackDistance && dis > _unitController.MinAttackDistance)
            {
                _unitController.CheckAttack(gbTarget.CurrentUnit);
                return true;
            }
            else if (_moves?.Count > 0 && dis > _unitController.MaxAttackDistance)
            {
                _moves = MaxMovementPath(_moves, _unitController.MoveDistance + _unitController.CurrentGridBlock.MovementCost).ToList();
                _pM.ResetPathMatrix(_player);

                if (_unitController.Target != null)
                {
                    int index = Mathf.Clamp(_moves.Count - (_unitController.MaxAttackDistance - 1), 0, 9999);
                    int amount = Mathf.Clamp(_unitController.MaxAttackDistance - 1, 0, _moves.Count);
                    _moves.RemoveRange(index, amount);
                    _unitController.MoveTo(_moves);
                    return true;
                }
                if (_moves.Count > 0)
                {
                    if (_unitController.Target != null)
                        _moves.RemoveRange(_moves.Count - (_unitController.MaxAttackDistance - 1), _unitController.MaxAttackDistance - 1);
                    _unitController.MoveTo(_moves);
                    return true;
                }
            }
            else if ((backupSpaces = _unitController.CurrentGridBlock.AvailableAttackSpace(gbTarget)).Count > 0)
            {
                _unitController.MoveTo(new List<GridBlock>() { _unitController.CurrentGridBlock, backupSpaces.First() });
                return true;
            }
            else
            {
                _unitController.Target = null;
            }

            //if (path.Count > 1)
            //{
            //    path = MaxMovementPath(path, _unitController.MoveDistance + _unitController.CurrentGridBlock.MovementCost).ToList();
            //    _pM.ResetPathMatrix(_player);
            //    _unitController.MoveTo(path);
            //    return true;
            //}
            //return false;
        }

        Debug.Log("Unit Controller no GridBlock");
        return false;
    }

    private GridBlock BestSpaceNextToTarget(GridBlock target, GridBlock start)
    {
        GridBlock result = null;
        var neighbors = target.Neighbors;
        var possibleBest = neighbors.OrderByDistance(start).Where(n => n.CurrentUnit == null && !n.Unpassable).ToList();
        if(possibleBest.Count > 0)
        {
            result = possibleBest.First();
        }
        else
        {
            foreach(GridBlock gB in neighbors)
            {
                if ((result = BestSpaceNextToTarget(gB, start)) != null)
                    break;
            }
        }

        return result;
    }

    private IEnumerable<GridBlock> MaxMovementPath(List<GridBlock> path, int maxMovement)
    {
        while(maxMovement > 0 && path.Count > 0)
        {
            var block = path.FirstOrDefault();
            maxMovement -= block.MovementCost;
            if (maxMovement >= 0 && path.Count > 0)
            {
                path.Remove(block);
                yield return block;
            }
        }
    }
}
