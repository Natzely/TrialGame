using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class CursorController : MonoBehaviour
{
    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public float MoveTimer = 1;
    public float ActionTimer = .1f;
    public int CurrentMove;

    Enums.CursorState _currState;
    Enums.CursorState _lastState;

    PlayerManager _playerManager;
    GridBlock _currGridBlock;
    GridBlock _orgGridBlock;
    Animator _animator;
    UnitController _currUnit;
    SpriteRenderer _sR;
    List<MoveSpace> _moves;
    MoveSpace _currMoveSpace;
    MoveSpace _orgMoveSpace;
    Vector2 _startPos;
    Vector2 _attackPos;
    Color _playerColor;
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

        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _playerManager = FindObjectOfType<PlayerManager>();
        _moves = new List<MoveSpace>();
        _noGo = false;
        _moveTimer = 0;
        _horzClamp = (int)Boundaries.bounds.extents.x;
        _vertClamp = (int)Boundaries.bounds.extents.y;
        _actionTimer = 0;
        _startPos = transform.position;
        _playerPrefix = Player == Enums.Player.Player1 ? "P1" : "P2";

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
        _select = Input.GetButtonUp(_playerPrefix + "_Select");
        _cancel = Input.GetButtonUp(_playerPrefix + "_Cancel");
        _attack = Input.GetButtonUp(_playerPrefix + "_Attack");

        CheckForAction();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        var moveSpace = gO.GetComponent<MoveSpace>();
        var grid = gO.GetComponent<GridBlock>();

        if (grid != null)
        {
            _currGridBlock = grid;
        }

        if (moveSpace != null && moveSpace.Player == Player && _currState == Enums.CursorState.Moving)
        {
            if (_orgMoveSpace == null)
                _orgMoveSpace = moveSpace;

            _currMoveSpace = moveSpace;

            _moves = _playerManager.CreatePath(Player, _orgMoveSpace, _currMoveSpace).ToList();
        }
        else if (CheckForNoGo(gO))
        {
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
        MoveSpace ms = gO.GetComponent<MoveSpace>();

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
                    _currUnit.CancelMove();
                    transform.position = _orgMoveSpace.Position;
                }
                else if (transform.position.V2() == _startPos)
                {
                    _currState = Enums.CursorState.Default;
                    _currUnit.Select(false);
                    _playerManager.SetDeleteMoveSpace(Player, true);
                    _currMoveSpace = _orgMoveSpace = null;
                }
                else
                {
                    transform.position = _orgMoveSpace.Position;
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
            _playerManager.SetDeleteMoveSpace(Player, false);
            _currState = Enums.CursorState.Moving;
            if (transform.position == _currUnit.GetHolder().position)
            {
                if (!_currUnit.Moved)
                {
                    _startPos = transform.position;
                }
                _currUnit.Select(true);
                int gridSize = _currUnit.TotalMoves + _currUnit.AttackDistance;
                _playerManager.SetPathMatrix(Player, gridSize);
                _orgMoveSpace = _currMoveSpace;
                _currGridBlock.CreateGrid(Player, _currUnit.TotalMoves + _currGridBlock.MovementCost, _currUnit.AttackDistance, new Vector2(gridSize, gridSize));
                var matrix = _playerManager.GetPathMatrix(Player);
            }
        }
        else if (!_noGo && !_currUnit.Moving && !_currUnit.Moved && transform.position.V2() != _startPos)
        {
            _currUnit.MoveTo(_moves);
        }
        else if(_currUnit.Moving || _currUnit.Moved)
        {
            _currUnit.Select(false);
            _currUnit = null;
            _currState = Enums.CursorState.Default;
            _playerManager.SetDeleteMoveSpace(Player, true);
        }

        _actionTimer = ActionTimer;
    }   

    private void Move()
    {
        Vector2 tmpPos = transform.position.Copy();

        tmpPos.x = Mathf.Clamp(tmpPos.x + _horz, -_horzClamp, _horzClamp);
        tmpPos.y = Mathf.Clamp(tmpPos.y + _vert, -_vertClamp, _vertClamp);

        float moveDis = Vector2.Distance(tmpPos, _startPos);
        float attackDis = Vector2.Distance(tmpPos, _attackPos);

        Vector2 dif = transform.position.V2() - tmpPos;

        if (_currState == Enums.CursorState.Default)
        {
            transform.position = tmpPos;
        }
        else if (_currState == Enums.CursorState.Moving)// && moveDis <= _currUnit.TotalMoves)
        {
            transform.position = tmpPos;
        }
        else if(_currState == Enums.CursorState.Attacking)// && attackDis <= _currUnit.AttackDistance)
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
        }
        else if (transform.position != _currUnit.GetHolder().position)
        {
            if (_moves.Count > 0 && (_currUnit.Moving || !_currUnit.Moved))
            {
                Select();
            }
            _playerManager.SetDeleteMoveSpace(Player, true);
            _currUnit.ReadyAttack(transform.position);
            _currState = Enums.CursorState.Default;
            _animator.SetBool("Attacking", false);
            _currUnit = null;
        }

        _actionTimer = ActionTimer;
    }
}
