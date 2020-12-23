using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridBlock : MonoBehaviour
{
    public UnitController CurrentUnit;
    public Enums.GridBlockType Type;
    public GameObject MoveSpace;
    public GameObject AttackSpace;
    public GameObject ActivePath;
    public GameObject SavedPath;
    public Image MinimapTile;
    public int MovementCost = 0;
    public bool Unpassable = false;
    public bool IsDestination = false;

    public bool isOccupied { get { return CurrentUnit != null; } }
    public bool IsSpaceActive { get { return _gridParams.IsSpaceActive; } }
    public Vector2 GridPosition { get; set; }
    public Vector2 Position { get { return transform.position; } }
    public GridNeighbors Neighbors { get; private set; }
    public Enums.ActiveSpace ActivePlayerSpace { get { return _gridParams.ActiveSpace; } }
    public int PlayerMoveDistance { get { return _gridParams.MoveDistance; } }
    public int PlayerAttackDistance { get { return _gridParams.MaxAttackDistance; } }

    private PlayerManager _pM;
    private PlayerParams _gridParams;
    private CursorController _cursor;
    private GridBlock _bestNeighbor;
    private Dictionary<UnitController, Path_Saved> _savedPaths;
    private Path_Active _currentAcitvePath;
    private bool _gotNeighbors;

    public Enums.ActiveSpace ActiveSpace(Enums.Player player)
    {
        if (player == Enums.Player.Player1)
            return _gridParams.ActiveSpace;
        else
            return Enums.ActiveSpace.Move;
    }

    public (List<Enums.GridBlockType> FavorableTerrain, int MoveDistance, int MinAttackDis, int MaxAttackDis) GetPlayerMoveParams()
    {
        if (_gridParams == null)
            return (new List<Enums.GridBlockType>(),-1, -1, -1);
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
        var pParams = moveFrom.GetPlayerMoveParams();
        var tempMove = pParams.MoveDistance - (pParams.FavorableTerrain.Contains(this.Type) ?  1 : MovementCost);
        var tempAttack = pParams.MaxAttackDis - 1;

        if (tempMove > _gridParams.MoveDistance || tempAttack > _gridParams.MaxAttackDistance)
        {
            SetGrid(moveFrom, pParams.FavorableTerrain, pParams.MoveDistance, pParams.MinAttackDis, pParams.MaxAttackDis);
        }
    }

    public void SetGrid(GridBlock moveFrom, List<Enums.GridBlockType> favTerrain, int moveDistance, int minAttackDistance, int maxAttackDistance)
    {
        bool saveParams = true;
        if (Unpassable || CurrentUnit != null && CurrentUnit.Player != Enums.Player.Player1)
            moveDistance = -1;

        if(moveFrom == null)
            moveDistance += (favTerrain.Contains(this.Type) ? 1 : MovementCost);

        moveDistance = Mathf.Clamp(moveDistance - (favTerrain.Contains(this.Type) ? 1 : MovementCost), -1, 9999); // If this terrain is favorable to the unit, only subtract one.

        if (moveDistance >= 0)
        {
            _gridParams.ActiveSpace = Enums.ActiveSpace.Move;
            _gridParams.ShowMoveSpace(moveFrom?.Position);
        }
        else if (maxAttackDistance > 0)
        {
            _gridParams.ActiveSpace = Enums.ActiveSpace.Attack;
            _gridParams.ShowAttackSpace(moveFrom?.Position);
            maxAttackDistance--;
        }
        else
        {
            _gridParams.Reset();
            saveParams = false;
        }

        if (saveParams)
        {
            _gridParams.FavorableTerrain = favTerrain;
            _gridParams.MoveDistance = moveDistance;
            _gridParams.MinAttackDistance = minAttackDistance;
            _gridParams.MaxAttackDistance = maxAttackDistance;
        }

        _pM.UpdateBlockGrid(GridPosition, this, saveParams);
    }

    public void Disable(Enums.Player player)
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
        return CurrentUnit != null &&
                CurrentUnit.Player != player;// &&
                //!CurrentUnit.AlliedWith.Contains(player);
    }

    public void UpdatePathState(UnitController unit, Vector2 cDir, Vector2? nDir)
    {
        _currentAcitvePath.UpdatePathState(cDir, nDir);
        _currentAcitvePath.Show = true;
    }

    public void SavePath(UnitController unit, Color color)
    {
        var savedPath = Instantiate(SavedPath, transform.position, Quaternion.identity);
        var spScript = savedPath.GetComponent<Path_Saved>();
        spScript.SetColor(color);
        spScript.SetPathDirection(_currentAcitvePath.PathDirection);
        _savedPaths.Add(unit, spScript);
    }

    public void DeletePath(UnitController unit)
    {
        if (_savedPaths.ContainsKey(unit))
        {
            Destroy(_savedPaths[unit].gameObject);
            _savedPaths.Remove(unit);
        }
    }

    void Awake()
    {
        Neighbors = new GridNeighbors(this);
        _gridParams = new PlayerParams();
    }

    void Start()
    {
        _currentAcitvePath = CreatePathBlock();
        var cursorBoundaries = GameObject.Find("CursorBoundaries");
        var pCollider = cursorBoundaries.GetComponent<PolygonCollider2D>();
        _cursor = FindObjectOfType<CursorController>();

        if (!Position.InsideSquare(pCollider.points[1], pCollider.points[3]))
            Destroy(gameObject);
        else
        {
            try
            {
                var uiParent = GameObject.FindGameObjectWithTag("UI");
                var minimapObject = uiParent.FindObject("MapTileIcons");
                var uiTile = Instantiate(MinimapTile);
                uiTile.rectTransform.SetParent(minimapObject.transform);
                uiTile.rectTransform.anchoredPosition = new Vector2(MinimapTile.rectTransform.rect.width * (transform.position.x - .5f), MinimapTile.rectTransform.rect.height * (transform.position.y - .5f));
            }
            catch { Debug.Log("failed gridblock minimap"); }

            _pM = FindObjectOfType<PlayerManager>();

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
            GetNeighbors();
            _gotNeighbors = true;
        }

        if (!_pM.PlayerInfo.MovementPath.Contains(this) && _currentAcitvePath.Show)// && _pM.PlayerInfo.SelectedUnit == null)// && PathBlocks.ContainsKey(_pM.PlayerInfo.SelectedUnit))
        {
            _currentAcitvePath.Show = false;
        }

        var bestNeighbor = Neighbors?.GetBestMoveNeighbor();
        if (bestNeighbor != null)
            CheckGrid(bestNeighbor);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        var unitCon = colObj.GetComponent<UnitController>();
        if (unitCon != null && CurrentUnit == null)
        {
            CurrentUnit = unitCon;
            if (_gridParams.MoveSpace.Active && unitCon.Player != Enums.Player.Player1)
            {
                if (_pM.PlayerInfo.MovementPath.Contains(this))
                    _cursor.EnemyInPath(this);

                UpdateGrid();
                _pM.UpdateActiveGrid(this);
            }
        }
    }

    private void GetNeighbors()
    {
        Neighbors.SetNeighbors(
            GetNeighbor(Vector2.up),
            GetNeighbor(Vector2.down),  
            GetNeighbor(Vector2.left),
            GetNeighbor(Vector2.right)
        );
    }

    private GridBlock GetNeighbor(Vector2 dir)
    {
        Vector2 startPos = transform.position.V2();
        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, 1f, LayerMask.GetMask("GridBlock"));
        if (hit.collider != null)
        {
            GameObject rhgo = hit.transform.gameObject;
            GridBlock grid = rhgo.GetComponent<GridBlock>();

            return grid;
        }

        return null;
    }

    private void UpdateGrid()
    {
        SetGrid(null, _gridParams.FavorableTerrain, -1, _gridParams.MinAttackDistance, _gridParams.MaxAttackDistance);
    }

    private void CreateAttackSpace(GameObject space)
    {
        _gridParams.AttackSpace = GetGridBlockItemScript<AttackSpace>(this, space);
        _gridParams.AttackSpace.Player = Enums.Player.Player1;
        _gridParams.AttackSpace.ParentGridBlock = this;
    }

    private void CreateMoveSpace(GameObject space)
    {
        _gridParams.MoveSpace = GetGridBlockItemScript<MoveSpace>(this, space);
        _gridParams.MoveSpace.Player = Enums.Player.Player1;
        _gridParams.MoveSpace.ParentGridBlock = this;
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
            AttackSpace?.Disable();
            MoveSpace?.Disable();
            MoveDistance = -1;
            MinAttackDistance = -1;
            MaxAttackDistance = -1;
        }

        public void ShowMoveSpace(Vector2? moveFrom)
        {
            ShowSpace(MoveSpace, AttackSpace, moveFrom);
        }

        public void ShowAttackSpace(Vector2? moveFrom)
        {
            ShowSpace(AttackSpace, MoveSpace, moveFrom);
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
