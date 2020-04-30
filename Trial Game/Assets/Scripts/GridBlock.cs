using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

public class GridBlock : MonoBehaviour
{
    public Enums.GridBlockType Type;
    public GameObject MoveSpace;
    public GameObject AttackSpace;
    public int MovementCost = 0;
    public bool Unpassable = false;
    public bool IsDestination = false;

    [HideInInspector] public UnitController CurrentUnit;
    [HideInInspector] public bool IsSpaceActive { get { return _gridParams.IsSpaceActive; } }
    [HideInInspector] public Vector2 GridPosition { get; set; }
    [HideInInspector] public Vector2 Position { get { return transform.position; } }
    [HideInInspector] public GridNeighbors Neighbors { get; private set; }
    [HideInInspector] public Enums.ActiveSpace ActivePlayerSpace { get { return _gridParams.ActiveSpace; } }
    [HideInInspector] public int PlayerMoveDistance { get { return _gridParams.MoveDistance; } }
    [HideInInspector] public int PlayerAttackDistance { get { return _gridParams.MaxAttackDistance; } }

    private PlayerManager _pM;
    private PlayerParams _gridParams;
    private GridBlock _bestNeighbor;
    private bool _gotNeighbors;

    public Enums.ActiveSpace ActiveSpace(Enums.Player player)
    {
        if (player == Enums.Player.Player1)
            return _gridParams.ActiveSpace;
        else
            return Enums.ActiveSpace.Move;
    }

    public (int MoveDistance, int MinAttackDis, int MaxAttackDis) GetPlayerMoveParams()
    {
        if (_gridParams == null)
            return (-1, -1, -1);
        else
            return (_gridParams.MoveDistance, _gridParams.MinAttackDistance, _gridParams.MaxAttackDistance);
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
        var tempMove = pParams.MoveDistance - MovementCost;
        var tempAttack = pParams.MaxAttackDis - 1;

        if (tempMove > _gridParams.MoveDistance || tempAttack > _gridParams.MaxAttackDistance)
        {
            SetGrid(moveFrom, pParams.MoveDistance, pParams.MinAttackDis, pParams.MaxAttackDis);
        }
    }

    public void SetGrid(GridBlock moveFrom, int moveDistance, int minAttackDistance, int maxAttackDistance)
    {
        bool saveParams = true;
        if (Unpassable || CurrentUnit != null && CurrentUnit.Player != Enums.Player.Player1)
            moveDistance = -1;

        moveDistance = Mathf.Clamp(moveDistance - MovementCost, -1, 9999);

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
            _gridParams.MoveDistance = moveDistance;
            _gridParams.MinAttackDistance = minAttackDistance;
            _gridParams.MaxAttackDistance = maxAttackDistance;
        }

