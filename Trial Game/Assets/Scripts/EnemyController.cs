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

        List<UnitController> closeUnits = units.Where(u => Vector2.Distance(u.Position, _unitController.Position) <= maxAttackRange).ToList();
        List<UnitController> tooCloseUnits = closeUnits.Where(u => Vector2.Distance(u.Position, _unitController.Position) <= minAttackRange).ToList();
        List<UnitController> targets = closeUnits.Except(tooCloseUnits).ToList();

        if (targets.Count() > 0)
        {
            // Decide who to attack
            target = targets.OrderBy(u => Vector2.Distance(_unitController.Position, u.Position)).FirstOrDefault();
        }
        else
        {
            // If all the close targets were too close, remove them from the pool
            targets = units.Except(closeUnits).ToList();
            target = targets.OrderBy(u => Vector2.Distance(_unitController.Position, u.Position)).FirstOrDefault();
        }

        if (target != null)
        {
            if (_unitController.CheckAttack(target))
                return true;
            
            var gbTarget = BestSpaceNextToTarget(target.CurrentGridBlock, _unitController.CurrentGridBlock);

            _unitController.Target = target;
            return MoveToNextSpace(gbTarget);
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

    private bool MoveToNextSpace(GridBlock target)
    {
        if (_unitController.CurrentGridBlock != null)
        {
            _pM.GetPlayerInfo(_player).ResetNonPlayerGrid = false;
            // Try to create a path to target unit. Path returns 0, then the unit is stuck and can't move, so don't put it on cooldown.
            var path = _pM.CreatePath(_unitController.Player, _unitController.CurrentGridBlock, target).ToList();
            if (path.Count > 1)
            {
                path = MaxMovementPath(path, _unitController.MoveDistance + _unitController.CurrentGridBlock.MovementCost).ToList();
                _pM.ResetPathMatrix(_player);
                _unitController.MoveTo(path);
                return true;
            }
            return false;
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
