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

        if (target != null && !_unitController.CheckAttack(target))
            yield return new WaitUntil(() => MoveToNextSpace(target));
    }

    // Start is called before the first frame update
    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        _unitController = gameObject.GetComponent<UnitController>();
        _player = _unitController.Player;
        _unitController.EnemyController = this;
    }

    private bool MoveToNextSpace(UnitController target)
    {
        _pM.GetPlayerInfo(_player).ResetNonPlayerGrid = false;
        _pM.CreateGrid(_player, _unitController.CurrentGridBlock, _unitController.MoveDistance + _unitController.CurrentGridBlock.MovementCost, _unitController.AttackDistance);
        //_pM.PrintPlayerGrid(_player);
        var path = _pM.CreatePath(_unitController.Player, _unitController.CurrentGridBlock, target.CurrentGridBlock).ToList();
        _pM.ResetPathMatrix(_player);
        _unitController.MoveTo(path);
        return true;
    }
}
