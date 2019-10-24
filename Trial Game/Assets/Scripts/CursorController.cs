using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class CursorController : MonoBehaviour
{
    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public float MoveTimer = 1;
    public float ActionTimer = .1f;
    public int CurrentMove;

    Enums.CursorState _currState;

    PlayerManager _pM;
    GridBlock _currGridBlock;
    GridBlock _orgGridBlock;
    Animator _animator;
    UnitController _currUnit;
    SpriteRenderer _sR;
    List<GridBlock> _moves;
    Space _currSpace;
    Vector2 _startPos;
    Vector2 _attackPos;
    Color _playerColor;
    bool _select;
    bool _attack;
    bool _cancel;
    bool _nextUnit;
    float _moveTimer;
    float _horzClamp;
    float _vertClamp;
    float _actionTimer;
    int _vert;
    int _horz;
    int _gridSize;
    string _playerPrefix;

    // Start is called before the first frame update
    void Start()
    {
        _currState = Enums.CursorState.Default;

        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
        _moveTimer = 0;
        _horzClamp = Boundaries.bounds.extents.x;
        _vertClamp = Boundaries.bounds.extents.y;
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
        _nextUnit = Input.GetButtonUp("NextUnit");

        CheckForAction();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        var space = gO.GetComponent<Space>();
        var grid = gO.GetComponent<GridBlock>();

        if (grid != null)
            _currGridBlock = grid;

        //if (CheckForObstacle(gO))
        //    _okayToMove = false;
        //else if (CheckForUnit())
        if (CheckForUnit())
            GetUnit(gO);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var space = collision.gameObject.GetComponent<Space>();

        if (space != null && space.Player == Player && (_currGridBlock.CurrentUnit == null || _currGridBlock.CurrentUnit == _currUnit))// && _currState == Enums.CursorState.Moving)
        {
            _currSpace = space;

            if (space is MoveSpace)
            {
                _moves = _pM.CreatePath(Player, _orgGridBlock, _currSpace.ParentGridBlock).ToList();
            }
        }
        else if (CheckForUnit())
        {
            GameObject gO = collision.gameObject;
            GetUnit(gO);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;

        if (CheckForObstacle(gO))
        {
            _sR.color = _playerColor;
        }
        else
        {
            _currUnit?.Hover(false);
            if (_currState == Enums.CursorState.Default)
                ResetUnit();
        }
    }

    private bool CheckForObstacle(GameObject gO)
    {
        if(_currUnit != null)
            // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
            return _currState == Enums.CursorState.Selected && (gO.tag == "Unit" && gO.transform.position != _currUnit.transform.position);
        
        return false;
    }

    private bool CheckForUnit()
    {
        return _currState == Enums.CursorState.Default && _currUnit == null;
    }

    private void GetUnit(GameObject gO)
    {
        UnitController tmpUnit = gO.GetComponent<UnitController>();
        if (tmpUnit != null && tmpUnit.Player == Player && !tmpUnit.OnCooldown && !tmpUnit.Moving)
        {
            _currUnit = tmpUnit;
            _currUnit.OnUnitDeath += OnCurrentUnitDeath;
            _currUnit.Hover(true);
        }
    }

    private void OnCurrentUnitDeath()
    {
        ResetUnit();
        ResetCursor();
    }

    private void ResetCursor()
    {
        _currState = Enums.CursorState.Default;
        _currSpace = null;
        _pM.ResetPathMatrix(Player);
        _animator.SetBool("Attacking", false);
    }

    private void ResetUnit()
    {
        if (_currUnit != null)
            _currUnit.OnUnitDeath -= OnCurrentUnitDeath;
        _currUnit = null;   
    }

    private void CheckForAction()
    {
        if (_actionTimer <= 0)
        {
            if (_nextUnit && _currState == Enums.CursorState.Default)
            {
                NextUnit();
            }
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

    private void NextUnit()
    {
        var nextPos = _pM.GetNextUnit(Player, _currUnit);
        if (nextPos != null)
            transform.position = nextPos.Value;
    }

    private void Cancel()
    {
        if (_currState == Enums.CursorState.Selected)
        {
            if (_currUnit.Moved || _currUnit.Moving)
            {
                _currUnit.CancelMove();
                transform.position = _orgGridBlock.Position;
            }
            else if (transform.position.V2() == _startPos)
            {
                _currUnit.Select(false);
                ResetCursor();
            }
            else
            {
                transform.position = _orgGridBlock.Position;
            }
        }
        _actionTimer = ActionTimer;
    }

    private void Select()
    {
        if (_currState == Enums.CursorState.Default)
        {
            _currState = Enums.CursorState.Selected;
            _orgGridBlock = _currGridBlock;

            if (transform.position == _currUnit.Position)
            {
                if (!_currUnit.Moved)
                {
                    _startPos = transform.position;
                }
                _currUnit.Select(true);
                _gridSize = _currUnit.MoveDistance + _currUnit.AttackDistance;

                StartCoroutine(_pM.CreateGridAsync(Player, _currGridBlock, _currUnit.MoveDistance + _orgGridBlock.MovementCost, _currUnit.AttackDistance));
            }
        }
        else if (!_currUnit.Moving && !_currUnit.Moved && transform.position.V2() != _startPos && _currSpace is MoveSpace && _currGridBlock.CurrentUnit == null)
        {
            _currUnit.MoveTo(_moves);
        }
        else if (_currUnit.Moving || _currUnit.Moved)
        {
            _currUnit.Select(false);
            ResetUnit();
            ResetCursor();
            _moves = null;
        }

        _actionTimer = ActionTimer;
    }

    private void Move()
    {
        Vector2 tmpPos = transform.position.Copy();

        tmpPos.x = Mathf.Clamp(tmpPos.x + _horz, -_horzClamp, _horzClamp);
        tmpPos.y = Mathf.Clamp(tmpPos.y + _vert, -_vertClamp, _vertClamp);

        var dir = tmpPos - transform.position.V2();

        //if (_currState == Enums.CursorState.Moving)
        //{
        //    ContactFilter2D filter = new ContactFilter2D();
        //    LayerMask mask = LayerMask.GetMask("GridBlock");
        //    filter.SetLayerMask(mask);
        //    filter.useTriggers = true;
        //    RaycastHit2D[] results = new RaycastHit2D[6];
        //    int hits = Physics2D.Raycast(transform.position.V2()/* + dir * .8f*/, dir, filter, results, 1f);
        //    Debug.Log($"Hits {hits}");
        //    if (hits > 0)
        //    {
        //        var collider = results[0].collider;
        //        var grid = collider.GetComponent<GridBlock>();
        //        if (grid.CurrentUnit?.Player == Enums.Player.Player2)
        //            return;
        //    }
        //}       

        if (_currState == Enums.CursorState.Default)
        {
            transform.position = tmpPos;
        }
        else if (_currState == Enums.CursorState.Selected)
        {
            transform.position = tmpPos;
        }
        else if (_currState == Enums.CursorState.Attacking)
        {
            transform.position = tmpPos;
        }

        _moveTimer = MoveTimer;
        _actionTimer = ActionTimer;
    }

    private void Attack()
    {
        if (_currUnit == null)
            return;

        if( _currState == Enums.CursorState.Selected && _currGridBlock.IsSpaceActive &&   
            _currGridBlock.CurrentUnit != null && _currGridBlock.CurrentUnit.Player != Player)
        {
            if (_moves != null && _moves.Count > 0 && (_currUnit.Moving || !_currUnit.Moved))
            {
                _currUnit.MoveTo(_moves);
            }

            _currUnit.ReadyAttack(transform.position);
            _animator.SetBool("Attacking", false);
            ResetUnit();
            ResetCursor();
            _moves = null;
        }

        //if (_currState != Enums.CursorState.Attacking &&
        //    (_currGridBlock.CurrentUnit == null || _currGridBlock.CurrentUnit == _currUnit) &&
        //    (_currUnit.Position == transform.position || _currGridBlock.IsSpaceActive))
        //{
        //    _currUnit.Select(true);
        //    _gridSize = _currUnit.AttackDistance;
        //    StartCoroutine(_pM.CreateGridAsync(Player, _currGridBlock, 0, _gridSize));

        //    _lastState = _currState;
        //    _currState = Enums.CursorState.Attacking;
        //    _animator.SetBool("Attacking", true);   
        //    _attackPos = transform.position;
        //}
        //else if (transform.position != _currUnit.Position && transform.position.V2() != _attackPos && 
        //         _currSpace != null && (_currGridBlock.CurrentUnit == null || _currGridBlock.CurrentUnit.Player != Player))
        //{
        //    if (_moves != null && _moves.Count > 0 && (_currUnit.Moving || !_currUnit.Moved))
        //    {
        //        _currUnit.MoveTo(_moves);
        //    }

        //    _currUnit.ReadyAttack(transform.position);
        //    _animator.SetBool("Attacking", false);
        //    ResetUnit();
        //    ResetCursor();
        //    _moves = null;
        //}

        _actionTimer = ActionTimer;
    }
}
