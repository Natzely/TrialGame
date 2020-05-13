using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CursorController : MonoBehaviour, ILog
{
    [HideInInspector]
    public delegate void CursorMoveEventHandler(object sender, CursorMoveEventArgs e);
    [HideInInspector]
    public event CursorMoveEventHandler OnCursorMoveEvent;

    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public AudioClip Sound_Attack;
    public AudioClip Sound_Deselect;
    public AudioClip Sound_Move;
    public AudioClip Sound_Select;
    public float MoveTimer = 1;
    public int CurrentMove;

    private UnitController _currUnit { get; set; }

    Enums.CursorState _currState;

    GridBlock _currGridBlock;
    GridBlock _orgGridBlock;
    PlayerManager _pM;
    UnitController _quickSelectUnit;

    Animator _animator;
    SpriteRenderer _sR;
    AudioSource _aS;
    List<GridBlock> _moves;
    Vector2 _startPos;
    Color _playerColor;
    bool _select;
    bool _cancel;
    bool _quickSelect;
    bool _getUnit;
    float _moveTimer;
    float _maxXClamp;
    float _minXClamp;
    float _minYClamp;
    float _maxYClamp;
    float _actionTimer;
    int _vert;
    int _horz;

    public void EnemyInPath(GridBlock gB)
    {
        var enemyIndex = _pM.PlayerInfo.MovementPath.IndexOf(gB);
        var playerIndex = _pM.PlayerInfo.MovementPath.IndexOf(_currUnit.CurrentGridBlock);

        if(playerIndex > enemyIndex)
        {
            _aS.Play(Sound_Deselect);
            SelectUnit(false);
            ResetCursor();
        }
    }

    private void Awake()
    {
        _currState = Enums.CursorState.Default;
        _moveTimer = 0;
        _actionTimer = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _aS = GetComponent<AudioSource>();
        _pM = FindObjectOfType<PlayerManager>();
        _minXClamp = Boundaries.points[1].x + .5f;
        _minYClamp = Boundaries.points[1].y + .5f;
        _maxXClamp = Boundaries.points[3].x - .5f;
        _maxYClamp = Boundaries.points[3].y - .5f;
        _startPos = transform.position;
        _playerColor = _sR.color = Colors.Player1;
    }

    // Update is called once per frame
    void Update()
    {
        _vert = Mathf.RoundToInt(Input.GetAxis("P1_Vertical"));
        _horz = Mathf.RoundToInt(Input.GetAxis("P1_Horizontal"));
        _select = Input.GetButtonUp("P1_Select");
        _cancel = Input.GetButtonUp("P1_Cancel");
        _quickSelect = Input.GetButtonUp("NextUnit");

        if (_currState == Enums.CursorState.Selected && _currUnit == null)
            _currState = Enums.CursorState.Default;

        if (_getUnit && _currGridBlock != null)
        {
            GetUnit(_currGridBlock);
            _getUnit = false;
        }

        CheckForAction();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        var grid = gO.GetComponent<GridBlock>();

        Log("Cursor: Trigger Enter");
        if (grid != null)
        {
            _currGridBlock = grid;
            GetUnit(grid);

            if (grid.ActivePlayerSpace == Enums.ActiveSpace.Move && _currState == Enums.CursorState.Selected && (_currGridBlock.CurrentUnit == null || _currGridBlock.CurrentUnit == _currUnit) && !_currUnit.Moving && !_currUnit.Moved)
            {
                _moves = _pM.CreatePath(_orgGridBlock, _currGridBlock).ToList();
            }
        }
        else if (gO.name.StartsWith("UnitOff"))
        {
            Log("Cursor: Unit cooldown object detected");
            if (_currGridBlock == null)
            {
                Log("Cursor: Get Unit after cooldown... later");
                _getUnit = true;
            }
            else
            {
                GetUnit(_currGridBlock);
                Log("Cursor: Get unit after cooldown");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;

        if (CheckForObstacle(gO))
        {
            _sR.color = _playerColor;
        }

    }

    private bool CheckForObstacle(GameObject gO)
    {
        if (_currUnit != null)
            // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
            return _currState == Enums.CursorState.Selected && (gO.tag == "Unit" && gO.transform.position != _currUnit.transform.position);

        return false;
    }

    private void GetUnit(GridBlock gB)
    {
        if (_currState == Enums.CursorState.Default)
        {
            UnitController tmpUnit = gB.CurrentUnit;

            if (tmpUnit != null && (tmpUnit.Player != Player || tmpUnit.OnCooldown || tmpUnit.Moving))
            {
                Log($"Current unit {tmpUnit.gameObject.name} is not applicable");
                tmpUnit = null;
            }
            else if (tmpUnit != null)
            {
                Log($"Current unit {tmpUnit.gameObject.name}");
                tmpUnit.Hover(true);
            }

            _currUnit = tmpUnit;
        }
    }

    private void OnCurrentUnitInterupt()
    {
        Log($"Current unit {_currUnit.name} was interupted");
        ResetCursor();
    }

    private void ResetCursor(bool resetUnit = true)
    {
        Log("Cursor Reset");
        _currState = Enums.CursorState.Default;
        _pM.ResetBlockGrid();
        _animator.SetBool("Attacking", false);
        _moves = null;
        if (_currUnit != null && resetUnit)
        {
            _currUnit.OnUnitInterupt -= OnCurrentUnitInterupt;
            _currUnit = null;
            Log("Current Unit null");
        }
    }

    private void CheckForAction()
    {
        if (_actionTimer <= 0)
        {
            if (_quickSelect && _currState == Enums.CursorState.Default)
            {
                QuickSelect();
                _actionTimer = _pM.ActionTimer;
            }
            if (_cancel)
            {
                Cancel();
                _actionTimer = _pM.ActionTimer;
            }
            else if (_select && _currUnit != null && !_currUnit.OnCooldown)
            {
                Select();
                _actionTimer = _pM.ActionTimer;
            }
            // In order for the cursor to move it needs to meet the following conditions
            // - The action timer is off cooldown
            // - There is horizontal or vertical movement
            // - There isn't a unit selected
            // - If there is a unit selected
            //   - It hasn't moved or isn't moving
            //   - The cursor is in attack state
            else if (_moveTimer <= 0 && (_horz != 0 || _vert != 0))
            {
                Move();
                OnCursorMoveEvent?.Invoke(this, new CursorMoveEventArgs(transform.position));
                _actionTimer = _pM.ActionTimer;
            }

            //_actionTimer = ActionTimer;  Because CheckForAction is called every Update() frame DO NOT uncomment this. 
        }

        if (_actionTimer > 0)
            _actionTimer -= Time.deltaTime;
        if (_moveTimer > 0)
            _moveTimer -= Time.deltaTime;
    }

    // Selects the Unit closests to the cursor.
    private void QuickSelect()
    {
        _quickSelectUnit = _pM.GetNextUnit(Player, _currUnit ?? _quickSelectUnit);
        if (_quickSelectUnit != null)
        {
            transform.position = _quickSelectUnit.transform.position;
            _aS.Play(Sound_Move);
        }
    }

    private void Cancel()
    {
        if (_currState == Enums.CursorState.Selected)
        {
            if (_currUnit.Moved || _currUnit.Moving)
            {
                _currUnit.CancelMove();
                _pM.PlayerInfo.MovementPath.Clear();
                transform.position = _orgGridBlock.Position;
            }
            else if (transform.position.V2() == _startPos)
            {
                SelectUnit(false);
                _pM.ResetBlockGrid();
                ResetCursor(resetUnit: false);
            }
            else
            {
                transform.position = _orgGridBlock.Position;
            }

            _aS.Play(Sound_Deselect);
        }
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

                SelectUnit(true);

                _aS.Play(Sound_Select);

                _currGridBlock.SetGrid(null, _currUnit.MoveDistance + _currGridBlock.MovementCost, _currUnit.MinAttackDistance, _currUnit.MaxAttackDistance);

                _moves = new List<GridBlock>() { _currGridBlock };
            }
        }
        else if (!_currUnit.Moving && !_currUnit.Moved && transform.position.V2() != _startPos && _currGridBlock.ActivePlayerSpace != Enums.ActiveSpace.Inactive && 
            (_currGridBlock.IsCurrentUnitEnemy(Player) || (_currGridBlock.ActivePlayerSpace == Enums.ActiveSpace.Move && _currGridBlock.CurrentUnit == null)))
        {
            List<GridBlock> backupSpaces = null;
            var unit = _currGridBlock.CurrentUnit;
            double dis = 9999;
            if(unit != null)
                dis = unit.Position.GridDistance(_currUnit.Position);
            _currUnit.Target = unit;

            if(unit == null && _moves?.Count > 1)
            {
                _currUnit.MoveTo(_moves);
            }
            else if (dis >= _currUnit.MinAttackDistance && dis <= _currUnit.MaxAttackDistance && _moves?.Count <= _currUnit.MaxAttackDistance) // If the unit is already in range of the target
            {
                _aS.Play(Sound_Attack);
                _currUnit.CheckAttack(unit);
            }
            else if (unit != null && _moves?.Count > 1)// && dis >= _currUnit)// && (dis >= _currUnit.MaxAttackDistance))// If the unit needs to move to attack
            {
                double lastPosDis = _moves.Last().Position.GridDistance(unit.Position);
                if (lastPosDis < _currUnit.MinAttackDistance)
                {
                    var lastGrid = _moves.Last();
                    var orderedN = lastGrid.Neighbors.OrderByDistance(_orgGridBlock, true);
                    var canMoveTo = orderedN.Where(n => n.ActiveSpace(Player) == Enums.ActiveSpace.Move).ToList();
                    var newTargetGrid = canMoveTo.FirstOrDefault();
                        
                    if(newTargetGrid != null && _moves.Contains(newTargetGrid))
                    {
                        int index = _moves.IndexOf(newTargetGrid) + 1; // Erase everything higher than the selected grid
                        if (index <= _moves.Count())
                            _moves.RemoveRange(index, _moves.Count() - index);
                    }
                    else if(newTargetGrid != null)
                    {
                        _moves.Add(newTargetGrid);
                    }
                    else if(newTargetGrid == null)
                    {
                        _currUnit.Target = null;
                        return;
                    }
                }

                if (_moves?.Count > 0)
                {
                    _currUnit.MoveTo(_moves);
                }
            }
            else if ((backupSpaces = _orgGridBlock.AvailableAttackSpace(_currGridBlock, _currUnit.MaxAttackDistance).ToList()).Count > 0) // If the unit is too close to attack but there is room for it back up and attack
            {
                _aS.Play(Sound_Attack);
                _currUnit.MoveTo(new List<GridBlock>() { _orgGridBlock, backupSpaces.First() });
            }
            else // If non of the other conditions are met, then do nothing.
            {
                _currUnit.Target = null;
                return;
            }

            if (_currUnit.Target != null && unit.IsEnemy(Player))
            {
                _aS.Play(Sound_Attack);
                SelectUnit(false);
                ResetCursor();
            }
        }
        else if ((_currUnit.Moving || _currUnit.Moved) && _currGridBlock.IsCurrentUnitEnemy(Player))
        {
            _aS.Play(Sound_Attack);
            _currUnit.Target = _currGridBlock.CurrentUnit;
            SelectUnit(false);
            ResetCursor();
        }
        else if ((_currUnit.Moving || _currUnit.Moved))
        {
            _aS.Play(Sound_Attack);
            SelectUnit(false);
            ResetCursor();
        }
    }

    private void Move()
    {
        Vector2 tmpPos = transform.position.Copy();

        tmpPos.x = Mathf.Clamp(tmpPos.x + _horz, _minXClamp, _maxXClamp);
        tmpPos.y = Mathf.Clamp(tmpPos.y + _vert, _minYClamp, _maxYClamp);

        if (tmpPos != transform.position.V2())
        {
            transform.position = tmpPos;
            _aS.Play(Sound_Move);
        }

        _moveTimer = MoveTimer;
    }

    private void SelectUnit(bool select)
    {
        _currUnit.Select(select);
        if (select)
            _currUnit.OnUnitInterupt += OnCurrentUnitInterupt;
        else
            _currUnit.OnUnitInterupt -= OnCurrentUnitInterupt;
    }

    public void Log(string msg)
    {
        _pM.Log($"{gameObject.name} | {msg}");
    }
}

public class CursorMoveEventArgs : EventArgs
{
    public Vector2 Position { get; private set; }

    public CursorMoveEventArgs(Vector2 position)
    {
        Position = position;
    }
}
