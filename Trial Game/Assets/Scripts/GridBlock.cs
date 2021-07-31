using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public bool UseMoveAnimation { get; set; }
    public bool IsOccupied { get { return CurrentUnit; } }
    public bool IsSpaceActive { get { return _gridParams.IsSpaceActive; } }
    public Vector2 GridPosition { get; set; }
    public Vector2 Position { get { return transform.position; } }
    public GridNeighbors Neighbors { get; private set; }
    public Enums.ActiveSpace ActiveSpace { get { return _gridParams.ActiveSpace; } }
    public int PlayerMoveDistance { get { return _gridParams.MoveDistance; } }
    public int PlayerAttackDistance { get { return _gridParams.MaxAttackDistance; } }

    private int MovementCost { get { return _cursor.CurrentUnit.CheckGridMoveCost(Type); } }

    private PlayerManager _pM;
    private PlayerParams _gridParams;
    private CursorController _cursor;
    private BoxCollider2D _bC;
    private Dictionary<UnitController, Path_Saved> _savedPaths;
    private List<UnitController> _unitsMovingThrough;
    private Path_Active _currentActivePath;
    private bool _gotNeighbors;

    public (List<Enums.GridBlockType> FavorableTerrain, int MoveDistance, int MinAttackDis, int MaxAttackDis) GetPlayerMoveParams()
    {
        if (_gridParams == null)
            return (new List<Enums.GridBlockType>(), -1, -1, -1);
        else
            return (_gridParams.FavorableTerrain, _gridParams.MoveDistance, _gridParams.MinAttackDistance, _gridParams.MaxAttackDistance);
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
        var (favorableTerrain, moveDistance, minAttackDis, maxAttackDis) = moveFrom.GetPlayerMoveParams();
        var tempMove = moveDistance - (favorableTerrain.Contains(this.Type) ? 1 : MovementCost);
        var tempAttack = maxAttackDis - 1;
        UseMoveAnimation = moveFrom.UseMoveAnimation;

        if (tempMove > _gridParams.MoveDistance || tempAttack > _gridParams.MaxAttackDistance)
        {
            SetGrid(moveFrom, favorableTerrain, moveDistance, minAttackDis, maxAttackDis);
            if (_cursor.CurrentGridBlock == this || _pM.PlayerInfo.MovementPathContains(this))
                _cursor.CurrentGridUpdate();
        }
    }

    public void SetGrid(GridBlock moveFrom, List<Enums.GridBlockType> favTerrain, int moveDistance, int minAttackDistance, int maxAttackDistance)
    {
        bool saveParams = true;

        if (Unpassable || (CurrentUnit && CurrentUnit.IsEnemy(Enums.Player.Player1)) || _unitsMovingThrough.Any(uC => uC != null && uC.IsEnemy(Enums.Player.Player1)))
            moveDistance = -1;

        if (moveFrom == null)
        {
            moveDistance += (favTerrain.Contains(this.Type) ? 1 : MovementCost);
        }

        moveDistance = Mathf.Clamp(moveDistance - (favTerrain.Contains(this.Type) ? 1 : MovementCost), -1, 9999); // If this terrain is favorable to the unit, only subtract one.

        if (moveDistance >= 0)
        {
            _gridParams.ActiveSpace = Enums.ActiveSpace.Move;

            _gridParams.ShowMoveSpace(moveFrom);
        }
        else if (maxAttackDistance > 0)
        {
            _gridParams.ActiveSpace = Enums.ActiveSpace.Attack;
            _gridParams.ShowAttackSpace(moveFrom);
            maxAttackDistance--;
        }
        else
        {
            _gridParams.Reset();
            saveParams = false;
        }

        //if (moveFrom == null)
        //    text.text = "";
        //text.text += $"{gameObject.name}::: moveDis:{moveDistance}, minAttack:{minAttackDistance}, maxAttack:{maxAttackDistance}, neighbors:{Neighbors.Count()} |||||| ";

        if (saveParams)
        {
            _gridParams.FavorableTerrain = favTerrain;
            _gridParams.MoveDistance = moveDistance;
            _gridParams.MinAttackDistance = minAttackDistance;
            _gridParams.MaxAttackDistance = maxAttackDistance;
        }

        _pM.UpdateBlockGrid(GridPosition, this, saveParams);
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
        _currentActivePath.UpdatePathState(cDir, nDir);
        _currentActivePath.Show = true;
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
        _gridParams = new PlayerParams();
        _unitsMovingThrough = new List<UnitController>();
        _bC = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        gameObject.name = String.Format(Strings.GridblockName, Position.x, Position.y);

        _currentActivePath = CreatePathBlock();
        var cursorBoundaries = GameObject.Find("CursorBoundaries");
        var pCollider = cursorBoundaries.GetComponent<PolygonCollider2D>();
        _cursor = FindObjectOfType<CursorController>();

        if (!Position.InsideSquare(pCollider.points[1], pCollider.points[3]))
        {
            Destroy(gameObject);
        }
        else
        {
            _pM = FindObjectOfType<PlayerManager>();

            StartCoroutine(CreateMinimapIcon());

            _savedPaths = new Dictionary<UnitController, Path_Saved>();
            CreateAttackSpace(AttackSpace);
            CreateMoveSpace(MoveSpace);
            _gridParams.Reset();
        }
    }

    void Update()
    {
        if (!_gotNeighbors)
        {
            Neighbors = new GridNeighbors(this);
            _gotNeighbors = true;
        }

        CleanUpUnits();

        if (!_pM.PlayerInfo.MovementPathContains(this) && _currentActivePath.Show)// && _pM.PlayerInfo.SelectedUnit == null)// && PathBlocks.ContainsKey(_pM.PlayerInfo.SelectedUnit))
        {
            _currentActivePath.Show = false;
        }

        var bestNeighbor = Neighbors?.GetBestMoveNeighbor();
        if (bestNeighbor != null)
            CheckGrid(bestNeighbor);
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
        if (uC != null)
        {
            Log($"{uC.gameObject.name} has entered");
            //if (!IsUnitLocked(uC) && uC.AtDestination)
            //    UnitLock(uC);
            //else
            //{
                Log($"Block unit locked");
                _unitsMovingThrough.Add(uC);
            //}

            if (_gridParams.MoveSpace.Active && uC.Player != Enums.Player.Player1)
            {
                if (_pM.PlayerInfo.MovementPathContains(this))
                    _cursor.EnemyInPath(this);

                UpdateGrid();
                _pM.ActiveGrid_Update(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        var uC = colObj.GetComponent<UnitController>();
        if(uC != null)
        {
            if (_unitsMovingThrough.Contains(uC))
                _unitsMovingThrough.Remove(uC);
            //ResetLock(uC);
        }
    }

    private IEnumerator CreateMinimapIcon()
    {
        yield return new WaitUntil(() => _pM.FullGrid != null);

        var uiParent = GameObject.FindGameObjectWithTag("UI");
        var minimapObject = uiParent.FindObject("MapTileIcons");
        var _miniMapIcon = Instantiate(MinimapTile);
        _miniMapIcon.rectTransform.SetParent(minimapObject.transform);
        float squareSize = _pM.MinimapSquareSize;
        _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, squareSize);
        _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, squareSize);
        _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
    }

    private void UpdateGrid()
    {
        SetGrid(null, _gridParams.FavorableTerrain, -1, _gridParams.MinAttackDistance, _gridParams.MaxAttackDistance);
    }

    private void CreateAttackSpace(GameObject space)
    {
        _gridParams.AttackSpace = GetGridBlockItemScript<AttackSpace>(this, space);
        _gridParams.AttackSpace.ParentGridBlock = this;
        _gridParams.AttackSpace.PlayerManager = _pM;
    }

    private void CreateMoveSpace(GameObject space)
    {
        _gridParams.MoveSpace = GetGridBlockItemScript<MoveSpace>(this, space);
        _gridParams.MoveSpace.ParentGridBlock = this;
        _gridParams.MoveSpace.PlayerManager = _pM;
    }

    private Path_Active CreatePathBlock()
    {
        var newPB = GetGridBlockItemScript<Path_Active>(this, ActivePath);
        newPB.ParentGridBlock = this;
        return newPB;
    }

    private T GetGridBlockItemScript<T>(GridBlock parent, GameObject gO)
    {
        var o = Instantiate(gO, parent.transform.position, Quaternion.identity);
        return o.GetComponent<T>();
    }

    public void Log(string msg)
    {
        UnitManager.Log($"{gameObject.name} | {msg}");
    }

    public void LogError(string msg)
    {
        throw new NotImplementedException();
    }

    public class PlayerParams
    {
        public Enums.ActiveSpace ActiveSpace { get; set; }
        public GridBlock GridStart { get; set; }
        public UnitManager UnitManager { get; set; }
        public AttackSpace AttackSpace { get; set; }
        public MoveSpace MoveSpace { get; set; }
        public List<Enums.GridBlockType> FavorableTerrain { get; set; }
        public int MoveDistance { get; set; }
        public int MaxAttackDistance { get; set; }
        public int MinAttackDistance { get; set; }

        public bool IsSpaceActive
        {
            get { return ActiveSpace != Enums.ActiveSpace.Inactive; }
        }

        public void Reset()
        {
            ActiveSpace = Enums.ActiveSpace.Inactive;
            GridStart = null;
            if(AttackSpace) AttackSpace.Disable();
            if(MoveSpace) MoveSpace.Disable();
            MoveDistance = -1;
            MinAttackDistance = -1;
            MaxAttackDistance = -1;
        }

        public void ShowMoveSpace(GridBlock moveFrom)
        {
            Vector2 moveFromPos;
            if (moveFrom == null)
                moveFromPos = MoveSpace.transform.position;
            else
                moveFromPos = moveFrom.Position;

            ShowSpace(MoveSpace, AttackSpace, moveFromPos);
        }

        public void ShowAttackSpace(GridBlock moveFrom)
        {
            Vector2 moveFromPos;
            if (moveFrom == null)
                moveFromPos = MoveSpace.transform.position;
            else
                moveFromPos = moveFrom.Position;

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

        public override bool Equals(object obj)
        {
            var checkP = ((int, int, int))obj;
            return MoveDistance == checkP.Item1 && MinAttackDistance == checkP.Item2 && MaxAttackDistance == checkP.Item3;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
