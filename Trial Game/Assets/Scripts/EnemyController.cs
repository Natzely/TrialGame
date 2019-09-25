using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float NewMoveDelay = 3f;
    
    PlayerManager _pM;
    UnitController _unitController;
    bool _unitReady;
    float _delayTimer;

    public void ReadyNextMove()
    {
        _unitReady = true;
        _delayTimer = NewMoveDelay;
    }

    // Start is called before the first frame update
    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        _unitController = gameObject.GetComponent<UnitController>();
        _unitController.EnemyController = this;
        _unitReady = true;
        _delayTimer = NewMoveDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(_unitReady)
        {
            _delayTimer -= Time.deltaTime;
            if(_delayTimer <= 0)
            {
                GetNextSpot();                
            }
        }
    }

    private void GetNextSpot()
    {
        UnitController target = null;
        var units = _pM.GetPlayerInfo(Enums.Player.Player1).Units;
        

        var closeUnits = units.Where(u => Vector2.Distance(_unitController.Position, u.Position) <= _unitController.AttackDistance);
        if(closeUnits.Count() > 0)
        {
            // Decide who to attack
            target = closeUnits.OrderBy(u => Vector2.Distance(_unitController.Position, u.Position)).FirstOrDefault();
        }
        else
            target = units.OrderBy(u => Vector2.Distance(_unitController.Position, u.Position)).FirstOrDefault();


        var path = _pM.CreatePath(_unitController.Player, _unitController.CurrentGridBlock, target.CurrentGridBlock).ToList();
        path.Pop();
        _unitController.MoveTo(path);
    }
}
