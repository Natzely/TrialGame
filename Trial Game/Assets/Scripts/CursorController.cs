﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour, ILog
{
    [HideInInspector]
    public delegate void CursorMoveEventHandler(object sender, CursorMoveEventArgs e);
    [HideInInspector]
    public event CursorMoveEventHandler OnCursorMoveEvent;

    public CameraController _cameraController;
    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public AudioClip Sound_Attack;
    public AudioClip Sound_Deselect;
    public AudioClip Sound_Move;
    public AudioClip Sound_Select;
    [SerializeField] private Image MiniMapIcon;
    public float MoveTimer = 1;
    public int CurrentMove;

    public UnitController CurrentUnit
    {
        get { return _pM.PlayerInfo.SelectedUnit; }
        private set { _pM.PlayerInfo.SelectedUnit = value; }
    }
    public GridBlock CurrentGridBlock { get; private set; }

    private Enums.CursorState _cursorState;
    private Enums.CursorState CursorState
    {
        get { return _cursorState; }
        set
        {
            _cursorState = value;
            _animator.SetInteger("State", (int)_cursorState);
            if (_cameraController) _cameraController.UpdateZoom(_cursorState);
        }
    }

    public Vector2 Position { get { return transform.position; } }

    private GridBlock _orgGridBlock;
    private GridBlock _attackFromGridBlock;
    private PlayerManager _pM;
    private UnitController _quickSelectUnit;
    private CursorMenuManager _cursorMenu;

    private Animator _animator;
    private SpriteRenderer _sR;
    private AudioSource _aS;
    private Image _miniMapIcon;
    private List<MovePoint> _moves;
    private Vector2 _startPos;
    private Vector2 _moveVector;
    private Vector2 _maxClamp;
    private Vector2 _minClamp;
    private Color _playerColor;
    private bool _getUnit;
    private bool _delayedCursorState;
    private bool _onlyAttack;
    private float _moveTimer;
    private float _moveValue;

    public void EnemyInPath(GridBlock gB)
    {
        var enemyIndex = _pM.PlayerInfo.MovementPath.IndexOf(gB.ToMovePoint());
        var playerIndex = _pM.PlayerInfo.MovementPath.IndexOf(CurrentUnit.CurrentGridBlock.ToMovePoint());

        if (playerIndex > enemyIndex)
        {
            _aS.Play(Sound_Deselect);
            SelectUnit(false);
            ResetCursor();
        }
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void CurrentGridUpdate()
    {
        _delayedCursorState = true;
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _aS = GetComponent<AudioSource>();
        _pM = FindObjectOfType<PlayerManager>();
        _cursorMenu = GetComponentInChildren<CursorMenuManager>();

        CursorState = Enums.CursorState.Default;
        _moveTimer = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        _minClamp = new Vector2(Boundaries.points[1].x + .5f, Boundaries.points[1].y + .5f);
        _maxClamp = new Vector2(Boundaries.points[3].x - .5f, Boundaries.points[3].y - .5f);

        StartCoroutine(CreateMinimapIcon());
        //try
        //{
        //    var uiParent = GameObject.FindGameObjectWithTag("UI");
        //    var minimapPanel = uiParent.FindObject("UnitIcons");
        //    _miniMapIcon = Instantiate(MiniMapIcon);
        //    _miniMapIcon.rectTransform.SetParent(minimapPanel.transform);
        //    _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
        //}
        //catch { Debug.Log("failed cursor minimap"); }

        _startPos = transform.position;
        _playerColor = _sR.color = Colors.Player1;
    }

    // Update is called once per frame
    void Update()
    {
        if (CursorState == Enums.CursorState.Selected && CurrentUnit == null)
            CursorState = Enums.CursorState.Default;

        if (_delayedCursorState && CurrentGridBlock.ActiveSpace != Enums.ActiveSpace.Inactive)
        {
            SetCursorState();
            _delayedCursorState = false;
        }

        if (_getUnit && CurrentGridBlock != null)
        {
            GetUnit(CurrentGridBlock);
            _getUnit = false;
        }

        if (_moveTimer <= 0 && _moveVector != Vector2.zero)
        {
            Vector2 tmpPos = transform.position.V2() + _moveVector;
            tmpPos = tmpPos.Clamp(_minClamp, _maxClamp);

            if (tmpPos != transform.position.V2())
            {
                OnCursorMoveEvent?.Invoke(this, new CursorMoveEventArgs(transform.position));
                transform.position = tmpPos;
                _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
                _aS.Play(Sound_Move);
            }

            _moveTimer = MoveTimer;
        }
        else
            _moveTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        var grid = gO.GetComponent<GridBlock>();

        if (grid != null)
        {
            CurrentGridBlock = grid;
            GetUnit(grid);

            if (CursorState != Enums.CursorState.Default/* && CursorState != Enums.CursorState.OnlyAttack*/)
                SetCursorState();
        }
        else if (gO.name.StartsWith("UnitOff"))
        {
            Log("Cursor: Unit cooldown object detected");
            if (CurrentGridBlock == null)
            {
                Log("Cursor: Get Unit after cooldown... later");
                _getUnit = true;
            }
            else
            {
                GetUnit(CurrentGridBlock);
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

    private IEnumerator CreateMinimapIcon()
    {
        yield return new WaitUntil(() => _pM.FullGrid != null);

        try
        {
            _miniMapIcon = Instantiate(MiniMapIcon);
            _miniMapIcon.rectTransform.SetParent(_pM.Minimap_UnitIcons.transform);
            float squareSize = _pM.MinimapSquareSize;
            _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, squareSize);
            _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, squareSize);
            _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
            _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Idle : Colors.Enemy_Idle;
        }
        catch (Exception ex)
        {
            Log(ex.Message);
        }
    }

    private void SetCursorState()
    {
        Enums.CursorMenuState newState = Enums.CursorMenuState.None;

        if (CurrentGridBlock.IsCurrentUnitEnemy(Player))
        {
            CursorState = Enums.CursorState.Attack;
            newState = Enums.CursorMenuState.Attack;
        }
        else if (CurrentGridBlock.Unpassable)
        {
            CursorState = Enums.CursorState.Null;
        }
        else
        {
            CursorState = Enums.CursorState.Selected;
            if (CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Move)
            {
                if (!_onlyAttack)
                    _moves = _pM.CreatePath(_orgGridBlock, CurrentGridBlock).ToList();
                if (!CurrentGridBlock.IsOccupied)
                    newState = Enums.CursorMenuState.Move | CheckForDistanceAttack();
                if ((!CurrentGridBlock.IsOccupied || CurrentGridBlock.CurrentUnit == CurrentUnit) && CurrentGridBlock.Type == Enums.GridBlockType.Tree)
                {
                    if (CurrentUnit.IsHidden && !newState.HasFlag(Enums.CursorMenuState.Move))
                        newState |= Enums.CursorMenuState.Reveal;
                    else
                        newState |= Enums.CursorMenuState.Hide;
                }
            }
            else if (CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Attack)// && !_onlyAttack)
            {
                newState = CheckForDistanceAttack();// Enums.CursorMenuState.Attack;
            }
        }

        _cursorMenu.State = newState;
    }

    private Enums.CursorMenuState CheckForDistanceAttack()
    {
        Vector2 checkPos = _onlyAttack ? _attackFromGridBlock.Position : _orgGridBlock.Position;
        if (CurrentUnit.MaxAttackDistance > 1 && CurrentGridBlock.Position.GridDistance(checkPos) >= CurrentUnit.MinAttackDistance)
            return Enums.CursorMenuState.Attack;
        else
            return Enums.CursorMenuState.None;
    }

    private bool CheckForObstacle(GameObject gO)
    {
        if (CurrentUnit != null)
            // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
            return CursorState == Enums.CursorState.Selected && (gO.CompareTag("Unit") && gO.transform.position != CurrentUnit.transform.position);

        return false;
    }

    private void GetUnit(GridBlock gB)
    {
        if (CursorState == Enums.CursorState.Default)
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

            CurrentUnit = tmpUnit;
        }
    }

    private void OnCurrentUnitInterupt()
    {
        Log($"Current unit {CurrentUnit.name} was interupted");
        ResetCursor();
    }

    private void ResetCursor(bool resetUnit = true)
    {
        Log("Cursor Reset");
        CursorState = Enums.CursorState.Default;
        _cursorMenu.ResetPanels();
        _cursorMenu.State = Enums.CursorMenuState.None;
        _pM.ResetBlockGrid();
        _attackFromGridBlock = null;
        _onlyAttack = false;

        if (CurrentUnit != null && resetUnit)
        {
            CurrentUnit.OnUnitInterupt -= OnCurrentUnitInterupt;
            if (CurrentUnit.Moving)
            {
                var pathColor = Colors.GetPathColor();
                var savedPath = _moves?.Take(_moves.Count - 1).ToList();
                savedPath?.ForEach(m => m.Path_Save(CurrentUnit, pathColor));
            }
            CurrentUnit = null;
            Log("Current Unit null");
        }
        _moves = null;
    }

    public void Move_UpDown(InputAction.CallbackContext context)
    {
        //Debug.Log($"UpDown | Started: {context.started} | Performed: {context.performed} | Canceled: {context.canceled} | MoveVec: {context.ReadValue<Vector2>()}");

        if (context.started)
            _moveValue = context.ReadValue<float>();

        if (CursorState != Enums.CursorState.CursorMenu)
        {
            _moveVector.y = context.ReadValue<float>();
        }
        else if(context.performed)
        {
            _cursorMenu.SelectNextAvailablePanel(-1 * (int)_moveValue); // Flip value sign as going 'down' would be going positively through the list
        }
    }

    public void Move_LefRight(InputAction.CallbackContext context)
    {
        //Debug.Log($"LeftRight | Started: {context.started} | Performed: {context.performed} | Canceled: {context.canceled} | MoveVec: {context.ReadValue<Vector2>()}");

        if (CursorState != Enums.CursorState.CursorMenu)
        {
            _moveVector.x = context.ReadValue<float>();
        }
    }

    // Selects the Unit closests to the cursor.
    public void QuickSelect(InputAction.CallbackContext context)
    {
        if (context.performed && _cursorState == Enums.CursorState.Default)
        {
            UnitController useUC;
            if (CurrentUnit)
                useUC = CurrentUnit;
            else
                useUC = _quickSelectUnit;

            _quickSelectUnit = _pM.GetNextUnit(useUC);
            if (_quickSelectUnit != null)
            {
                transform.position = _quickSelectUnit.transform.position;
                _aS.Play(Sound_Move);
            }
        }
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (CursorState != Enums.CursorState.Default)
        {
            if (_onlyAttack)//CursorState == Enums.CursorState.OnlyAttack)
            {
                _onlyAttack = false;
                _cursorMenu.ResetPanels();
                transform.position = _attackFromGridBlock.Position;
                _attackFromGridBlock = null;
                _pM.ResetBlockGrid();
                CursorState = Enums.CursorState.Selected;
                InitializeGrid(_orgGridBlock);
                _delayedCursorState = true;
            }
            else if (CursorState == Enums.CursorState.CursorMenu)
            {
                CursorState = Enums.CursorState.Selected;
                _cursorMenu.ResetPanels();
            }
            else if (transform.position.V2() == _startPos)
            {
                SelectUnit(false);
                ResetCursor(resetUnit: false);
            }
            else
            {
                transform.position = _orgGridBlock.Position;
            }

            _aS.Play(Sound_Deselect);
        }
    }

    public void Select(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (CursorState == Enums.CursorState.Default && CurrentUnit && Position == CurrentUnit.CurrentGridBlock.Position)
        {
            CursorState = Enums.CursorState.Selected;
            _orgGridBlock = CurrentGridBlock;

            if (!CurrentUnit.Moved)
            {
                _startPos = transform.position;
            }

            SelectUnit(true);

            _aS.Play(Sound_Select);

            CurrentGridBlock.UseMoveAnimation = true;
            InitializeGrid(CurrentGridBlock);

            SetCursorState();
            _moves = new List<MovePoint>() { new MovePoint(CurrentGridBlock) };

        }
        else if (_cursorMenu.VisiblePanels == 1)
        {
            if (!_onlyAttack && CursorState == Enums.CursorState.Selected && CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Move && (CurrentGridBlock.CurrentUnit == null || !CurrentGridBlock.CurrentUnit.AtDestination))
            {
                SetAfterMoveAction();
            }
            else if (_cursorMenu.IsPanelSelected && 
                     (_cursorMenu.SelectedPanel.ActiveState == Enums.CursorMenuState.Hide ||
                      _cursorMenu.SelectedPanel.ActiveState == Enums.CursorMenuState.Reveal))
            {
                UnitMove(_cursorMenu.SelectedPanel.ActiveState);
            }
            else if (_onlyAttack/*CursorState == Enums.CursorState.OnlyAttack*/ && CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Move)
            {
                _cursorMenu.SelectFirstPanel();
                UnitMove();
            }
            else if (_onlyAttack)//CursorState == Enums.CursorState.OnlyAttack)
            {
                _cursorMenu.SelectFirstPanel();
                CurrentUnit.Target = CurrentGridBlock.ToMovePoint();
                CurrentUnit.MoveTo(_moves, false);

                _aS.Play(Sound_Attack);
                SelectUnit(false);
                ResetCursor();
            }
            else if (CursorState == Enums.CursorState.Attack)
            {
                _cursorMenu.SelectFirstPanel();
                MoveToAttack();
            }
        }
        else if (_cursorMenu.Active && _cursorMenu.VisiblePanels > 1)
        {
            switch (_cursorMenu.SelectedPanel.ActiveState)
            {
                case Enums.CursorMenuState.Move:
                    if (_onlyAttack)
                        UnitMove();
                    else
                        SetAfterMoveAction();
                    break;
                case Enums.CursorMenuState.Attack:
                    MoveToAttack();
                    break;
                case Enums.CursorMenuState.Hide:
                case Enums.CursorMenuState.Reveal:
                    UnitMove(_cursorMenu.SelectedPanel.ActiveState);
                    break;
            }
        }
        else if (CurrentGridBlock == _attackFromGridBlock)
        {
            UnitMove();
        }
        else if (CursorState != Enums.CursorState.Default && CursorState != Enums.CursorState.CursorMenu && _cursorMenu.VisiblePanels > 0)
        {
            CursorState = Enums.CursorState.CursorMenu;
            _cursorMenu.SelectFirstPanel();
        }
    }

    private void UnitMove(Enums.CursorMenuState state = Enums.CursorMenuState.None)
    {

        if (state == Enums.CursorMenuState.Hide)
            _moves.Add(CurrentGridBlock.ToMovePoint(true));
        else if (state == Enums.CursorMenuState.Reveal)
            _moves.Add(CurrentGridBlock.ToMovePoint(false));


        CurrentUnit.MoveTo(_moves);

        _aS.Play(Sound_Select);
        SelectUnit(false);
        ResetCursor();
    }

    private void SetAfterMoveAction()
    {
        _pM.ActiveGrid_Hide();
        _onlyAttack = true;
        _attackFromGridBlock = CurrentGridBlock;
        SetCursorState();
        //CursorState = Enums.CursorState.OnlyAttack;
        //_cursorMenu.State = Enums.CursorMenuState.Attack;
        _pM.ResetBlockGrid();
        CurrentGridBlock.UseMoveAnimation = false;
        InitializeGrid(CurrentGridBlock);
        _aS.Play(Sound_Select);
    }

    private void MoveToAttack()
    {
        var bestGrid = FirstAvaiableMovePoint();
        if (bestGrid == null)
            return;
        else
            _moves.RemoveAllAfter(bestGrid);

        CurrentUnit.Target = CurrentGridBlock.ToMovePoint();
        CurrentUnit.MoveTo(_moves, _attackFromGridBlock == null);

        _aS.Play(Sound_Attack);
        SelectUnit(false);
        ResetCursor();
    }

    private MovePoint FirstAvaiableMovePoint()
    {
        var gBList = _moves.Where(gB => 
            (gB.CurrentUnit == null || gB.CurrentUnit == CurrentUnit) && 
            gB.Position.GridDistance(CurrentGridBlock.Position) <= CurrentUnit.MaxAttackDistance).ToList();
        var gB = gBList.FirstOrDefault();
        return gB;
    }

    private void InitializeGrid(GridBlock origin)
    {
        int moveDistance = /*CursorState == Enums.CursorState.OnlyAttack*/_onlyAttack ? 0 : CurrentUnit.AdjustedMoveDistance;

        origin.SetGrid(
            null,
            CurrentUnit.FavorableTerrain,
            moveDistance,
            CurrentUnit.AdjustedMinAttackDistance,
            CurrentUnit.AdjustedMaxAttackDistance
        );
    }

    private void SelectUnit(bool select)
    {
        CurrentUnit.Select(select);
        if (select)
            CurrentUnit.OnUnitInterupt += OnCurrentUnitInterupt;
        else
            CurrentUnit.OnUnitInterupt -= OnCurrentUnitInterupt;
    }

    public void Log(string msg)
    {
        UnitManager.Log($"{gameObject.name} | {msg}");
    }

    public void LogError(string msg)
    {
        throw new NotImplementedException();
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