        _pM.UpdateBlockGrid(GridPosition, this, saveParams);
    }

    //public void CreateNonPlayerGrid(GridBlock start, UnitManager uM, int moveDistance, int maxAttackDistance)
    //{
    //    moveDistance -= MovementCost;

    //    if (CurrentUnit != null && this != start && !(uM.Player == Enums.Player.Player1 && CurrentUnit.Player == Enums.Player.Player1))// TODO: Figure out how to not allow enemy units how to stack on each other.
    //        moveDistance = -1;

    //    if (uM.IsGridActive(GridPosition) && _gridParams.ContainsKey(uM.Player) && _gridParams[uM.Player].MoveDistance > moveDistance)
    //        return;

    //    _gridParams[uM.Player].GridStart = start;

    //    if (moveDistance < 0)
    //    {
    //        _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Attack;
    //        --maxAttackDistance;
    //    }
    //    else
    //        _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Move;

    //    _gridParams[uM.Player].MoveDistance = moveDistance;
    //    _gridParams[uM.Player].MaxAttackDistance = maxAttackDistance;

    //    if (moveDistance >= 0)
    //        uM.UpdateBlockGrid(GridPosition, this);

    //    // If there aren't any more move or attack spaces, dont ask neighbors to do anything
    //    if (moveDistance > 0)
    //    {
    //        Neighbors.Up?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
    //        Neighbors.Down?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
    //        Neighbors.Right?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
    //        Neighbors.Left?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
    //    }
    //}

    //public void CreateGrid()
    //{
    //    foreach (GridBlock neighbor in Neighbors)
    //    {
    //        neighbor?.CreateGrid(this,
    //            _gridParams[Enums.Player.Player1].UnitManager,
    //            _gridParams[Enums.Player.Player1].MoveDistance,
    //            _gridParams[Enums.Player.Player1].MaxAttackDistance,
    //            true);
    //    }
    //}

    //public void CreateGrid(GridBlock start, UnitManager uM, int moveDistance, int maxAttackDistance, bool update = false)
    //{
    //    if (this.transform.position.x == 3.5 && this.transform.position.y == -6.5)
    //        Console.WriteLine("");

    //     moveDistance -= MovementCost;

    //    if (CurrentUnit != null && this != start && !(uM.Player == Enums.Player.Player1 && CurrentUnit.Player == Enums.Player.Player1))// && CurrentUnit.Player != player) TODO: Figure out how to not allow enemy units how to stack on each other.
    //        moveDistance = -1;

    //    if (_gridParams[uM.Player].MoveDistance > moveDistance && !(_gridParams[uM.Player].MoveDistance ==  moveDistance && update))
    //        return;

    //    _gridParams[uM.Player].GridStart = start;
    //    _gridParams[uM.Player].UnitManager = uM;

    //    if (moveDistance < 0)
    //    {
    //        _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Attack;
    //        --maxAttackDistance;
    //    }
    //    else
    //        _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Move;

    //    _gridParams[uM.Player].MoveDistance = moveDistance;
    //    _gridParams[uM.Player].MaxAttackDistance = maxAttackDistance;

    //    if (uM.Player == Enums.Player.Player1)
    //    {
    //        // if the space is visited again through a better path, reset it.
    //        if (_gridParams[uM.Player].MoveDistance < moveDistance && _gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Attack)
    //            _gridParams[uM.Player].AttackSpace?.Disable();
    //        else if (_gridParams[uM.Player].MoveDistance < moveDistance && _gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Move)
    //            _gridParams[uM.Player].MoveSpace?.Disable();
    //        //else if (_gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Move)
    //        //    return;

    //        if (moveDistance >= 0)
    //        {
    //            _gridParams[uM.Player].AttackSpace?.Disable();

    //            if (_gridParams[uM.Player].MoveSpace == null)
    //                _gridParams[uM.Player].CreateMoveSpace(this, MoveSpace);
    //            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Move;
    //            _gridParams[uM.Player].MoveSpace?.Enable();

    //        }
    //        else if (_gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Attack)
    //        {
    //            if (_gridParams[uM.Player].AttackSpace == null)
    //                _gridParams[uM.Player].CreateAttackSpace(this, AttackSpace);
    //            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Attack;
    //            _gridParams[uM.Player].AttackSpace?.Enable();
    //        }
    //        else
    //            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Inactive;
    //    }

    //    uM.UpdateBlockGrid(GridPosition, this);

    //    // If there aren't any more move or attack spaces, dont ask neighbors to do anything
    //    if (moveDistance > 0 || maxAttackDistance > 0) 
    //    {
    //        Neighbors.Up?.CreateGrid(start, uM, moveDistance, maxAttackDistance, update);
    //        Neighbors.Down?.CreateGrid(start, uM, moveDistance, maxAttackDistance, update);
    //        Neighbors.Right?.CreateGrid(start, uM, moveDistance, maxAttackDistance, update);
    //        Neighbors.Left?.CreateGrid(start, uM, moveDistance, maxAttackDistance, update);
    //    }
    //}

    //public void UpdatePlayerGrid(Enums.Player player = Enums.Player.Player1)
    //{
    //    if (_gridParams[player].ActiveSpace == Enums.ActiveSpace.Move)
    //        CreateGrid(_gridParams[player].GridStart, _gridParams[player].UnitManager, 1, _gridParams[player].MaxAttackDistance, true);
    //    else
    //    {
    //        var values = Neighbors.GetBestMoveNeighbor();
    //        CreateGrid(_gridParams[player].GridStart, _gridParams[player].UnitManager, values.Item1, values.Item2, true);
    //    }
    //}

    public void Disable(Enums.Player player)
    {
        _gridParams.Reset();
    }

    public GridBlock GetRangedSpaces(GridBlock start, int minDis, GridBlock target = null, List<OrderedGridBlock> gridDis = null)
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
                g.GetRangedSpaces(start, minDis, g, gridDis);
            }
        }

        if (org)
        {
            var orderedList = gridDis.OrderBy(g => g.OrderedValue).ToList();
            var lowestVal = orderedList.First().OrderedValue;
            var trimmedList = orderedList.Where(x => x.OrderedValue == lowestVal).ToList();
            System.Random random = new System.Random();
            var randomN = random.Next(trimmedList.Count() - 1);
            return trimmedList[randomN].GridBlock;
        }
        else
            return null;
    }

    public bool IsCurrentUnitEnemy(Enums.Player player)
    {
        return CurrentUnit != null &&
                CurrentUnit.Player != player &&
                !CurrentUnit.AlliedWith.Contains(player);
    }

    public void UpdateMoveSpaceState(Enums.Player player, Vector2 cDir, Vector2? nDir)
    {
        _gridParams.UpdateMoveSpaceState(cDir, nDir);
    }

    void Awake()
    {
        Neighbors = new GridNeighbors(this);
        _gridParams = new PlayerParams();
    }

    void Start()
    {
        var cursorBoundaries = GameObject.Find("CursorBoundaries");
        var pCollider = cursorBoundaries.GetComponent<PolygonCollider2D>();
        if (!Position.InsideSquare(pCollider.points[1], pCollider.points[3]))
            Destroy(gameObject);

        _pM = FindObjectOfType<PlayerManager>();

        CreateAttackSpace(AttackSpace);
        CreateMoveSpace(MoveSpace);
        _gridParams.Reset();
    }

    void Update()
    {
        if (!_gotNeighbors)
        {
            GetNeighbors();
            _gotNeighbors = true;
        }

        if (!_pM.PlayerInfo.MovementPath.Contains(this))
        {
            _gridParams.ResetSpace();
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
        SetGrid(null, -1, _gridParams.MinAttackDistance, _gridParams.MaxAttackDistance);
    }

    private void CreateAttackSpace(GameObject space)
    {
        _gridParams.AttackSpace = GetSpaceScript<AttackSpace>(this, space);
        _gridParams.AttackSpace.Player = Enums.Player.Player1;
        _gridParams.AttackSpace.ParentGridBlock = this;
    }

    private void CreateMoveSpace(GameObject space)
    {
        _gridParams.MoveSpace = GetSpaceScript<MoveSpace>(this, space);
        _gridParams.MoveSpace.Player = Enums.Player.Player1;
        _gridParams.MoveSpace.ParentGridBlock = this;
    }

    private T GetSpaceScript<T>(GridBlock parent, GameObject gO)
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
            ResetSpace();
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
            MoveSpace.ResetSpace();
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

        public void ResetSpace()
        {
            MoveSpace?.ResetSpace();
        }

        public void UpdateMoveSpaceState(Vector2 cDir, Vector2? nDir)
        {
            MoveSpace.MoveState(cDir, nDir);
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
