using System;
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
    [SerializeField] private Image MinimapCursor;
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
            _cameraController?.UpdateZoom(_cursorState);
        }
    }

    public Vector2 Position { get { return transform.position; } }

    private GridBlock _orgGridBlock;
    private PlayerManager _pM;
    private UnitController _quickSelectUnit;

    private Animator _animator;
    private SpriteRenderer _sR;
    private AudioSource _aS;
    private Image _miniMapIcon;
    private List<GridBlock> _moves;
    private Vector2 _startPos;
    private Vector2 _moveVector;
    private Vector2 _maxClamp;
    private Vector2 _minClamp;
    private Color _playerColor;
    private bool _getUnit;
    private bool _delayedCursorState;
    private float _moveTimer;

    public void EnemyInPath(GridBlock gB)
    {
        var enemyIndex = _pM.PlayerInfo.MovementPath.IndexOf(gB);
        var playerIndex = _pM.PlayerInfo.MovementPath.IndexOf(CurrentUnit.CurrentGridBlock);

         if(playerIndex > enemyIndex)
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

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _aS = GetComponent<AudioSource>();
        _pM = FindObjectOfType<PlayerManager>();

        CursorState = Enums.CursorState.Default;
        _moveTimer = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        _minClamp = new Vector2(Boundaries.points[1].x + .5f, Boundaries.points[1].y + .5f);
        _maxClamp = new Vector2(Boundaries.points[3].x - .5f, Boundaries.points[3].y - .5f); 
        
        try
        {
            var uiParent = GameObject.FindGameObjectWithTag("UI");
            var minimapPanel = uiParent.FindObject("UnitIcons");
            _miniMapIcon = Instantiate(MinimapCursor);
            _miniMapIcon.rectTransform.SetParent(minimapPanel.transform);
            _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
        }
        catch { Debug.Log("failed cursor minimap"); }

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

        Log("Cursor: Trigger Enter");
        if (grid != null)
        {
            CurrentGridBlock = grid;
            GetUnit(grid);

            if (CursorState != Enums.CursorState.OnlyAttack && CursorState != Enums.CursorState.Default)
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

    private void SetCursorState()
    {
        if (CurrentGridBlock.IsCurrentUnitEnemy(Player))
        {
            CursorState = Enums.CursorState.Attack;
        }
        else if (CurrentGridBlock.Unpassable)
        {
            CursorState = Enums.CursorState.Null;
        }
        else
        {
            CursorState = Enums.CursorState.Selected;
            if (CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Move)
                _moves = _pM.CreatePath(_orgGridBlock, CurrentGridBlock).ToList();
        }
    }

    private bool CheckForObstacle(GameObject gO)
    {
        if (CurrentUnit != null)
            // Check if we're moving a unit                        and we've hit a map collider     or a unit that isn't our moving unit
            return CursorState == Enums.CursorState.Selected && (gO.tag == "Unit" && gO.transform.position != CurrentUnit.transform.position);

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
        _pM.ResetBlockGrid();
        if (CurrentUnit != null && resetUnit)
        {
            CurrentUnit.OnUnitInterupt -= OnCurrentUnitInterupt;
            Log("Current Unit null");
            var pathColor = Colors.GetPathColor();
            var savedPath = _moves?.Take(_moves.Count - 1).ToList();
            savedPath?.ForEach(m => m.Path_Save(CurrentUnit, pathColor));
            CurrentUnit = null;
        }
        _moves = null;
    }

    public void Move_UpDown(InputAction.CallbackContext context)
    {
        //Debug.Log($"UpDown | Started: {context.started} | Performed: {context.performed} | Canceled: {context.canceled} | MoveVec: {context.ReadValue<Vector2>()}");

        _sR.color = new Color(0, 0, 1, .5f);
        _moveVector.y = context.ReadValue<float>();
    }

    public void Move_LefRight(InputAction.CallbackContext context)
    {
        //Debug.Log($"LeftRight | Started: {context.started} | Performed: {context.performed} | Canceled: {context.canceled} | MoveVec: {context.ReadValue<Vector2>()}");

        _sR.color = new Color(1, 0, 0, .5f);
        _moveVector.x = context.ReadValue<float>();
    }

    // Selects the Unit closests to the cursor.
    public void QuickSelect(InputAction.CallbackContext context)
    {
        if (context.performed && _cursorState == Enums.CursorState.Default)
        {
            _quickSelectUnit = _pM.GetNextUnit(CurrentUnit ?? _quickSelectUnit);
            if (_quickSelectUnit != null)
            {
                transform.position = _quickSelectUnit.transform.position;
                _aS.Play(Sound_Move);
            }
        }
    }

    public  void Cancel(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (CursorState != Enums.CursorState.Default)
        {
            if(CursorState == Enums.CursorState.OnlyAttack)
            {
                _pM.ResetBlockGrid();
                CursorState = Enums.CursorState.Selected;
                InitializeGrid(_orgGridBlock);
                _delayedCursorState = true;
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

        if (CursorState == Enums.CursorState.Default && CurrentUnit != null)
        {
            CursorState = Enums.CursorState.Selected;
            _orgGridBlock = CurrentGridBlock;

            if (Position == CurrentUnit.Position)
            {
                if (!CurrentUnit.Moved)
                {
                    _startPos = transform.position;
                }

                SelectUnit(true);

                _aS.Play(Sound_Select);

                CurrentGridBlock.UseMoveAnimation = true;
                InitializeGrid(CurrentGridBlock);


                _moves = new List<GridBlock>() { CurrentGridBlock };
            }
        }
        else if (CursorState == Enums.CursorState.Selected && CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Move && (CurrentGridBlock.CurrentUnit == null || !CurrentGridBlock.CurrentUnit.AtDestination))
        {
            _pM.ActiveGrid_Hide();
            CursorState = Enums.CursorState.OnlyAttack;
            _pM.ResetBlockGrid();
            CurrentGridBlock.UseMoveAnimation = false;
            InitializeGrid(CurrentGridBlock);
            _aS.Play(Sound_Select);
        }
        else if (CursorState == Enums.CursorState.OnlyAttack && CurrentGridBlock.ActiveSpace == Enums.ActiveSpace.Move)
        {
            CurrentUnit.MoveTo(_moves);

            _aS.Play(Sound_Attack);
            SelectUnit(false);
            ResetCursor();
        }
        else if (CursorState == Enums.CursorState.Attack)
        {
            var bestGrid = FirstAvaiableGridBlock();
            if (bestGrid == null)
                return;
            else
                _moves.RemoveAllAfter(bestGrid);

            CurrentUnit.Target = CurrentGridBlock;
            CurrentUnit.MoveTo(_moves, true);
            
            _aS.Play(Sound_Attack);
            SelectUnit(false);
            ResetCursor();
        }
        else if(CursorState == Enums.CursorState.OnlyAttack)
        {
            CurrentUnit.Target = CurrentGridBlock;
            CurrentUnit.MoveTo(_moves, false);

            _aS.Play(Sound_Attack);
            SelectUnit(false);
            ResetCursor();
        }
        //else if (!CurrentUnit.Moving && /*!CurrentUnit.Moved &&*/ transform.position.V2() != _startPos && CurrentGridBlock.ActivePlayerSpace != Enums.ActiveSpace.Inactive &&
        //    (CurrentGridBlock.IsCurrentUnitEnemy(Player) || 
        //    (CurrentGridBlock.ActivePlayerSpace == Enums.ActiveSpace.Move && CurrentGridBlock.CurrentUnit == null) //||
        //    /*(_attackSpace && (CurrentGridBlock.CurrentUnit == null || CurrentGridBlock.IsCurrentUnitEnemy(Player)))*/
        //    ))
        //{
        //    List<GridBlock> backupSpaces = null;
        //    var target = CurrentGridBlock;
        //    double dis = 9999;
        //    if (target.CurrentUnit != null)// || _attackSpace)
        //    {
        //        dis = target.Position.GridDistance(CurrentUnit.Position);
        //        CurrentUnit.Target = target;
        //    }

        //    if (target.CurrentUnit == null && _moves?.Count > 1)// && !_attackSpace)
        //    {
        //        CurrentUnit.MoveTo(_moves);
        //    }
        //    else if (!CurrentUnit.Attacked) // If the unit has not attacked yet, check attack options
        //    {                                                                                                                // If attacking an empty space, subtrack a move because the cursor created a move to reach that space
        //        if (!CurrentUnit.Attacked && 
        //            ((dis >= CurrentUnit.MinAttackDistance && dis <= CurrentUnit.MaxAttackDistance) /*|| (_attackSpace && dis <= CurrentUnit.MaxAttackDistance)*/) && 
        //            _moves?.Count /*- (_attackSpace ? 1 : 0)*/ <= CurrentUnit.MaxAttackDistance) // If the unit is already in range of the target,
        //        {
        //            _aS.Play(Sound_Attack);
        //            CurrentUnit.CheckAttack(target, false);// _attackSpace);
        //        }
        //        else if (_moves?.Count > 1) // If the unit needs to move to attack
        //        {
        //            double lastPosDis = _moves.Last().Position.GridDistance(target.Position);
        //            if (lastPosDis < CurrentUnit.MinAttackDistance)
        //            {
        //                var lastGrid = _moves.Last();
        //                var orderedN = lastGrid.Neighbors.OrderByDistance(_orgGridBlock, true);
        //                var canMoveTo = orderedN.Where(n => n.ActiveSpace(Player) == Enums.ActiveSpace.Move).ToList();
        //                var newTargetGrid = canMoveTo.FirstOrDefault();

        //                if (newTargetGrid != null && _moves.Contains(newTargetGrid))
        //                {
        //                    int index = _moves.IndexOf(newTargetGrid) + 1; // Erase everything higher than the selected grid
        //                    if (index <= _moves.Count())
        //                        _moves.RemoveRange(index, _moves.Count() - index);
        //                }
        //                else if (newTargetGrid != null)
        //                {
        //                    _moves.Add(newTargetGrid);
        //                }
        //                else if (newTargetGrid == null)
        //                {
        //                    CurrentUnit.Target = null;
        //                    return;
        //                }
        //            }

        //            if (_moves?.Count > 0)
        //            {
        //                CurrentUnit.MoveTo(_moves);
        //            }
        //        }
        //    }
        //    else if ((backupSpaces = _orgGridBlock.AvailableAttackSpace(CurrentGridBlock, CurrentUnit.MaxAttackDistance).ToList()).Count > 0) // If the unit is too close to attack but there is room for it back up and attack
        //    {
        //        _aS.Play(Sound_Attack);
        //        CurrentUnit.MoveTo(new List<GridBlock>() { _orgGridBlock, backupSpaces.First() });
        //    }
        //    else // If non of the other conditions are met, then do nothing.
        //    {
        //        CurrentUnit.Target = null;
        //        return;
        //    }

        //    if (CurrentUnit.Target != null && ((target.CurrentUnit != null && target.CurrentUnit.IsEnemy(Player))))// || _attackSpace))
        //    {
        //        _aS.Play(Sound_Attack);
        //        SelectUnit(false);
        //        ResetCursor();
        //    }
        //}
        //else if ((CurrentUnit.Moving || CurrentUnit.Moved) && CurrentGridBlock.IsCurrentUnitEnemy(Player))
        //{
        //    _aS.Play(Sound_Attack);
        //    CurrentUnit.Target = CurrentGridBlock;
        //    SelectUnit(false);
        //    ResetCursor();
        //}
        //else if ((CurrentUnit.Moving || CurrentUnit.Moved))
        //{
        //    _aS.Play(Sound_Attack);
        //    SelectUnit(false);
        //    ResetCursor();
        //}
    }

    private GridBlock FirstAvaiableGridBlock()
    {
        var gB = _moves.FirstOrDefault(gB => (gB.CurrentUnit == null || gB.CurrentUnit == CurrentUnit) && gB.Position.GridDistance(CurrentGridBlock.Position) <= CurrentUnit.MaxAttackDistance);
        return gB;
    }

    private void InitializeGrid(GridBlock origin)
    {
        int moveDistance = CursorState == Enums.CursorState.OnlyAttack ? 0 : CurrentUnit.AdjustedMoveDistance;

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
