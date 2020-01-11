using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class CursorController : MonoBehaviour
{
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
    public UnitController CurrentUnit { get; set; }

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
    float _horzClamp;
    float _vertClamp;
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
        _horzClamp = Boundaries.bounds.extents.x;
        _vertClamp = Boundaries.bounds.extents.y;
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

        CheckForAction();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        var grid = gO.GetComponent<GridBlock>();

        if (grid != null)
        {
            _currGridBlock = grid;
            CurrentSpace = grid.PlayerActiveSpace;

             if (CurrentSpace is MoveSpace && _currState == Enums.CursorState.Selected && (_currGridBlock.CurrentUnit == null || _currGridBlock.CurrentUnit == CurrentUnit) && !CurrentUnit.Moving && !CurrentUnit.Moved)
            {
                _moves = _pM.CreatePath(Player, _orgGridBlock, _currGridBlock).ToList();
            }
        }

        if (CheckForUnit())
            GetUnit(gO);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //if (CheckForUnit())// && _stayUnitSearch)
        //{
        //    GameObject gO = collision.gameObject;
        //    GetUnit(gO);
        //}
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
        if(CurrentUnit != null)
            // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
            return _currState == Enums.CursorState.Selected && (gO.tag == "Unit" && gO.transform.position != CurrentUnit.transform.position);
        
        return false;
    }

    private bool CheckForUnit()
    {
        return _currState == Enums.CursorState.Default && CurrentUnit == null;
    }

    private void GetUnit(GameObject gO)
    {
        UnitController tmpUnit = gO.GetComponent<UnitController>();
        if (tmpUnit != null && tmpUnit.Player == Player && !tmpUnit.OnCooldown && !tmpUnit.Moving)
        {
            CurrentUnit = tmpUnit;
            CurrentUnit.OnUnitDeath += OnCurrentUnitDeath;
            CurrentUnit.Hover(true);
        }
    }

    private void OnCurrentUnitDeath()
    {
        ResetCursor();
    }

    private void ResetCursor(bool resetUnit = true)
    {
        _currState = Enums.CursorState.Default;
        CurrentSpace = null;
        _pM.ResetPathMatrix(Player);
        _animator.SetBool("Attacking", false);
        _moves = null;
        if (CurrentUnit != null && resetUnit)
        {
            CurrentUnit.OnUnitDeath -= OnCurrentUnitDeath;
            CurrentUnit = null;
        }
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
            //else if (_attack)
            //{
            //    Attack();
            //}
            else if (_select && CurrentUnit != null && !CurrentUnit.OnCooldown)
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
            else if (_moveTimer <= 0 && (_horz != 0 || _vert != 0))
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
        _actionTimer = ActionTimer;
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

                StartCoroutine(_pM.CreateGridAsync(Player, _currGridBlock, CurrentUnit.MoveDistance + _orgGridBlock.MovementCost, CurrentUnit.MaxAttackDistance));
            }
        }
        else if (!CurrentUnit.Moving && !CurrentUnit.Moved && transform.position.V2() != _startPos && CurrentSpace != null && 
            (_currGridBlock.IsCurrentUnitEnemy(Player) || (CurrentSpace is MoveSpace && _currGridBlock.CurrentUnit == null)))
        {
            var unit = _currGridBlock.CurrentUnit;
            CurrentUnit.Target = unit;

            if (_moves?.Count > 0)
            {
                _aS.Play(SoundSelect);
                CurrentUnit.MoveTo(_moves);
            }
            else
            {
                _aS.Play(SoundAttack);
                CurrentUnit.CheckAttack(unit);
            }

            if (unit != null && unit.IsEnemy(Player))
                ResetCursor();

        }
        else if ((CurrentUnit.Moving || CurrentUnit.Moved) && _currGridBlock.IsCurrentUnitEnemy(Player) && CurrentSpace != null)
        {
            _aS.Play(SoundAttack);
            CurrentUnit.Target = _currGridBlock.CurrentUnit;
            ResetCursor();
        }
        else if ((CurrentUnit.Moving || CurrentUnit.Moved))
        {
            _aS.Play(SoundAttack);
            CurrentUnit.Select(false);
            ResetCursor();
        }

        _actionTimer = ActionTimer;
    }

    

    private void Move()
    {
        Vector2 tmpPos = transform.position.Copy();

        tmpPos.x = Mathf.Clamp(tmpPos.x + _horz, -_horzClamp, _horzClamp);
        tmpPos.y = Mathf.Clamp(tmpPos.y + _vert, -_vertClamp, _vertClamp);

        if (tmpPos != transform.position.V2())
        {
            transform.position = tmpPos;
            _aS.Play(SoundMove);
        }

        _moveTimer = MoveTimer;
        _actionTimer = ActionTimer;
    }
}
