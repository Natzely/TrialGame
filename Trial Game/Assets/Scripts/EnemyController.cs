using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public UnitManager UnitManager;
    [HideInInspector] public UnitController UnitController;

    private PlayerManager _pM;

    public bool NextAction()
    {
        UnitController target = null;
        var units = _pM.PlayerInfo.Units;

        int maxAttackRange = UnitController.MoveDistance + UnitController.MaxAttackDistance;
        int minAttackRange = UnitController.MinAttackDistance;

        List<UnitController> inRangeUnits = units.Where(u => u.Position.GridDistance(UnitController.Position) == UnitController.MaxAttackDistance).ToList();
        units = units.Except(inRangeUnits).ToList();
        List<UnitController> tooCloseUnits = units.Except(inRangeUnits).Where(u => u.Position.GridDistance(UnitController.Position) <= minAttackRange).ToList();
        units = units.Except(tooCloseUnits).ToList();
        List<UnitController> closeUnits = units.Except(inRangeUnits.Union(tooCloseUnits)).Where(u => u.Position.GridDistance(UnitController.Position) <= maxAttackRange).ToList();
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
            target = closeUnits.OrderBy(u => u.Position.GridDistance(UnitController.Position)).FirstOrDefault();
        }
        else if(targets.Count > 0)
        {
            target = targets.OrderBy(u => u.Position.GridDistance(UnitController.Position)).FirstOrDefault();
        }

        if (target != null)
        {
            if (UnitController.CheckAttack(target))
                return true;

            var gbTarget = target.CurrentGridBlock.GetRangedSpaces(UnitController.CurrentGridBlock, minAttackRange);

            if (gbTarget != null)
            {
                UnitController.Target = target;
                return MoveToNextSpace(gbTarget, target);
            }
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        UnitController = gameObject.GetComponent<UnitController>();
        UnitController.EnemyController = this;
    }

    private bool MoveToNextSpace(GridBlock gbTarget, UnitController ucTarget)
    {
        if (UnitController.CurrentGridBlock != null)
        {
            // Try to create a path to target unit. Path returns 0, then the unit is stuck and can't move, so don't put it on cooldown.
            var _moves = UnitManager.CreatePath(UnitController.CurrentGridBlock, gbTarget).ToList();

            List<GridBlock> backupSpaces = null;
            double dis = 9999;
            if (ucTarget != null)
                dis = UnitController.Position.GridDistance(ucTarget.Position);
            UnitController.Target = ucTarget;


            if (dis <= UnitController.MaxAttackDistance && dis >= UnitController.MinAttackDistance)
            {
                UnitController.CheckAttack(gbTarget.CurrentUnit);
                return true;
            }
            else if (_moves?.Count > 0 && dis > UnitController.MaxAttackDistance)
            {
                _moves = MaxMovementPath(_moves, UnitController.MoveDistance + UnitController.CurrentGridBlock.MovementCost).ToList();
                UnitManager.ResetBlockGrid();
                bool action = false;
                if (UnitController.Target != null)
                {
                    if (_moves.Last().Position.GridDistance(ucTarget.Position) < UnitController.MaxAttackDistance)
                    {
                        var lastGrid = _moves.Last();
                        var newTargetGrid = lastGrid.Neighbors.OrderByDistance(UnitController.CurrentGridBlock, true).ToList().FirstOrDefault();

                        if (newTargetGrid != null && _moves.Contains(newTargetGrid))
                        {
                            int index = _moves.IndexOf(newTargetGrid);
                            _moves.RemoveRange(index, _moves.Count() - index - 1); // the -1 is to acount for index being 0 based and Count() being 1 based.
                        }
                        else if (newTargetGrid != null)
                        {
                            _moves.Add(newTargetGrid);
                        }
                        else if (newTargetGrid == null)
                        {
                            UnitController.Target = null;
                            action = false;
                        }
                    }
                }
                if (_moves.Count > 0)
                {
                    UnitController.MoveTo(_moves);
                    action = true;
                }

                if (action)
                    return true;
            }
            else if ((backupSpaces = UnitController.CurrentGridBlock.AvailableAttackSpace(gbTarget, UnitController.MaxAttackDistance).ToList()).Count > 0)
            {
                UnitController.MoveTo(new List<GridBlock>() { UnitController.CurrentGridBlock, backupSpaces.First() });
                return true;
            }
            else
            {
                UnitController.Target = null;
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
