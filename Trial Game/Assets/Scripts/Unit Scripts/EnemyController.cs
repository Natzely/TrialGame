using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public UnitManager UnitManager { get; set; }
    public UnitController UnitController { get; set; }

    private PlayerManager _pM;
    public bool NextAction()
    {

        UnitController target = null;
        var units = _pM.PlayerInfo.Units;//.Where(u => u.gameObject.name != "P1_Unit_Melee (1)");

        int maxAttackRange = UnitController.MoveDistance + UnitController.MaxAttackDistance;
        int minAttackRange = UnitController.MinAttackDistance;

        // Grab units that are within attack distance and order them by distance
        List<UnitController> inRangeUnits = units.Where(u => u.Position.GridDistance(UnitController.Position) <= UnitController.MaxAttackDistance).ToList();
        inRangeUnits = inRangeUnits.OrderBy(u => u.Position.GridDistance(UnitController.Position)).ToList();
        units = units.Except(inRangeUnits).ToList();
        
        // Seperate the ones that are too close to attack from the previous list
        List<UnitController> tooCloseUnits = inRangeUnits.Where(u => u.Position.GridDistance(UnitController.Position) > minAttackRange).ToList();
        inRangeUnits = inRangeUnits.Except(tooCloseUnits).ToList();
        
        // Grab the units that are outside of the immediate attack distance, but can be moved to. order by distance
        List<UnitController> reachableUnits = units.Where(u => u.Position.GridDistance(UnitController.Position) <= maxAttackRange).ToList();
        reachableUnits = reachableUnits.OrderBy(u => u.Position.GridDistance(UnitController.Position)).ToList();
        units = units.Except(reachableUnits).ToList();
        
        // The remaining units can't be attacked this turn. Order the list by distance anyway
        List<UnitController> unreachableUnits = units.OrderBy(u => u.Position.GridDistance(UnitController.Position)).ToList();


        try
        {
            if (inRangeUnits.Count > 0) // Check for units that can be attacked first
                target = CheckForAttackableUnit(inRangeUnits, false);
            if (!target && tooCloseUnits.Count > 0) // Then the ones that are too close
                target = CheckForAttackableUnit(tooCloseUnits, false);
            if (!target && reachableUnits.Count > 0) // Then the ones withing walking distance
                target = CheckForAttackableUnit(reachableUnits, true);
            if (!target && unreachableUnits.Count > 0) // If none of the ones above worked
            {
                target = unreachableUnits.FirstOrDefault(); // Grab the first of the really far ones
                var tmpPath = UnitManager.CreatePath(UnitController.CurrentGridBlock, target.CurrentGridBlock).ToList();
                tmpPath = VerifiedPath(tmpPath, UnitController.MoveDistance);
                UnitController.MoveTo(tmpPath);
            }
        }
        catch(Exception e)
        {
            Debug.Log("");
        }

        return target;

        // If the unit has to move can't attack this move, select the closest unit to start walking to
        //if (!target && unreachableUnits.Count > 0)
        //{
        //    target = unreachableUnits.OrderBy(u => u.Position.GridDistance(UnitController.Position)).FirstOrDefault();
        //}

        //// If a unit was found, get the path started
        //if (target != null)
        //{
        //    if (UnitController.CheckAttack(target.CurrentGridBlock)) // Check again if the unit can be attacked from the units current position
        //        return true;

        //    // Otherwise find a position to do so.
        //    GridBlock gbTarget = null;
        //    List<MovePoint> moves = null;
        //    var gbTargets = target.CurrentGridBlock.GetRangeSpaces(UnitController.CurrentGridBlock, minAttackRange).ToList();

        //    if (gbTargets != null)
        //    {
        //        int pathLength = 99;
        //        foreach (var gB in gbTargets)
        //        {
        //            var tempMoves = UnitManager.CreatePath(UnitController.CurrentGridBlock, gB);
        //            if (tempMoves != null && tempMoves.Count() < pathLength)
        //            {
        //                pathLength = tempMoves.Count();
        //                moves = tempMoves.ToList();
        //                gbTarget = gB;
        //            }
        //        }
        //    }

        //    if (gbTarget != null)
        //    {
        //        UnitController.Target = target.CurrentGridBlock.ToMovePoint();
        //        return MoveToNextSpace(gbTarget, target, moves);
        //    }
        //}

        //return false;
    }

    // Check if the unit can attack any of the following targets
    private UnitController CheckForAttackableUnit(List<UnitController> targets, bool farUnit)
    {
        //var target = targets.FirstOrDefault(u => UnitController.CheckAttack(u.CurrentGridBlock, true));
        UnitController target = null;
        
        foreach (UnitController unit in targets)
        {
            // First check if the unit can attack the targete from it's current position
            target = unit;
            GridBlock targetGB = target.CurrentGridBlock;
            if (UnitController.CheckAttack(targetGB, true))
            {
                UnitController.CheckAttack(targetGB); // if it can, set the target 
                break; //and stop looking
            }
            List<MovePoint> tempMoves = null;
            List<MovePoint> backupMoves = null;
            // Else, check if it can move into a position where it can attack the target from.
            // Create a path and check all the spaces for a possible attack point
            tempMoves = UnitManager.CreatePath(UnitController.CurrentGridBlock, targetGB).ToList();
            tempMoves = VerifiedPath(tempMoves, UnitController.MoveDistance).ToList();
            backupMoves = new List<MovePoint>(tempMoves);
            var attackFromPoint = tempMoves.Last();
            while (!UnitController.CheckAttack(targetGB, true, attackFromPoint.GridBlock))
            {
                tempMoves.Remove(attackFromPoint); // The unit couldn't attack from that last spot so remove it
                if (tempMoves.Count > 0) // if there are still some left
                    attackFromPoint = tempMoves.Last(); // look at the next one.
                else
                    break;
            }

            // If there were left over moves then the unit was able to find a position to attack from
            if (tempMoves.Count > 0)
            {
                UnitController.CheckAttack(targetGB, !farUnit); // Set target
                UnitController.MoveTo(tempMoves, true); // And set movement path for the unit
                break; // Stop looking
            } // Something that can happen that a "reachable" target is actually past a wall
            else if(backupMoves.Count > 1) // the unit must move pretty far to reach them.
            {// so if the unit found a path (path > 1, a.k.a not stuck) then let the unit move
                UnitController.MoveTo(backupMoves);
                break; // stop looking
            }

            target = null; // If the target didnt work, then go to the next one.
        }

        return target;
    }

    // Start is called before the first frame update
    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        UnitController = gameObject.GetComponent<UnitController>();
        UnitController.EnemyController = this;
    }

    //private bool MoveToNextSpace(GridBlock gbTarget, UnitController ucTarget, List<MovePoint> moves)
    //{
    //    if (UnitController.CurrentGridBlock != null)
    //    {
    //        // Try to create a path to target unit. Path returns 0, then the unit is stuck and can't move, so don't put it on cooldown.

    //        List<GridBlock> backupSpaces;
    //        double dis = 9999;
    //        if (ucTarget)
    //            dis = UnitController.Position.GridDistance(ucTarget.Position);
    //        UnitController.Target = ucTarget.CurrentGridBlock.ToMovePoint();


    //        //if (dis <= UnitController.MaxAttackDistance && dis >= UnitController.MinAttackDistance)
    //        //{
    //        //    UnitController.CheckAttack(gbTarget);
    //        //    return true;
    //        //}
    //        //else
    //        if (moves?.Count > 0 && dis > UnitController.MaxAttackDistance)
    //        {
    //            var prevMoveCount = moves.Count;
    //            moves = VerifiedPath(moves, UnitController.MoveDistance + UnitController.CurentGridMoveCost).ToList();
    //            if (moves.Count == 0 && prevMoveCount > 0) // The unit doens't have enought movement to get out of it's spot, so move to the next unit
    //                return true;

    //            UnitManager.ResetBlockGrid();
    //            bool action = false;
    //            if (moves.Count > 0 && UnitController.Target != null)
    //            {
    //                if (moves.Last().Position.GridDistance(ucTarget.Position) < UnitController.MaxAttackDistance)
    //                {
    //                    var lastMovePoint = moves.Last();
    //                    var lastGrid = lastMovePoint.GridBlock;
    //                    var newTargetGrid = lastGrid.Neighbors.OrderByDistance(UnitController.CurrentGridBlock, true).ToList().FirstOrDefault();
    //                    var newTargetMovePoint = newTargetGrid.ToMovePoint();

    //                    if (newTargetGrid != null && moves.Contains(newTargetMovePoint))
    //                    {
    //                        int index = moves.IndexOf(newTargetMovePoint);
    //                        moves.RemoveRange(index, moves.Count() - index - 1); // the -1 is to acount for index being 0 based and Count() being 1 based.
    //                    }
    //                    else if (newTargetGrid != null)
    //                    {
    //                        moves.Add(newTargetMovePoint);
    //                    }
    //                    else if (newTargetGrid == null)
    //                    {
    //                        UnitController.Target = null;
    //                        action = false;
    //                    }
    //                }
    //            }

    //            if (moves.Count > 0)
    //            {
    //                UnitController.MoveTo(moves, true);
    //                action = true;
    //            }

    //            if (action)
    //                return action;
    //        }
    //        else if ((backupSpaces = UnitController.CurrentGridBlock.AvailableAttackSpace(gbTarget, UnitController.MaxAttackDistance).ToList()).Count > 0)
    //        {
    //            UnitController.MoveTo(new List<MovePoint>() { UnitController.CurrentGridBlock.ToMovePoint(), backupSpaces.First().ToMovePoint() });
    //            return true;
    //        }
    //        else
    //        {
    //            UnitController.Target = null;
    //        }
    //    }

    //    Debug.Log("Unit Controller no GridBlock");
    //    return false;
    //}

    private List<MovePoint> VerifiedPath(List<MovePoint> path, int maxMovement)
    {
        var maxPath = MaxMovementPath(path, maxMovement+1).ToList(); // We have to retrieve [maxMovement + 1] moves because their current location counts as a move
                                                                     // We need to save their current location in case they need return to it.

        while (maxPath.Count > 0 && maxPath.Last().IsOccupied && maxPath.Last().CurrentUnit != UnitController)
        {
            maxPath.Remove(maxPath.Last());
        }

        return maxPath;

    }

    private IEnumerable<MovePoint> MaxMovementPath(List<MovePoint> path, int maxMovement)
    {
        while(maxMovement > 0 && path.Count > 0)
        {
            var block = path.FirstOrDefault();
            maxMovement -= UnitController.Type == Enums.UnitType.Horse ? 1 : UnitController.CurentGridMoveCost;
            if (maxMovement >= 0 && path.Count > 0)
            {
                path.Remove(block);
                yield return block;
            }
        }
    }
}
