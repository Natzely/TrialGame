using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpace : Space
{
    public Enums.PathDirection PathDirection
    {
        get; private set;
    }

    public override Enums.Player Player
    {
        get { return _player; }
        set
        {
            _player = value;
            switch (_player)
            {
                case Enums.Player.Player2:
                    _sR.color = Colors.Player2;
                    break;
                default:
                    _sR.color = Colors.Player1;
                    break;
            }
        }
    }

    public void MoveState(Vector2 cDir, Vector2? nDir, int pathOrder)
    {
        ResetRotation();
        PathDirection = cDir.ToPathDirection();
        Enums.PathDirection nextDir = (nDir ??  new Vector2(0,0)).ToPathDirection();
        _pathOrder = pathOrder;

        if (PathDirection == Enums.PathDirection.Start)
        {
            _animator.SetBool("Start", true);
            switch (nextDir)
            {
                case Enums.PathDirection.Down:
                    transform.Rotate(0, 0, -90);
                    break;
                case Enums.PathDirection.Up:
                    transform.Rotate(0, 0, 90);
                    break;
                case Enums.PathDirection.Left:
                    transform.Rotate(0, 0, 180);
                    break;
                default:
                    break;
            }
        }
        else    if (nDir == null)
        {
            _animator.SetBool("Head", true);
            _animator.SetBool("Straight", false);
            _animator.SetBool("Curve", false);
            switch (PathDirection)
            {
                case Enums.PathDirection.Up:
                    transform.Rotate(0, 0, 90);
                    break;
                case Enums.PathDirection.Down:
                    transform.Rotate(0, 0, -90);
                    break;
                case Enums.PathDirection.Left:
                    transform.Rotate(0, 0, 180);
                    break;
            }
        }
        else if (PathDirection == nextDir)
        {
            _animator.SetBool("Straight", true);
            _animator.SetBool("Head", false);
            _animator.SetBool("Curve", false);
            if (PathDirection == Enums.PathDirection.Up || nextDir == Enums.PathDirection.Down)
                transform.Rotate(0, 0, 90);
        }
        else
        {
            _animator.SetBool("Curve", true);
            _animator.SetBool("Head", false);
            _animator.SetBool("Straight", false);
            if ((PathDirection == Enums.PathDirection.Right && nextDir == Enums.PathDirection.Up) ||
               (PathDirection == Enums.PathDirection.Down && nextDir == Enums.PathDirection.Left))
            {
                _sR.flipY = true;
            }
            else if ((PathDirection == Enums.PathDirection.Up && nextDir == Enums.PathDirection.Right) ||
                    (PathDirection == Enums.PathDirection.Left && nextDir == Enums.PathDirection.Down))
            {
                _sR.flipX = true;
            }
            else if ((PathDirection == Enums.PathDirection.Left && nextDir == Enums.PathDirection.Up) ||
                    (PathDirection == Enums.PathDirection.Down && nextDir == Enums.PathDirection.Right))
            {
                _sR.flipX = true;
                _sR.flipY = true;
            }
        }
    }

    public void ResetRotation()
    {
        _sR.flipX = false;
        _sR.flipY = false;
        transform.rotation = Quaternion.identity;
    }

    public void Reset()
    {
        ResetRotation();
        _pathOrder = -1;
        PathDirection = Enums.PathDirection.Start;
        _animator.SetBool("Head", false);
        _animator.SetBool("Straight", false);
        _animator.SetBool("Curve", false);
        _animator.SetBool("Start", false);
    }
    
    private Animator _animator;
    private int _pathOrder;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _pathOrder = -1;
    }

    override protected void Update()
    {
        if (!_pM.GetPlayerInfo(Player).MovementPath.Contains(this))
        {
            Reset();
        }

        base.Update();
    }
}
