using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public float MoveTimer = 1;
    public float ActionTimer = .1f;

    Enums.CursorState _currState;
    Enums.CursorState _lastState;
    UnitController _currUnit;
    SpriteRenderer _sR;
    List<Vector2> _moves;
    Vector2 _startPos;
    Color _playerColor;
    Color _errorColor;
    bool _noGo;
    bool _select;
    bool _attack;
    bool _cancel;
    float _moveTimer;
    float _horzClamp;
    float _vertClamp;
    float _actionTimer;
    int _vert;
    int _horz;
    string _playerPrefix;

    // Start is called before the first frame update
    void Start()
    {
        _currState = Enums.CursorState.Default;
        _moves = new List<Vector2>();
        _sR = GetComponent<SpriteRenderer>();
        _noGo = false;
        _moveTimer = 0;
        _horzClamp = (int)Boundaries.bounds.extents.x;
        _vertClamp = (int)Boundaries.bounds.extents.y;
        _actionTimer = 0;
        _startPos = transform.position;
        _playerPrefix = Player == Enums.Player.Player1 ? "P1" : "P2";
        _errorColor = Colors.Error;

        switch (Player)
        {
            case Enums.Player.Player2:
                _playerColor = _sR.color = Colors.Player2;
                break;
            default:
                _playerColor = _sR.color = Colors.Player1;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _vert = Mathf.RoundToInt(Input.GetAxis(_playerPrefix + "_Vertical"));
        _horz = Mathf.RoundToInt(Input.GetAxis(_playerPrefix + "_Horizontal"));
        _select = Input.GetButtonUp(_playerPrefix + "_Select") ;
        _cancel = Input.GetButtonUp(_playerPrefix + "_Cancel");
        _attack = Input.GetButtonUp(_playerPrefix + "_Attack");

        CheckForAction();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        if ((_currState == Enums.CursorState.Moving && gO.layer == LayerMask.NameToLayer("Map")) ||
             (gO.tag == "Unit" && gO.layer != (int)Player))
        {
            _sR.color = _errorColor;
            _noGo = true;
            if (_moves.Count > 0)
                _moves.Pop();
        }
        else if (_currState == Enums.CursorState.Default)
        {
            UnitController tmpUnit = gO.GetComponent<UnitController>();
            if (tmpUnit != null && tmpUnit.Player == Player)
            {
                _currUnit = tmpUnit;
                _currUnit.Hover(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        Debug.Log($"Player: {Player} | Unit: {gO.layer}");
        if ((_currState == Enums.CursorState.Moving && gO.layer == LayerMask.NameToLayer("Map")) || 
            (gO.tag == "Unit" && gO.layer != (int)Player))
        {
            _sR.color = _playerColor;
            _noGo = false;
        }
        else if(_currUnit != null)
        {
            _currUnit.Hover(false);
            if (_currState == Enums.CursorState.Default)
                _currUnit = null;
        }
    }

    private void CheckForAction()
    {
        if (_actionTimer <= 0)
        {
            if (_cancel)
            {
                Debug.Log("Cancel");
                Cancel();
            }
            else if (_attack)
            {
                Debug.Log("Attack");
                Attack();
            }
            else if (_select && _currUnit != null)
            {
                Debug.Log("Select");
                Select();
            }
            else if (_moveTimer <= 0 && (_horz != 0 || _vert != 0) && (_currUnit != null ? !_currUnit.Moving && !_currUnit.Moved : true))
            {
                Move();
            }
        }

        _actionTimer -= Time.deltaTime;
        _moveTimer -= Time.deltaTime;
    }

    private void Cancel()
    {
        switch(_currState)
        {
            case Enums.CursorState.Attacking:
                if (transform.position != _currUnit.GetHolder().position)
                    transform.position = _currUnit.GetHolder().position;
                _currState = _lastState;
                break;
            case Enums.CursorState.Moving:
                if(_currUnit.Moved || _currUnit.Moving)
                    _currUnit.CancelMove();
                if (transform.position.V2() == _startPos)
                {
                    Debug.Log("Unselect");
                    _currState = Enums.CursorState.Default;
                    _currUnit.Select(false);
                }
                else
                {
                    Debug.Log("Move back to start");
                    transform.position = _startPos;
                    _moves.Clear();
                }
                break;
            default:
                break;
        }
        _actionTimer = ActionTimer;
    }

    private void Attack()
    {
        if (_currState != Enums.CursorState.Attacking)
        {
            _currState = Enums.CursorState.Attacking;
        }
        else if (transform.position != _currUnit.GetHolder().position)
        {
            _currUnit.Attack(transform.position);
            _currUnit.Select(false);
            _currState = Enums.CursorState.Default;
        }

        _actionTimer = ActionTimer;
    }

    private void Select()
    {
        if(_currState != Enums.CursorState.Moving)
        {
            _currState = Enums.CursorState.Moving;
            if (transform.position == _currUnit.GetHolder().position)
            {
                if (!_currUnit.Moved)
                {
                    _startPos = transform.position;
                    _moves.Clear();
                    LinkAndAddMove(_startPos);
                }
                _currUnit.Select(true);
            }
        }
        else if (!_noGo && !_currUnit.Moving && !_currUnit.Moved)
        {
            _currUnit.MoveTo(_moves);
        }
        else if(_currUnit.Moving || _currUnit.Moved)
        {
            Debug.Log("Deselect");  
            _currUnit.Select(false);
            _currUnit = null;
            _currState = Enums.CursorState.Default;
        }

        _actionTimer = ActionTimer;
    }

    private void Move()
    {
        Vector2 tmpPos = transform.position;

        tmpPos.x = Mathf.Clamp(tmpPos.x + _horz, -_horzClamp, _horzClamp);
        tmpPos.y = Mathf.Clamp(tmpPos.y + _vert, -_vertClamp, _vertClamp);

        float distance = Vector2.Distance(tmpPos, _startPos);
        if (_currState == Enums.CursorState.Default)
        {
            transform.position = tmpPos;
        } 
        if (_currState == Enums.CursorState.Moving && distance <= _currUnit.TotalMoves)
        {
            transform.position = tmpPos;
            if(!_noGo)
            {
                if (_moves.Contains(tmpPos))
                    while ((Vector2)_moves.Pop() != tmpPos) { }

                LinkAndAddMove(tmpPos);
            }
        }

        _moveTimer = MoveTimer;
        _actionTimer = ActionTimer;
    }

    private void LinkAndAddMove(Vector2 move)
    {
        _moves.Add(move);
    }
}
