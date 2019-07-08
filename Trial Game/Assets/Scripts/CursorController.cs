using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CursorController : MonoBehaviour
{
    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public GameObject MovePath;
    public float MoveTimer = 1;
    public float ActionTimer = .1f;
    public int CurrentMove;

    Enums.CursorState _currState;
    Enums.CursorState _lastState;
    Enums.PathDirection _lastDir;
    Enums.PathDirection _currDir;

    Animator _animator;
    UnitController _currUnit;
    SpriteRenderer _sR;
    List<MoveSpace> _moves;
    //MoveSpace _lastMove;
    Vector2 _startPos;
    Vector2 _attackPos;
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
        _lastDir = _currDir = Enums.PathDirection.Start;

        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _moves = new List<MoveSpace>();
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
        var movePath = gO.GetComponentInChildren<MoveSpace>();
        if (movePath != null)
        {
            CopyLastMove(movePath);
        }
        else if (CheckForNoGo(gO))
        {
            _sR.color = _errorColor;
            _noGo = true;
        }
        else if (CheckForYesGo())
        {
            GetUnit(gO);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(CheckForYesGo())
        {
            GameObject gO = collision.gameObject;
            GetUnit(gO);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        if (CheckForNoGo(gO))
        {
            _sR.color = _playerColor;
            _noGo = false;
        }
        else
        {
            if (_currUnit != null)
                _currUnit.Hover(false);
            if (_currState == Enums.CursorState.Default)
                _currUnit = null;
        }
    }

    private bool CheckForNoGo(GameObject gO)
    {
                // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
        return _currState == Enums.CursorState.Moving && (gO.layer == LayerMask.NameToLayer("Map") || (gO.tag == "Unit" && gO.transform.position != _currUnit.transform.position));
    }

    private bool CheckForYesGo()
    {
        return _currState == Enums.CursorState.Default && _currUnit == null;
    }

    private void GetUnit(GameObject gO)
    {
        UnitController tmpUnit = gO.GetComponent<UnitController>();
        if (tmpUnit != null && tmpUnit.Player == Player && !tmpUnit.OnCooldown)
        {
            _currUnit = tmpUnit;
            _currUnit.Hover(true);
        }
    }

    private void CheckForAction()
    {
        if (_actionTimer <= 0)
        {
            if (_cancel)
            {
                Cancel();
            }
            else if (_attack)
            {
                Attack();
            }
            else if (_select && _currUnit != null && _currState != Enums.CursorState.Attacking)
            {
                Select();
            }
            // In order for the cursor to move it needs to meet the following conditions
            // - The action timer is off cooldown
            // - There is horizontal or vertical movement
            // - There isn't a unit selected
            // - If there is a unit selected
            //   - It hasn't moved or isn't moving
            //   - The cursor is in attack state
            else if (_moveTimer <= 0 && (_horz != 0 || _vert != 0) && 
                (_currUnit != null ? (_currState == Enums.CursorState.Attacking) || 
                (!_currUnit.Moving && !_currUnit.Moved) : true))
            {
                Move();
            }
        }

        if (_actionTimer > 0)
            _actionTimer -= Time.deltaTime;
        if (_moveTimer > 0)
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
                _animator.SetBool("Attacking", false);
                _moves.ForEach(i => i.Show = true);
                break;
            case Enums.CursorState.Moving:
                if (_currUnit.Moved || _currUnit.Moving)
                {
                    _currDir = _lastDir;
                    _currUnit.CancelMove();
                }
                else if (transform.position.V2() == _startPos)
                {
                    _currState = Enums.CursorState.Default;
                    _currUnit.Select(false);
                }
                else
                {
                    ClearPath();
                }
                break;
            default:
                break;
        }
        _actionTimer = ActionTimer;
    }

    private void Select()
    {
        if(_currState == Enums.CursorState.Default)
        {
            _currState = Enums.CursorState.Moving;
            if (transform.position == _currUnit.GetHolder().position)
            {
                if (!_currUnit.Moved)
                {
                    _startPos = transform.position;
                    ClearPath();
                }
                _currUnit.Select(true);
            }
        }
        else if (!_noGo && !_currUnit.Moving && !_currUnit.Moved && transform.position.V2() != _startPos)
        {
            CreateMovePath(_currState == Enums.CursorState.Attacking ? _attackPos : transform.position.V2(), true);
            _currUnit.MoveTo(_moves);
        }
        else if(_currUnit.Moving || _currUnit.Moved)
        {
            _currUnit.Select(false);
            _currUnit = null;
            _currState = Enums.CursorState.Default;
            ClearPath(false);
        }

        _actionTimer = ActionTimer;
    }

    private void Move()
    {
        Vector2 tmpPos = transform.position;

        tmpPos.x = Mathf.Clamp(tmpPos.x + _horz, -_horzClamp, _horzClamp);
        tmpPos.y = Mathf.Clamp(tmpPos.y + _vert, -_vertClamp, _vertClamp);

        float moveDis = Vector2.Distance(tmpPos, _startPos);
        float attackDis = Vector2.Distance(tmpPos, _attackPos);

        if (_currState == Enums.CursorState.Default)
        {
            transform.position = tmpPos;
        } 
        else if (_currState == Enums.CursorState.Moving && moveDis <= _currUnit.TotalMoves)
        {
            if (tmpPos == _startPos)
            {
                ClearPath();
            }
            //else if(_lastMove != null && _lastMove.Position == tmpPos)
            //{
            //    CopyLastMove(_lastMove);
            //}
            else if(!_noGo && !_moves.Any(i => i.Position == tmpPos))
            {
                SetCursorMove();
                CreateMovePath(transform.position);
            }

            transform.position = tmpPos;
        }
        else if(_currState == Enums.CursorState.Attacking && attackDis <= _currUnit.AttackDistance)
        {
            transform.position = tmpPos;
        }

        _moveTimer = MoveTimer;
        _actionTimer = ActionTimer;
    }

    private void Attack()
    {
        if (_currState != Enums.CursorState.Attacking)
        {
            _lastState = _currState;
            _currState = Enums.CursorState.Attacking;
            _animator.SetBool("Attacking", true);
            _attackPos = transform.position;
            _moves.ForEach(i => i.Show = false);
        }
        else if (transform.position != _currUnit.GetHolder().position)
        {
            if (_moves.Count > 0 && (_currUnit.Moving || !_currUnit.Moved))
            {
                Select();
            }
            ClearPath(false);
            _currUnit.ReadyAttack(transform.position);
            _currState = Enums.CursorState.Default;
            _animator.SetBool("Attacking", false);
            _currUnit = null;
        }

        _actionTimer = ActionTimer;
    }

    private void CreateMovePath(Vector2 move, bool end = false)
    {
        _lastDir = _currDir;
        if (!end)
            _currDir = _horz == 1 ? Enums.PathDirection.Right : _horz == -1 ? Enums.PathDirection.Left : _vert == 1 ? Enums.PathDirection.Up : Enums.PathDirection.Down;
        else
            _currDir = Enums.PathDirection.End;

        CurrentMove = _moves.Count+1;
        var newPath = Instantiate(MovePath, new Vector3(move.x, move.y, 0), Quaternion.identity);
        MoveSpace mS = newPath.GetComponentInChildren<MoveSpace>();
        mS.MoveState(Player, _lastDir, _currDir, this, _moves.Count);
        _moves.Add(mS);
        //_lastMove = mS;
    }

    private void CopyLastMove(MoveSpace mS)
    {
        _currDir = mS.PathDirection;
        CurrentMove = mS.PathOrder;
        while (_moves.Count > CurrentMove)
            _moves.Pop();

        _vert = _currDir == Enums.PathDirection.Down ? -1 : _currDir == Enums.PathDirection.Up ? 1 : 0;
        _horz = _currDir == Enums.PathDirection.Left ? -1 : _currDir == Enums.PathDirection.Right ? 1 : 0;

        if (mS.Position == _startPos)
            ClearPath();
        else
            SetCursorMove();
    }

    private void ClearPath(bool resetLoc = true)
    {
        _animator.SetBool("Moving", false);
        _lastDir = _currDir = Enums.PathDirection.Start;
        _moves.Clear();
        if (resetLoc)
            transform.position = _startPos;
    }

    private void SetCursorMove()
    {
        _animator.SetBool("Moving", true);
        _animator.SetFloat("Horizontal", _horz);
        _animator.SetFloat("Vertical", _vert);
    }
}
