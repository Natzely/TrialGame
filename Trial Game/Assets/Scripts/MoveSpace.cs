using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpace : MonoBehaviour
{
    public static Enums.PathDirection VectorToDirection(Vector2 vector)
    {
        if (vector == Vector2.down)
            return Enums.PathDirection.Down;
        else if (vector == Vector2.up)
            return Enums.PathDirection.Up;
        else if (vector == Vector2.left)
            return Enums.PathDirection.Left;
        else if (vector == Vector2.right)
            return Enums.PathDirection.Right;
        else
            return Enums.PathDirection.Start;
    }

    public Enums.PathDirection PathDirection
    {
        get; private set;
    }
    public Vector2 Position
    {
        get { return new Vector2(transform.position.x - .5f, transform.position.y - .5f); }
    }
    public bool Show
    {
        get { return _sR.enabled; }
        set
        {
            if (PathDirection != Enums.PathDirection.Start)
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

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public Vector2 GridPosition
    {
        get
        {
            return _gridPos;
        }
        set
        {
            _gridPos = value;
            _pM.UpdatePathMatrix(Player, value, this);
        }
    }

    private PlayerManager _pM;
    private Animator _animator;
    private SpriteRenderer _sR;
    private Enums.Player _player;
    private Vector2 _gridPos;
    private int _pathOrder;

    //public void MoveState(Vector2 vDir, int pathOrder)
    //{
    //    Enums.PathDirection dir = VectorToDirection(vDir);

    //    _pathOrder = pathOrder;
    //    PathDirection = dir;

    //    ResetRotation();

    //    _animator.SetBool("Curve", false);
    //    _animator.SetBool("Straight", false);

    //    if (dir == Enums.PathDirection.Start)
    //        _animator.SetBool("Start", true);
    //    else
    //    {
    //        _animator.SetBool("Head", true);
    //        switch (dir)
    //        {
    //            case Enums.PathDirection.Up:
    //                transform.Rotate(0,0,90);
    //                break;
    //            case Enums.PathDirection.Down:
    //                transform.Rotate(0, 0, -90);
    //                break;
    //            case Enums.PathDirection.Left:
    //                transform.Rotate(0, 0, 180);
    //                break;
    //        }
    //    }
    //}

    public void MoveState(Vector2 cDir, Vector2? nDir, int pathOrder)
    {
        ResetRotation();
        PathDirection = VectorToDirection(cDir);
        Enums.PathDirection nextDir = VectorToDirection(nDir ??  new Vector2(0,0));
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

    public void Destroy()
    {
        Destroy(transform.parent.gameObject);
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
        _pathOrder = -1;
    }

    void Update()
    {
        if (_pM.GetDeleteMoveSpace(Player))
            gameObject.SetActive(false);

        else if (!_pM.GetPlayerInfo(Player).MovementPath.Contains(this))
        {
            Reset();
        }
    }
}
