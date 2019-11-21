using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Enums.Player _player;
    PlayerManager _pM;
    UnitController _unitController;

    public IEnumerator NextAction()
    {
        UnitController target = null;
        var units = _pM.GetPlayerInfo(Enums.Player.Player1).Units;

        int attackRange = _unitController.MoveDistance + _unitController.AttackDistance;
        var closeUnits = units.Where(u => Vector2.Distance(_unitController.Position, u.Position) <= attackRange);
        if (closeUnits.Count() > 0)
        {
            // Decide who to attack
            target = closeUnits.OrderBy(u => Vector2.Distance(_unitController.Position, u.Position)).FirstOrDefault();
        }
        else
            target = units.OrderBy(u => Vector2.Distance(_unitController.Position, u.Position)).FirstOrDefault();

        var gbTarget = BestSpaceNextToTarget(target.CurrentGridBlock, _unitController.CurrentGridBlock);

        if (target != null && !_unitController.CheckAttack(target))
        {
            _unitController.Target = target;
            yield return new WaitUntil(() => MoveToNextSpace(gbTarget));
        }
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
            //_pM.CreateGrid(_player, _unitController.CurrentGridBlock, _unitController.MoveDistance + _unitController.CurrentGridBlock.MovementCost, _unitController.AttackDistance);
            //_pM.PrintPlayerGrid(_player);
            var path = _pM.CreatePath(_unitController.Player, _unitController.CurrentGridBlock, target).ToList();            
            path = MaxMovementPath(path, _unitController.MoveDistance + _unitController.CurrentGridBlock.MovementCost).ToList();
            _pM.ResetPathMatrix(_player);
            _unitController.MoveTo(path);
            return true;
        }
        
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
