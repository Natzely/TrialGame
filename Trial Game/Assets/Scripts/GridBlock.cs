using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [HideInInspector] public bool IsSpaceActive { get { return _gridParams[Enums.Player.Player1].IsActive; } }
    [HideInInspector] public Vector2 GridPosition { get; set; }
    [HideInInspector] public Vector2 Position { get { return transform.position; } }
    [HideInInspector] public GridNeighbors Neighbors { get; private set; }
    [HideInInspector] public Enums.ActiveSpace ActivePlayerSpace { get { return _gridParams[Enums.Player.Player1].ActiveSpace; } }

    private PlayerManager _pM;
    private Dictionary<Enums.Player, GridParams> _gridParams;
    private bool _gotNeighbors;

    public Enums.ActiveSpace ActiveSpace(Enums.Player player)
    {
        if (player == Enums.Player.Player1)
            return _gridParams[player].ActiveSpace;
        else
            return Enums.ActiveSpace.Move;
    }

    public IEnumerable<GridBlock> AvailableAttackSpace(GridBlock behindGrid, int unitAttackDistance)
    {
        var neightbors = Neighbors.AvailableNeighbors(behindGrid.Position).ToList();
        foreach(var n in neightbors)
        {
            if (n.Position.GridDistance(behindGrid.Position) <= unitAttackDistance)
                yield return n;
        }
    }

    public void CreateNonPlayerGrid(GridBlock start, UnitManager uM, int moveDistance, int maxAttackDistance)
    {
        moveDistance -= MovementCost;

        if (CurrentUnit != null && this != start && !(uM.Player == Enums.Player.Player1 && CurrentUnit.Player == Enums.Player.Player1))// && CurrentUnit.Player != player) TODO: Figure out how to not allow enemy units how to stack on each other.
            moveDistance = -1;

        if (uM.IsGridActive(GridPosition) && _gridParams.ContainsKey(uM.Player) && _gridParams[uM.Player].MoveDistance > moveDistance)
            return;

        _gridParams[uM.Player].GridStart = start;

        if (moveDistance < 0)
        {
            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Attack;
            _gridParams[uM.Player].MaxAttackDistance = --maxAttackDistance;
        }
        else
            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Move;

        _gridParams[uM.Player].MoveDistance = moveDistance;

        if (moveDistance >= 0)
            uM.UpdateBlockGrid(GridPosition, this);

        // If there aren't any more move or attack spaces, dont ask neighbors to do anything
        if (moveDistance > 0)
        {
            Neighbors.Up?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
            Neighbors.Down?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
            Neighbors.Right?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
            Neighbors.Left?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
        }
    }

    public void CreateGrid(GridBlock start, UnitManager uM, int moveDistance, int maxAttackDistance)
    {
        moveDistance -= MovementCost;

        if (CurrentUnit != null && this != start && !(uM.Player == Enums.Player.Player1 && CurrentUnit.Player == Enums.Player.Player1))// && CurrentUnit.Player != player) TODO: Figure out how to not allow enemy units how to stack on each other.
            moveDistance = -1;

        if (uM.IsGridActive(GridPosition) && _gridParams.ContainsKey(uM.Player) && _gridParams[uM.Player].MoveDistance > moveDistance)
            return;

        _gridParams[uM.Player].GridStart = start;

        if (moveDistance < 0)
        {
            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Attack;
            _gridParams[uM.Player].MaxAttackDistance = --maxAttackDistance;
        }
        else
            _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Move;

        if (uM.Player == Enums.Player.Player1)
        {
            // if the space is visited again through a better path, reset it.
            if (_gridParams[uM.Player].MoveDistance < moveDistance && _gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Attack)
                _gridParams[uM.Player].AttackSpace?.Disable();
            else if (_gridParams[uM.Player].MoveDistance < moveDistance && _gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Move)
                _gridParams[uM.Player].MoveSpace?.Disable();
            else if (_gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Move)
                return;

            _gridParams[uM.Player].MoveDistance = moveDistance;

            if (moveDistance >= 0)
            {
                _gridParams[uM.Player].AttackSpace?.Disable();

                if (_gridParams[uM.Player].MoveSpace == null)
                    _gridParams[uM.Player].CreateMoveSpace(this, MoveSpace);
                _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Move;
                _gridParams[uM.Player].MoveSpace?.Enable();

            }
            else if (_gridParams[uM.Player].ActiveSpace == Enums.ActiveSpace.Attack)
            {
                if (_gridParams[uM.Player].AttackSpace == null)
                    _gridParams[uM.Player].CreateAttackSpace(this, AttackSpace);
                _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Attack;
                _gridParams[uM.Player].AttackSpace?.Enable();
            }
            else
                _gridParams[uM.Player].ActiveSpace = Enums.ActiveSpace.Inactive;
        }
        else
        {
            _gridParams[uM.Player].MoveDistance = moveDistance;
        }

        if (moveDistance >= 0 || uM.Player == Enums.Player.Player1)
            uM.UpdateBlockGrid(GridPosition, this);

        // If there aren't any more move or attack spaces, dont ask neighbors to do anything
        if (moveDistance > 0 || (uM.Player == Enums.Player.Player1 && maxAttackDistance > 0)) 
        {
            Neighbors.Up?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
            Neighbors.Down?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
            Neighbors.Right?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
            Neighbors.Left?.CreateGrid(start, uM, moveDistance, maxAttackDistance);
        }
    }

    public void UpdateGrid(Enums.Player player, Enums.ActiveSpace active)
    {
        _gridParams[player].ActiveSpace = active;
    }

    public void Disable(Enums.Player player)
    {
        var gridInfo = _gridParams[player];
        gridInfo.ActiveSpace = Enums.ActiveSpace.Inactive;
        gridInfo.ResetSpace();
        gridInfo.AttackSpace?.Disable();
        gridInfo.MoveSpace?.Disable();
        gridInfo.MoveDistance = -1;
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
        return  CurrentUnit != null && 
                CurrentUnit.Player != player && 
                !CurrentUnit.AlliedWith.Contains(player);
    }

    public void UpdateMoveSpaceState(Enums.Player player, Vector2 cDir, Vector2? nDir)
    {
        _gridParams[player].UpdateMoveSpaceState(cDir, nDir);
    }

    void Awake()
    {
        Neighbors = new GridNeighbors(this);
        _gridParams = new Dictionary<Enums.Player, GridParams>();
    }

    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        
        foreach(Enums.Player player in Enum.GetValues(typeof(Enums.Player)))
        {
            _gridParams.Add(player, new GridParams(player, -1));;
        }
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
            _gridParams[Enums.Player.Player1].ResetSpace();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        var unitCon = colObj.GetComponent<UnitController>();
        if (unitCon != null && CurrentUnit == null)
            CurrentUnit = unitCon;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (CurrentUnit == null)
        {
            var colObj = collision.gameObject;
            var unitCon = colObj.GetComponent<UnitController>();
            if (unitCon != null && CurrentUnit == null)
                CurrentUnit = unitCon;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var colObj = collision.gameObject;
        var unitCon = colObj.GetComponent<UnitController>();
        if (unitCon == CurrentUnit)
            CurrentUnit = null;
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

    class GridParams
    {
        public GridBlock GridStart;
        public int MoveDistance;
        public int MaxAttackDistance;

        public Enums.ActiveSpace ActiveSpace { get; set; }
        public AttackSpace AttackSpace { get; private set; }
        public MoveSpace MoveSpace { get; private set; }

        private Enums.Player _player;

        public GridParams(Enums.Player player, int moveDistance)
        {
            MoveDistance = moveDistance;
            _player = player;
        }

        public bool IsActive
        {
            get { return ActiveSpace != Enums.ActiveSpace.Inactive; }
        }

        public void ResetSpace()
        {
            MoveSpace?.ResetSpace();
        }

        public void UpdateMoveSpaceState(Vector2 cDir, Vector2? nDir)
        {
            MoveSpace.MoveState(cDir, nDir);
        }

        public void CreateAttackSpace(GridBlock parent, GameObject space)
        {
            AttackSpace = GetSpaceScript<AttackSpace>(parent, space);
            AttackSpace.Player = _player;
            AttackSpace.ParentGridBlock = parent;
        }

        public void CreateMoveSpace(GridBlock parent, GameObject space)
        {
            MoveSpace = GetSpaceScript<MoveSpace>(parent, space);
            MoveSpace.Player = _player;
            MoveSpace.ParentGridBlock = parent;
        }

        private T GetSpaceScript<T>(GridBlock parent, GameObject gO)
        {
            var o = Instantiate(gO, parent.transform.position, Quaternion.identity);
            return o.GetComponent<T>();
        }
    }
}
