using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpace : MonoBehaviour
{
    public Transform Holder;
    public Sprite Curve;
    public Enums.PathDirection PathDirection
    {
        get { return _pDir; }
    }
    public Vector2 Position
    {
        get { return transform.position; }
    }
    public bool Show
    {
        get { return _sR.enabled; }
        set
        {
            if (_pDir != Enums.PathDirection.Start)
                _sR.enabled = value;
        }
    }
    public Enums.Player Player
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

    public int PathOrder
    {
        get { return _pathOrder; }
    }

    Animator _animator;
    SpriteRenderer _sR;
    CursorController _parent;
    Enums.Player _player;
    Enums.PathDirection _pDir;
    int _pathOrder;

    public void MoveState(Enums.PathDirection direction, Enums.PathDirection nextDirection,
        CursorController parent, int pathOrder)
    {
        _pDir = direction;
        _parent = parent;
        _pathOrder = pathOrder;

        if (direction == Enums.PathDirection.Start || nextDirection == Enums.PathDirection.End)
        {
            //_enable = false;
            return;
        }

        if (Mathf.Abs(_pDir - nextDirection) > 1)
        {
            _sR.sprite = Curve;//    _animator.SetBool("Curve", true);
            if ((_pDir == Enums.PathDirection.Right && nextDirection == Enums.PathDirection.Up) ||
               (_pDir == Enums.PathDirection.Down && nextDirection == Enums.PathDirection.Left))
            {
                _sR.flipY = true;
            }
            else if ((_pDir == Enums.PathDirection.Up && nextDirection == Enums.PathDirection.Right) ||
                    (_pDir == Enums.PathDirection.Left && nextDirection == Enums.PathDirection.Down))
            {
                _sR.flipX = true;
            }
            else if ((_pDir == Enums.PathDirection.Left && nextDirection == Enums.PathDirection.Up) ||
                    (_pDir == Enums.PathDirection.Down && nextDirection == Enums.PathDirection.Right))
            {
                _sR.flipX = true;
                _sR.flipY = true;
            }
        }
        else
        {
            if (_pDir == Enums.PathDirection.Up || _pDir == Enums.PathDirection.Down)
                transform.Rotate(0, 0, 90);
        }
    }

    public void Destroy()
    {
        Destroy(transform.parent.gameObject);
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //if (_parent.CurrentMove <= _pathOrder)
        //    Destroy(transform.parent.gameObject);
    }
}
