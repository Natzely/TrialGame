using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridBlock : MonoBehaviour, ILog
{
    public Enums.GridBlockType Type;
    public UnitController CurrentUnit { get; private set; }
    public GameObject MoveSpace;
    public GameObject AttackSpace;
    public GameObject ActivePath;
    public GameObject SavedPath;
    public Image MinimapTile;
    public int SpeedCost = 0;
    public bool Unpassable = false;
    public bool IsDestination = false;

    public Enums.GridStatusEffect StatusEffects { get { return _statuses; } }
    public PlayerManager PlayerManager { get; set; }
    public CursorController Cursor { get; set; }
    public Vector2 GridPosition { get; set; }
    public Vector2 Position { get { return transform.position; } }
    public GridNeighbors Neighbors { get; private set; }
    public Enums.ActiveSpace ActiveSpace { get { return _gridParams.ActiveSpace; } }
    public UnitController PlayerActiveUnit { get { return _gridParams.UnitController; } }
    public MoveGridParams GridParams { get { return _gridParams; } }
    public bool UseMoveAnimation { get; set; }
    public bool IsOccupied { get { return CurrentUnit; } }
    public bool IsSpaceActive { get { return _gridParams != null && _gridParams.IsSpaceActive; } }
    public int PlayerMoveDistance { get { return _gridParams.MoveDistance; } }
    public int PlayerAttackDistance { get { return _gridParams.MaxAttackDistance; } }
    private int MovementCost { get { return Cursor.CurrentUnit.CheckGridMoveCost(Type); } }

    private Enums.GridStatusEffect _statuses;
    private MoveGridParams _gridParams;
    private GameObject _triggerObject;
    private BoxCollider2D _bC;
    private Dictionary<UnitController, Path_Saved> _savedPaths;
    private List<UnitController> _unitsMovingThrough;
    private Path_Active _currentActivePath;
    private bool _initialized;

    public (UnitController uC, int MoveDistance, int MaxAttackDistance) GetPlayerMoveParams()
    {
        if (_gridParams == null)
            return (null, -1, -1);
        else
            return (_gridParams.UnitController, _gridParams.MoveDistance, _gridParams.MaxAttackDistance);
    }

    public IEnumerable<GridBlock> AvailableAttackSpace(GridBlock behindGrid, int unitAttackDistance)
    {
        var neightbors = Neighbors.AvailableNeighbors(behindGrid.Position).ToList();
        foreach (var n in neightbors)
        {
            if (n.Position.GridDistance(behindGrid.Position) <= unitAttackDistance)
                yield return n;
        }
    }

    public void CheckGrid(GridBlock moveFrom)
    {
        var (uC, moveDis, maxAttackDis) = moveFrom.GetPlayerMoveParams();
        if (uC && uC.FavorableTerrain != null)
        {
            var tempMove = moveDis - (uC.FavorableTerrain.Contains(moveFrom.Type) ? 1 : moveFrom.MovementCost);
            var tempAttack = maxAttackDis - 1;
            UseMoveAnimation = moveFrom.UseMoveAnimation;

            if (tempMove > _gridParams.MoveDistance || tempAttack > _gridParams.MaxAttackDistance)
            {
                 SetGrid(moveFrom, uC);
                if (Cursor.CurrentGridBlock == this || PlayerManager.PlayerInfo.MovementPathContains(this))
                    Cursor.CurrentGridUpdate();
            }
        }
    }

    private bool MovingFromRageBox(GridBlock moveFrom, UnitController uC)
    {
        return moveFrom.StatusEffects.HasFlag(Enums.GridStatusEffect.Rage) &&
               !_statuses.HasFlag(Enums.GridStatusEffect.Rage) &&
               uC.StatusEffects.HasFlag(Enums.UnitStatusEffect.Rage);
    }

    public void SetGrid(GridBlock moveFrom, UnitController uC, bool onlyAttack = false)
    {
        _gridParams.Update(moveFrom, uC);
         
        if (Unpassable || (CurrentUnit && CurrentUnit.IsEnemy(Enums.Player.Player1)) ||
            MovingFromRageBox(moveFrom, uC) || onlyAttack ||
            _unitsMovingThrough.Any(uC => uC != null && uC.IsEnemy(Enums.Player.Player1)))
            _gridParams.MoveDistance = -1;

        if (gameObject.layer == LayerMask.NameToLayer("GridBlock_Wall") &&
            !uC.gameObject.CompareTag("Unit_Atlatl"))
            _gridParams.MaxAttackDistance = 0;

        if(moveFrom != this && _gridParams.MoveDistance >= 0)
            _gridParams.MoveDistance = moveFrom.GridParams.MoveDistance - (uC.FavorableTerrain.Contains(this.Type) ? 1 : MovementCost); // If this terrain is favorable to the unit, only subtract one.

        if (_gridParams.MoveDistance >= 0 || onlyAttack)
        {
            _gridParams.ActiveSpace = Enums.ActiveSpace.Move;
            _gridParams.ShowMoveSpace(moveFrom);
        }
        else if (_gridParams.MaxAttackDistance > 0)
        {
            _gridParams.ActiveSpace = Enums.ActiveSpace.Attack;
            if (_gridParams.GridStart.Position.GridDistance(this.Position) > _gridParams.MinAttackDistance) // Check if the unit needs to be a certain distance away to attack.
                _gridParams.ShowAttackSpace(moveFrom); // Create red square if it is.

            _gridParams.MaxAttackDistance = _gridParams.MaxAttackDistance - 1;
        }
        else
            _gridParams.Reset();

        PlayerManager.UpdateBlockGrid(GridPosition, this, _gridParams.IsSpaceActive);
    }

    public void Disable()
    {
        _gridParams.Reset();
    }

    public IEnumerable<GridBlock> GetRangeSpaces(GridBlock start, int minDis, GridBlock target = null, List<OrderedGridBlock> gridDis = null)
    {
        bool org = false;
        if (gridDis == null)
        {
            gridDis = new List<OrderedGridBlock>();
            target = this;
            org = true;
        }

        var orderedNeighbors = target.Neighbors.OrderByDistance(start, minDis <= 1).ToList();
        var nonUsedNeighbors = orderedNeighbors.Except(gridDis.Select(g => g.GridBlock).ToList()).ToList();
        foreach (var n in nonUsedNeighbors)
        {
            var dis = n.Position.GridDistance(start.Position);
            gridDis.Add(new OrderedGridBlock(n, dis));
        }

        if (minDis > 1)
        {
            minDis--;
            foreach (GridBlock g in nonUsedNeighbors)
            {
                g.GetRangeSpaces(start, minDis, g, gridDis);
            }
        }

        if (org && gridDis != null && gridDis.Count > 0)
        {
            var orderedList = gridDis.OrderBy(g => g.OrderedValue).ToList();
            var lowestVal = orderedList.FirstOrDefault().OrderedValue;
            var trimmedList = orderedList.Where(x => x.OrderedValue == lowestVal);
            return trimmedList.Select(tL => tL.GridBlock);
        }
        else
            return null;
    }

    public bool IsCurrentUnitEnemy(Enums.Player player)
    {
        return CurrentUnit &&
               CurrentUnit.Player != player;// &&
                                            //!CurrentUnit.AlliedWith.Contains(player);
    }

    public void UpdatePathState(Vector2 cDir, Vector2? nDir)
    {
        if (!_currentActivePath)
            CreatePathBlock();

        _currentActivePath.UpdatePathState(cDir, nDir);
    }

    public void Path_Save(UnitController unit, Color color)
    {
        var savedPath = Instantiate(SavedPath, transform.position, Quaternion.identity);
        var spScript = savedPath.GetComponent<Path_Saved>();
        var spmScript = savedPath.GetComponent<Path_Saved_Mask>();
        spScript.SetColor(color);
        spScript.SetPathDirection(_currentActivePath.NextDirection);
        spmScript.Unit = unit;
        _savedPaths[unit] = spScript;
    }

    public void Path_Delete(UnitController unit)
    {
        if (_savedPaths.ContainsKey(unit))
        {
            if (_savedPaths[unit])
            {
                Destroy(_savedPaths[unit].gameObject);
                _savedPaths[unit] = null;
                _savedPaths.Remove(unit);
            }
        }
    }

    public void SetCurrentUnit(UnitController uc)
    {
        if (CurrentUnit == null)
            CurrentUnit = uc;
    }

    public void ResetCurrentUnit(UnitController uC)
    {
        if (CurrentUnit == uC)
            CurrentUnit = null;
    }

    public IEnumerable<UnitController> GetAlliedUnits(Enums.Player player)
    {
        var alliedUnits = _unitsMovingThrough.Where(u => u.Player == player).ToList();
        if (CurrentUnit != null && CurrentUnit.Player == player)
            alliedUnits.Add(CurrentUnit);
        return alliedUnits;
    }

    public MovePoint ToMovePoint(bool hide = false)
    {
        return new MovePoint(this, hide);
    }

    void Awake()
    {
        _unitsMovingThrough = new List<UnitController>();
        _bC = GetComponent<BoxCollider2D>();
    }

    public void Initialize()
    {
        if (_gridParams == null)
            _gridParams = new MoveGridParams(this, PlayerManager, AttackSpace, MoveSpace);

        StartCoroutine(CreateMinimapIcon());

        _savedPaths = new Dictionary<UnitController, Path_Saved>();
        _gridParams.Reset();

        Neighbors = new GridNeighbors(this);

        _initialized = true;
    }

    void Start()
    {
        gameObject.name = String.Format(Strings.GridblockName, Position.x, Position.y);

        var cursorBoundaries = GameObject.Find("CursorBoundaries");
        var pCollider = cursorBoundaries.GetComponent<PolygonCollider2D>();

        if (!Position.InsideSquare(pCollider.points[1], pCollider.points[3]))
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (_initialized)
        {
            CleanUpUnits();

            if (PlayerManager && !PlayerManager.PlayerInfo.MovementPathContains(this) && _currentActivePath && _currentActivePath.Show)// && _pM.PlayerInfo.SelectedUnit == null)// && PathBlocks.ContainsKey(_pM.PlayerInfo.SelectedUnit))
            {
                _currentActivePath.Show = false;
            }

            var bestNeighbor = Neighbors?.GetBestMoveNeighbor();
            if (bestNeighbor != null)
                CheckGrid(bestNeighbor);
        }
    }

    private void CleanUpUnits()
    {
        if (CurrentUnit != null && (CurrentUnit.IsDestroyed || !CurrentUnit.Position.InsideSquare(_bC.bounds.min, _bC.bounds.max)))
            ResetCurrentUnit(CurrentUnit);

        var unitsCopy = new List<UnitController>(_unitsMovingThrough);
        foreach(UnitController uC in unitsCopy)
        {
            if (!uC || uC.IsDestroyed || !_bC.IsTouching(uC.BoxCollider))
                _unitsMovingThrough.Remove(uC);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        var uC = colObj.GetComponent<UnitController>();
        var sG = colObj.GetComponent<StatusEffect_Giver>();
        var damager = colObj.GetComponent<Damager>();

        if (_gridParams == null)
            _gridParams = new MoveGridParams(this, PlayerManager, AttackSpace, MoveSpace);

        if (uC != null)
        {
            _unitsMovingThrough.Add(uC);

            if (!_gridParams.MoveSpace)
                _gridParams.CreateSpaces();

            if (_gridParams.MoveSpace.Active && uC.Player != Enums.Player.Player1)
            {
                if (PlayerManager.PlayerInfo.MovementPathContains(this))
                    Cursor.EnemyInPath(this);

                UpdateGrid();
                PlayerManager.ActiveGrid_Update(this);
            }
        }

        if(sG)
            _statuses = sG.GridStatusEffects;

        if(damager)
            Destroy(damager.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        var uC = colObj.GetComponent<UnitController>();
        if(uC != null)
        {
            if (_unitsMovingThrough.Contains(uC))
                _unitsMovingThrough.Remove(uC);
        }
    }

    private IEnumerator CreateMinimapIcon()
    {
        yield return new WaitUntil(() => PlayerManager && PlayerManager.FullGrid != null);

        var _miniMapIcon = Instantiate(MinimapTile);
        _miniMapIcon.rectTransform.SetParent(PlayerManager.Minimap_TileIcons.transform);
        float squareSize = PlayerManager.MinimapSquareSize;
        _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, squareSize);
        _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, squareSize);
        _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
    }

    private void UpdateGrid()
    {
        SetGrid(null, _gridParams.UnitController);
    }

    private void CreatePathBlock()
    {
        var o = Instantiate(ActivePath, this.transform.position, Quaternion.identity);
        _currentActivePath = o.GetComponent<Path_Active>();
        _currentActivePath.ParentGridBlock = this;
    }

    private T GetGridBlockItemScript<T>(GridBlock parent, GameObject gO)
    {
        var o = Instantiate(gO, parent.transform.position, Quaternion.identity);
        return o.GetComponent<T>();
    }

    public void Log(string msg)
    {
        DebugLogger.Instance.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new NotImplementedException();
    }

    public class MoveGridParams
    {
        public Enums.ActiveSpace ActiveSpace { get; set; }
        public UnitController UnitController { get { return _uC; } }
        public GridBlock GridStart { get; set; }
        public UnitManager UnitManager { get; set; }
        public AttackSpace AttackSpace { get; set; }
        public MoveSpace MoveSpace { get; set; }
        public List<Enums.GridBlockType> FavorableTerrain { get { return _uC.FavorableTerrain; } }
        public int MoveDistance
        {
            get { return _uC ? _uC.MoveDistance - _moveMod : -1; }
            set
            {
                _moveMod = (_uC ? _uC.MoveDistance : -1) - value;
            }
        }
        public int MaxAttackDistance
        {
            get { return _uC ? _uC.MaxAttackDistance - _maxAttackMod : -1; }
            set
            {
                _maxAttackMod = (_uC ? _uC.MaxAttackDistance : -1) - value;
            }
        }
        public int MinAttackDistance
        {
            get { return _uC ? _uC.MinAttackDistance - _minAttackMod : -1; }
            set 
            {
                _minAttackMod = (_uC ? _uC.MinAttackDistance : -1) - value;
            }
        }

        private GridBlock _parent;
        private PlayerManager _pM;
        private UnitController _uC;
        private GameObject _object_AS;
        private GameObject _object_MS;
        private int _moveMod;
        private int _maxAttackMod;
        private int _minAttackMod;

        public bool IsSpaceActive
        {
            get { return ActiveSpace != Enums.ActiveSpace.Inactive; }
        }

        public void Update(GridBlock moveFrom, UnitController uC)
        {
            if (moveFrom.GridParams.UnitController == null)
            {
                _uC = uC;
                _moveMod = 0;
                _maxAttackMod = 0;
                _minAttackMod = 0;
                GridStart = _parent;
            }
            else
                _uC = uC;

            MoveDistance = moveFrom.GridParams.MoveDistance;
            MaxAttackDistance = moveFrom.GridParams.MaxAttackDistance;
            MinAttackDistance = moveFrom.GridParams.MinAttackDistance;
            GridStart = moveFrom.GridParams.GridStart;
        }

        public void Reset()
        {
            ActiveSpace = Enums.ActiveSpace.Inactive;
            GridStart = null;
            if(AttackSpace) AttackSpace.Disable();
            if(MoveSpace) MoveSpace.Disable();
            _moveMod = 99;
            _maxAttackMod = 99;
            _minAttackMod = 99;
            _uC = null;
        }

        public void ShowMoveSpace(GridBlock moveFrom)
        {
            Vector2 moveFromPos;
            if (moveFrom == null)
                moveFromPos = MoveSpace.transform.position;
            else
                moveFromPos = moveFrom.Position;

            if (!MoveSpace || !AttackSpace)
                CreateSpaces();

            ShowSpace(MoveSpace, AttackSpace, moveFromPos);
        }

        public void ShowAttackSpace(GridBlock moveFrom)
        {
            Vector2 moveFromPos;
            if (moveFrom == null)
                moveFromPos = MoveSpace.transform.position;
            else
                moveFromPos = moveFrom.Position;

            if (!MoveSpace || !AttackSpace)
                CreateSpaces();

            ShowSpace(AttackSpace, MoveSpace, moveFromPos);
        }

        public void ShowSpace(GridSpace eSpace, GridSpace dSpace, Vector2? moveFrom)
        {
            if(!eSpace.Active)
            {
                dSpace.Disable();
                eSpace.Enable(moveFrom);
            }
        }

        public void CreateSpaces()
        {
            if (!MoveSpace)
            {
                var o = Instantiate(_object_MS, _parent.transform.position, Quaternion.identity);
                MoveSpace = o.GetComponent<MoveSpace>();
                MoveSpace.ParentGridBlock = _parent;
                MoveSpace.PlayerManager = _pM;
                MoveSpace.Disable();
            }
            if (!AttackSpace)
            {
                var o = Instantiate(_object_AS, _parent.transform.position, Quaternion.identity);
                AttackSpace = o.GetComponent<AttackSpace>();
                AttackSpace.ParentGridBlock = _parent;
                AttackSpace.PlayerManager = _pM;
                AttackSpace.Disable();
            }
        }

        public override bool Equals(object obj)
        {
            var checkP = ((int, int, int))obj;
            return MoveDistance == checkP.Item1 && MinAttackDistance == checkP.Item2 && MaxAttackDistance == checkP.Item3;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public MoveGridParams(GridBlock parent, PlayerManager playerManager, GameObject AttackSpace, GameObject MoveSpace)
        {
            _parent = parent;
            _pM = playerManager;
            _object_AS = AttackSpace;
            _object_MS = MoveSpace;
            _maxAttackMod = 99;
            _moveMod = 99;
        }
    }
}
