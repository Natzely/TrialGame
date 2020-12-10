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
        var units = _pM.PlayerInfo.Units;//.Where(u => u.gameObject.name != "P1_Unit_Melee (1)");

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
            if (UnitController.CheckAttack(target.CurrentGridBlock))
                return true;

            GridBlock gbTarget = null;
            List<GridBlock> moves = null;
            var gbTargets = target.CurrentGridBlock.GetRangeSpaces(UnitController.CurrentGridBlock, minAttackRange);

            if (gbTargets != null)
            {
                int pathLength = 99;
                foreach (var gB in gbTargets)
                {
                    var tempMoves = UnitManager.CreatePath(UnitController.CurrentGridBlock, gB);
                    if (tempMoves.Count() < pathLength)
                    {
                        pathLength = tempMoves.Count();
                        moves = tempMoves.ToList();
                        gbTarget = gB;
                    }
                }
            }

            if (gbTarget != null)
            {
                UnitController.Target = target.CurrentGridBlock;
                return MoveToNextSpace(gbTarget, target, moves);
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

    private bool MoveToNextSpace(GridBlock gbTarget, UnitController ucTarget, List<GridBlock> moves)
    {
        if (UnitController.CurrentGridBlock != null)
        {
            // Try to create a path to target unit. Path returns 0, then the unit is stuck and can't move, so don't put it on cooldown.

            List<GridBlock> backupSpaces = null;
            double dis = 9999;
            if (ucTarget != null)
                dis = UnitController.Position.GridDistance(ucTarget.Position);
            UnitController.Target = ucTarget.CurrentGridBlock;


            if (dis <= UnitController.MaxAttackDistance && dis >= UnitController.MinAttackDistance)
            {
                UnitController.CheckAttack(gbTarget);
                return true;
            }
            else if (moves?.Count > 0 && dis > UnitController.MaxAttackDistance)
            {
                moves = VerifiedPath(moves, UnitController.MoveDistance + UnitController.CurrentGridBlock.MovementCost).ToList();
                UnitManager.ResetBlockGrid();
                bool action = false;
                if (moves.Count > 0 && UnitController.Target != null)
                {
                    if (moves.Last().Position.GridDistance(ucTarget.Position) < UnitController.MaxAttackDistance)
                    {
                        var lastGrid = moves.Last();
                        var newTargetGrid = lastGrid.Neighbors.OrderByDistance(UnitController.CurrentGridBlock, true).ToList().FirstOrDefault();

                        if (newTargetGrid != null && moves.Contains(newTargetGrid))
                        {
                            int index = moves.IndexOf(newTargetGrid);
                            moves.RemoveRange(index, moves.Count() - index - 1); // the -1 is to acount for index being 0 based and Count() being 1 based.
                        }
                        else if (newTargetGrid != null)
                        {
                            moves.Add(newTargetGrid);
                        }
                        else if (newTargetGrid == null)
                        {
                            UnitController.Target = null;
                            action = false;
                        }
                    }
                }

                if (moves.Count > 0)
                {
                    UnitController.MoveTo(moves);
                    action = true;
                }

                if (action)
                    return action;
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
        }

        Debug.Log("Unit Controller no GridBlock");
        return false;
    }

    private List<GridBlock> VerifiedPath(List<GridBlock> path, int maxMovement)
    {
        var maxPath = MaxMovementPath(path, maxMovement).ToList();

        while(maxPath.Last().isOccupied)
        {
            maxPath.Remove(maxPath.Last());
        }

        return maxPath;
    }

    private IEnumerable<GridBlock> MaxMovementPath(List<GridBlock> path, int maxMovement)
    {
        while(maxMovement > 0 && path.Count > 0)
        {
            var block = path.FirstOrDefault();
            maxMovement -= UnitController.Type == Enums.UnitType.Horse ? 1 : block.MovementCost;
            if (maxMovement >= 0 && path.Count > 0)
            {
                path.Remove(block);
                yield return block;
            }
        }
    }
}
