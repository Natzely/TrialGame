using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpace : MonoBehaviour
{
    public Transform Holder;
    public Enums.PathDirection PathDirection
    {
        get { return _pDir; }
    }
    public Vector2 Position
    {
        get { return Holder.position; }
    }

    Animator _animator;
    SpriteRenderer _sR;
    Enums.PathDirection _pDir;

    public void MoveState(Enums.Player player, Enums.PathDirection direction, Enums.PathDirection nextDirection)
    {
        if (direction == Enums.PathDirection.Start)
            return;

        switch (player)
        {
            case Enums.Player.Player2:
                _sR.color = Colors.Player2;
                break;
            default:
                _sR.color = Colors.Player1;
                break;
        }

        _pDir = direction;
        if (Mathf.Abs(_pDir - nextDirection) > 1)
        {
            _animator.SetBool("Curve", true);
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
        _sR.enabled = true;
    }

    public void Destroy()
    {
        Destroy(transform.parent.gameObject);
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _sR.enabled = false;
    }
}
