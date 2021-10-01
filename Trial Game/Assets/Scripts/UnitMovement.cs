using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitMovement : MonoBehaviour
{
    public UnitController MainController;
    public float Speed;

    
    private Enums.UnitState _unitState
    {
        get { return MainController.UnitState; }
        set { MainController.UnitState = value; }
    }
    private Vector2 Position
    {
        get { return MainController.Position; }
        set { MainController.Position = value; }
    }
    private float GridblockSpeedModifyer { get { return MainController.GridblockSpeedModifier; } }

    private Queue<MovePoint> _movePositions;
    private Stack<MovePoint> _prevPositions;
    private MovePoint _nextPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (!MainController.OnCooldown)
        //{
        //    if ((_nextPoint != null || _movePositions.Count > 0) && _unitState != Enums.UnitState.Attacking && _unitState != Enums.UnitState.Hurt)
        //    {
        //        if (_nextPoint == null)
        //            GetNextPoint();

        //        Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Position, Speed * GridblockSpeedModifyer * Time.deltaTime);
        //        MainController.UpdateMinimapIcon();

        //        transform.position = moveVector;
        //        if (transform.position.V2() == _nextPoint.Position)
        //        {
        //            var fod = _prevPositions.FirstOrDefault();
        //            fod?.Path_Delete(MainController);
        //            _prevPositions.Push(_nextPoint);

        //            if (CurrentGridBlock.CurrentUnit == null && _attackWhenInRange && AttackTargetDistance() <= MaxAttackDistance && CurrentGridBlock && CurrentGridBlock.CurrentUnit == this)
        //            {
        //                DeleteSavedPath();
        //                _nextPoint = null;
        //            }
        //            else if (!_movePositions.IsEmpty())
        //            {
        //                GetNextPoint();
        //            }
        //            else
        //            {
        //                if (CurrentGridBlock != _nextPoint.GridBlock) // Arrived at the last point, make sure it's the currentgridblock
        //                    CurrentGridBlock = _nextPoint.GridBlock;
        //                Log("Last move");
        //                _nextPoint = null;
        //            }

        //            if (_movePositions.IsEmpty() && _nextPoint == null)
        //            {
        //                if (CurrentGridBlock.CurrentUnit == null)
        //                {
        //                    Log($"Ends move");
        //                    Moving = false;
        //                    Moved = true;

        //                    CurrentGridBlock.SetCurrentUnit(this);
        //                    _unitState = Enums.UnitState.Idle;
        //                }
        //                else
        //                {
        //                    FindGoodPreviousPoint();
        //                }
        //            }
        //        }
        //    }

        //    if (!_selected && !Moving && _animator.GetBool("Moving"))
        //        _animator.SetBool("Moving", false);

        //    if (_nextPoint == null && _unitState == Enums.UnitState.Idle && _tasked && !Moving && !_attack && (Moved || Attacked))
        //    {
        //        Log($"goes on cooldown");
        //        if (CurrentGridBlock.CurrentUnit && CurrentGridBlock.CurrentUnit != this)
        //        {
        //            FindGoodPreviousPoint();
        //        }
        //        else
        //        {
        //            MainController.GoOnCooldown();
        //        }
        //    }
        //}
    }
    private void GetNextPoint()
    {
        var currentPoint = _nextPoint;
        var possiblePoint = _movePositions.Peek();
        if (possiblePoint == null || possiblePoint.Position.GridDistance(MainController.Position) > 1) // Make sure the next point is one point away, this should trigger VERY rarely
        {
            var orderedNeighbors = MainController.CurrentGridBlock.Neighbors.OrderByDistance(possiblePoint.GridBlock);
            foreach (GridBlock gB in orderedNeighbors)
            {
                if (gB.Position.GridDistance(possiblePoint.Position) <= 1)
                {
                    possiblePoint = gB.ToMovePoint();
                    break;
                }
            }
        }
        else
            _movePositions.Dequeue(); // point is fine go ahead and remove it from queue

        if (currentPoint != null && _movePositions.IsEmpty() && possiblePoint.CurrentUnit != null && !possiblePoint.CurrentUnit.IsEnemy(MainController.Player)) // if this is the last point in the queue make sure it's not empty
        {
            if (currentPoint.CurrentUnit == this) // Last spot is taken and current spot is safe to stay at
            {
                _nextPoint = null;
                return;
            }
            else // current spot isn't safe so find a new spot
            {
                FindGoodPreviousPoint();
                GetNextPoint();
                return;
            }
        }

        //_nextPoint = possiblePoint;
        //Moving = true;
        //_miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Moving : Colors.Enemy_Moving;
        //UnitState = Enums.UnitState.Moving;
        //BoxCollider.size = ColliderSizeMoving;
        //_animator.SetBool("Moving", true);
        //IsHidden = false;
        //LookAt(possiblePoint.Position);
    }

    private void FindGoodPreviousPoint()
    {
        _movePositions.Clear();

        foreach (var pos in _prevPositions)
        {
            _movePositions.Enqueue(pos);
            if (pos.CurrentUnit == null || pos.CurrentUnit == this)
                break;
        }

        _prevPositions.Clear();
        if (_nextPoint != null)
        {
            _prevPositions.Push(_nextPoint);
            _nextPoint = null;
        }
    }
}
