using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Unit_Movement : MonoBehaviour
{
    public float Speed;
    public bool Moved;
    public bool Moving;

    private Enums.UnitState _unitState
    {
        get { return _unitController.UnitState; }
        set { _unitController.UnitState = value; }
    }

    private Vector2 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private GridBlock CurrentGridBlock
    {
        get { return _unitController.CurrentGridBlock; }
        set { _unitController.CurrentGridBlock = value; }
    }
    private float GridblockSpeedModifyer { get { return _unitController.GridblockSpeedModifier; } }

    private UnitController _unitController;
    private Queue<MovePoint> _movePositions;
    private Stack<MovePoint> _prevPositions;
    private Stack<MovePoint> _backupPos;
    private MovePoint _nextPoint;

    private int _triedFindingPlace;


    public void MoveTo(List<MovePoint> movePoints)
    {
        if (_unitState == Enums.UnitState.Idle && !Moved && movePoints.Count > 0)
        {

            for (int x = 0; x < movePoints.Count; x++)
            {
                var point = movePoints[x];
                _movePositions.Enqueue(point);
            }

            CurrentGridBlock.ResetCurrentUnit(_unitController);
            GetNextPoint();
        }
    }

    private void Awake()
    {
        _unitController = GetComponent<UnitController>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_unitController.UnitState == Enums.UnitState.Moving)
        {
            if ((_nextPoint != null || _movePositions.Count > 0) && _unitState != Enums.UnitState.Attacking && _unitState != Enums.UnitState.Hurt)
            {
                if (_nextPoint == null)
                    GetNextPoint();

                Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Position, Speed * GridblockSpeedModifyer * Time.deltaTime);
                _unitController.UpdateMinimapIcon();

                transform.position = moveVector;
                if (transform.position.V2() == _nextPoint.Position)
                {
                    var fod = _prevPositions.FirstOrDefault();
                    fod?.Path_Delete(_unitController);
                    _prevPositions.Push(_nextPoint);

                    if (!_movePositions.IsEmpty())
                    {
                        GetNextPoint();
                    }
                    else
                    {
                        if (CurrentGridBlock != _nextPoint.GridBlock) // Arrived at the last point, make sure it's the currentgridblock
                            CurrentGridBlock = _nextPoint.GridBlock;
                        _nextPoint = null;
                    }

                    if (_movePositions.IsEmpty() && _nextPoint == null)
                    {
                        if (CurrentGridBlock.CurrentUnit == null)
                        {
                            Moving = false;
                            Moved = true;

                            CurrentGridBlock.SetCurrentUnit(_unitController);
                            _unitState = Enums.UnitState.Idle;
                        }
                        else
                        {
                            FindGoodPreviousPoint();
                        }
                    }
                }
            }

            //if (!_selected && !Moving && _animator.GetBool("Moving"))
            //    _animator.SetBool("Moving", false);

            //if (_nextPoint == null && _unitState == Enums.UnitState.Idle && _tasked && !Moving && !_attack && (Moved || Attacked))
            //{
            //    if (CurrentGridBlock.CurrentUnit && CurrentGridBlock.CurrentUnit != this)
            //    {
            //        FindGoodPreviousPoint();
            //    }
            //    else
            //    {
            //        _unitController.GoOnCooldown();
            //    }
            //}
        }
    }
    private void GetNextPoint()
    {
        var currentPoint = _nextPoint;
        var possiblePoint = _movePositions.Peek();
        if (possiblePoint == null || possiblePoint.Position.GridDistance(_unitController.Position) > 1) // Make sure the next point is one point away, this should trigger VERY rarely
        {
            var orderedNeighbors = _unitController.CurrentGridBlock.Neighbors.OrderByDistance(possiblePoint.GridBlock);
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

        if (currentPoint != null && _movePositions.IsEmpty() && possiblePoint.CurrentUnit != null && !possiblePoint.CurrentUnit.IsEnemy(_unitController.Player)) // if this is the last point in the queue make sure it's not empty
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
    }

    private void FindGoodPreviousPoint()
    {
        _movePositions.Clear();
        _triedFindingPlace++;

        if (_triedFindingPlace > 3)
            _prevPositions = _backupPos;

        if (_backupPos == null)
            _backupPos = new Stack<MovePoint>(_prevPositions);

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
