using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class CursorController : MonoBehaviour
{
    [HideInInspector]
    public delegate void CursorMoveEventHandler(object sender, CursorMoveEventArgs e);
    [HideInInspector]
    public event CursorMoveEventHandler OnCursorMoveEvent;

    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public AudioClip SoundSelect;
    public AudioClip SoundDeselect;
    public AudioClip SoundMove;
    public AudioClip SoundAttack;
    public float MoveTimer = 1;
    public float ActionTimer = .1f;
    public int CurrentMove;

    [HideInInspector]
    public Space CurrentSpace { get; set; }
    private UnitController CurrentUnit { get; set; }

    Enums.CursorState _currState;

    PlayerManager _pM;
    GridBlock _currGridBlock;
    GridBlock _orgGridBlock;
    Animator _animator;
    SpriteRenderer _sR;
    AudioSource _aS;
    List<GridBlock> _moves;
    Vector2 _startPos;
    Color _playerColor;
    bool _select;
    bool _cancel;
    bool _nextUnit;
    float _moveTimer;
    float _maxXClamp;
    float _minXClamp;
    float _minYClamp;
    float _maxYClamp;
    float _actionTimer;
    int _vert;
    int _horz;
    string _playerPrefix;

    private void Awake()
    {
        _currState = Enums.CursorState.Default;
        _moveTimer = 0;
        _actionTimer = 0;
        _playerPrefix = Player == Enums.Player.Player1 ? "P1" : "P2";
    }
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _aS = GetComponent<AudioSource>();
        _pM = FindObjectOfType<PlayerManager>();
        _minXClamp = Boundaries.points[0].x + .5f;//.bounds.extents.x - .5f;
        _maxYClamp = Boundaries.points[0].y - .5f;
        _maxXClamp = Boundaries.points[2].x - .5f;
        _minYClamp = Boundaries.points[2].y + .5f;
        _startPos = transform.position;

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
        _nextUnit = Input.GetButtonUp("NextUnit");

        if (_currState == Enums.CursorState.Selected && CurrentUnit == null)
            _currState = Enums.CursorState.Default;

        CheckForAction();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        var grid = gO.GetComponent<GridBlock>();

        if (grid != null)
        {
            _currGridBlock = grid;
            if (_currState == Enums.CursorState.Default)
                CurrentUnit = GetUnit(grid);

            CurrentSpace = grid.PlayerActiveSpace;

            if (CurrentSpace is MoveSpace && _currState == Enums.CursorState.Selected && (_currGridBlock.CurrentUnit == null || _currGridBlock.CurrentUnit == CurrentUnit) && !CurrentUnit.Moving && !CurrentUnit.Moved)
            {
                _moves = _pM.CreatePath(Player, _orgGridBlock, _currGridBlock).ToList();
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
        if (CurrentUnit != null)
            // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
            return _currState == Enums.CursorState.Selected && (gO.tag == "Unit" && gO.transform.position != CurrentUnit.transform.position);

        return false;
    }

    private UnitController GetUnit(GridBlock gB)
    {
        UnitController tmpUnit = gB.CurrentUnit;

        if (tmpUnit != null && (tmpUnit.Player != Player || tmpUnit.OnCooldown || tmpUnit.Moving))
        {
            Debug.Log($"Current unit {tmpUnit.gameObject.name} is not applicable");
            tmpUnit = null;
        }
        else if (tmpUnit != null)
        {
            Debug.Log($"Current unit {tmpUnit.gameObject.name}");
            tmpUnit.OnUnitDeath += OnCurrentUnitDeath;
            CurrentUnit.Hover(true);
        }

        return tmpUnit;
    }

    private void OnCurrentUnitDeath()
    {
        ResetCursor();
    }

    private void ResetCursor(bool resetUnit = true)
    {
        Debug.Log("Cursor Reset");
        _currState = Enums.CursorState.Default;
        CurrentSpace = null;
        _pM.ResetPathMatrix(Player);
        _animator.SetBool("Attacking", false);
        _moves = null;
        if (CurrentUnit != null && resetUnit)
        {
            CurrentUnit.OnUnitDeath -= OnCurrentUnitDeath;
            CurrentUnit = null;
            Debug.Log("Current Unit null");
        }
    }

    private void CheckForAction()
    {
        if (_actionTimer <= 0)
        {
            if (_nextUnit && _currState == Enums.CursorState.Default)
            {
                QuickSelect();
                _actionTimer = ActionTimer;
            }
            if (_cancel)
            {
                Cancel();
                _actionTimer = ActionTimer;
            }
            else if (_select && CurrentUnit != null && !CurrentUnit.OnCooldown)
            {
                Select();
                _actionTimer = ActionTimer;
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
                _actionTimer = ActionTimer;
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
        var nextPos = _pM.GetNextUnit(Player, CurrentUnit);
        if (nextPos != null)
        {
            transform.position = nextPos.Value;
            _aS.Play(SoundMove);
        }
    }

    private void Cancel()
    {
        if (_currState == Enums.CursorState.Selected)
        {
            if (CurrentUnit.Moved || CurrentUnit.Moving)
            {
                CurrentUnit.CancelMove();
                _pM.GetPlayerInfo(Enums.Player.Player1).MovementPath.Clear();
                transform.position = _orgGridBlock.Position;
            }
            else if (transform.position.V2() == _startPos)
            {
                CurrentUnit.Select(false);
                ResetCursor(resetUnit: false);
            }
            else
            {
                transform.position = _orgGridBlock.Position;
            }

            _aS.Play(SoundDeselect);
        }
    }

    private void Select()
    {
        if (_currState == Enums.CursorState.Default)
        {
            _currState = Enums.CursorState.Selected;
            _orgGridBlock = _currGridBlock;

            if (transform.position == CurrentUnit.Position)
            {
                if (!CurrentUnit.Moved)
                {
                    _startPos = transform.position;
                }
                CurrentUnit.Select(true);

                _aS.Play(SoundSelect);

                StartCoroutine(_pM.CreateGridAsync(_currGridBlock, Player, _currGridBlock, CurrentUnit.MoveDistance + _orgGridBlock.MovementCost, 
                    CurrentUnit.MinAttackDistance, CurrentUnit.MaxAttackDistance));
            }
        }
        else if (!CurrentUnit.Moving && !CurrentUnit.Moved && transform.position.V2() != _startPos && CurrentSpace != null && 
            (_currGridBlock.IsCurrentUnitEnemy(Player) || (CurrentSpace is MoveSpace && _currGridBlock.CurrentUnit == null)))
        {
            List<GridBlock> backupSpaces = null;
            var unit = _currGridBlock.CurrentUnit;
            double dis = 9999;
            if(unit != null)
                dis = unit.Position.GridDistance(CurrentUnit.Position);
            CurrentUnit.Target = unit;

            if (_moves?.Count > 0 && (dis > CurrentUnit.MaxAttackDistance || dis < CurrentUnit.MaxAttackDistance))
            {
                if (CurrentUnit.Target != null)
                {
                    if (_moves.Last().Position.GridDistance(unit.Position) <= CurrentUnit.MaxAttackDistance - 1)
                    {
                        var lastGrid = _moves.Last();
                        var newTargetGrid = lastGrid.Neighbors.OrderByDistance(_currGridBlock, true).ToList().FirstOrDefault();
                        
                        if(newTargetGrid != null && _moves.Contains(newTargetGrid))
                        {
                            int index = _moves.IndexOf(newTargetGrid);
                            _moves.RemoveRange(index, _moves.Count() - index - 1); // the -1 is to acount for index being 0 based and Count() being 1 based.
                        }
                        else if(newTargetGrid != null)
                        {
                            _moves.Add(newTargetGrid);
                        }
                        else if(newTargetGrid == null)
                        {
                            CurrentUnit.Target = null;
                            return;
                        }
                    }
                }

                if (_moves?.Count > 0)
                {
                    CurrentUnit.MoveTo(_moves);
                }
            }
            else if (dis <= CurrentUnit.MaxAttackDistance && dis > CurrentUnit.MinAttackDistance)
            {
                _aS.Play(SoundAttack);
                CurrentUnit.CheckAttack(unit);
            }
            else if ((backupSpaces = _orgGridBlock.AvailableAttackSpace(_currGridBlock)).Count > 0)
            {
                _aS.Play(SoundAttack);
                CurrentUnit.MoveTo(new List<GridBlock>() { _orgGridBlock, backupSpaces.First() });
            }
            else
            {
                CurrentUnit.Target = null;
                return;
            }

            if (CurrentUnit.Target != null && unit.IsEnemy(Player))
            {
                _aS.Play(SoundAttack);
                CurrentUnit.Select(false);
                ResetCursor();
            }
        }
        else if ((CurrentUnit.Moving || CurrentUnit.Moved) && _currGridBlock.IsCurrentUnitEnemy(Player) && CurrentSpace != null)
        {
            _aS.Play(SoundAttack);
            CurrentUnit.Target = _currGridBlock.CurrentUnit;
            CurrentUnit.Select(false);
            ResetCursor();
        }
        else if ((CurrentUnit.Moving || CurrentUnit.Moved))
        {
            _aS.Play(SoundAttack);
            CurrentUnit.Select(false);
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
            _aS.Play(SoundMove);
        }

        _moveTimer = MoveTimer;
    }
}

public class CursorMoveEventArgs
{
    public Vector2 Position { get; private set; }

    public CursorMoveEventArgs(Vector2 position)
    {
        Position = position;
    }
}
