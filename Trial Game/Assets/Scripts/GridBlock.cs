using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public Enums.GridBlockType Type;
    public GameObject MoveSpace;
    public GameObject AttackSpace;
    public int MovementCost = 0;
    public bool Unpassable = false;
    public bool IsDestination = false;

    [HideInInspector]
    public UnitController CurrentUnit;

    [HideInInspector]
    public bool IsSpaceActive
    {
        get { return _moveSpaces.GetSpaceEnabled(Enums.Player.Player1) ||
                     _attackSpaces.GetSpaceEnabled(Enums.Player.Player1);}
    }

    [HideInInspector]
    public Vector2 GridPosition { get; set; }
    [HideInInspector]
    public Vector2 Position
    {
        get { return transform.position; }
    }
    [HideInInspector]
    public  GridNeighbors Neighbors
    {
        get { return _neighbors; }
    }

    [HideInInspector]
    public Space PlayerActiveSpace { get; set; }

    private PlayerManager _pM;
    private GridPlayerSpaces _moveSpaces;
    private GridPlayerSpaces _attackSpaces;
    private GridNeighbors _neighbors;
    private GameObject _space;
    private SpriteRenderer _sR;
    private Dictionary<Enums.Player, int> _moveDistance;
    private Dictionary<Enums.Player, int> _attackDistance;
    private Dictionary<Enums.Player, Enums.ActiveTile> _activeTile;
    private bool _gotNeighbors;
    private bool _showingGrid;

    public IEnumerable<GridBlock> GetRangedSpaces(GridBlock target, GridBlock start, int minDis = 1, HashSet<GridBlock> gridDis = null, List<GridBlock> usedGrids = null)
    {
        bool org = false;
        if (gridDis == null)
        {
            gridDis = new HashSet<GridBlock>();
            usedGrids = new List<GridBlock>();
            org = true;
        }

        var n = target.Neighbors.OrderByDistance(start, minDis <= 1).ToList();
        foreach(GridBlock g in n)
        {
            if(!usedGrids.Contains(g) && (minDis > 1 && g.CurrentUnit != null))
            {
                gridDis.Add(g);
            }
        }


        if (minDis > 1)
        {
            foreach (GridBlock g in gridDis)
            {
                usedGrids.Add(g);
                gridDis.Remove(g);

                g.GetRangedSpaces(target, start, --minDis, gridDis, usedGrids);
            }
        }

        if (org)
            yield return gridDis.First();
        else
            yield break;

        //var neighbors = target.Neighbors;
        //var orderedNeighbors = neighbors.OrderByDistance(start, true);
        //var possibleBest = orderedNeighbors.Where(n => n.CurrentUnit == null && !n.Unpassable).ToList();
        //if (possibleBest.Count > 0)
        //{
        //    result = possibleBest.First();
        //}
        //else
        //{
        //    foreach (GridBlock gB in neighbors)
        //    {
        //        if ((result = BestSpaceNextToTarget(gB, start)) != null)
        //            break;
        //    }
        //}

        //return result;
    }

    public Enums.ActiveTile ActiveSpace(Enums.Player player)
    {
        if (player == Enums.Player.Player1)
            return _activeTile[player];
        else
            return Enums.ActiveTile.Move;
    }

    public List<GridBlock> AvailableAttackSpace(GridBlock behindGrid = null)
    {
        return Neighbors.AvailableNeighbors(behindGrid.Position).ToList();
    }

    public void CreateGrid(GridBlock start, Enums.Player player, int moveDistance, int minAttackDistance, int maxAttackDistance)
    {
        GridPlayerSpaces spaces;
        moveDistance -= MovementCost;

        if (CurrentUnit != null && this != start && !(player == Enums.Player.Player1 && CurrentUnit.Player == Enums.Player.Player1))// && CurrentUnit.Player != player) TODO: Figure out how to not allow enemy units how to stack on each other.
            moveDistance = -1;

        if (_pM.GetMovementSpace(player, GridPosition) != null && 
            _moveDistance.ContainsKey(player) && _moveDistance[player] > moveDistance)
            return;

        if (moveDistance < 0)
        {
            //if (Vector2.Distance(start.GridPosition, GridPosition) > minAttackDistance)
            //{
                _activeTile[player] = Enums.ActiveTile.Attack;
            //}
            //else
            //    _activeTile[player] = Enums.ActiveTile.Inactive;
            _attackDistance[player] = --maxAttackDistance;
        }
        else
            _activeTile[player] = Enums.ActiveTile.Move;

        if (player == Enums.Player.Player1)
        {
            // if the space is visited again through a better path, reset it.
            if (_moveDistance[player] < moveDistance && _attackSpaces.GetSpaceEnabled(player))
                _attackSpaces[player].Disable();
            else if (_moveDistance[player] < moveDistance && _moveSpaces.GetSpaceEnabled(player))
                _moveSpaces[player].Disable();
            else if (_moveSpaces.GetSpaceEnabled(player))
                return;

            _moveDistance[player] = moveDistance;

            if (moveDistance >= 0)
            {
                _space = MoveSpace;
                spaces = _moveSpaces;
                _attackSpaces[player]?.Disable();
            }
            else if (_activeTile[player] == Enums.ActiveTile.Attack)
            {
                _space = AttackSpace;
                spaces = _attackSpaces;
            }
            else
                spaces = null;

            if (spaces != null && spaces[player] == null)
            {
                var gO = Instantiate(_space, transform.position, Quaternion.identity);
                spaces[player] = gO.GetComponent<Space>();
                spaces[player].Player = player;
                spaces[player].ParentGridBlock = this;
            }

            if (spaces != null)
                (PlayerActiveSpace = spaces[player]).Enable();
        }
        else
        {
            _moveDistance[player] = moveDistance;
        }

        if (moveDistance >= 0 || player == Enums.Player.Player1)
            _pM.UpdateMovementGrid(player, GridPosition, this);

        // If there aren't any more move or attack spaces, dont ask neighbors to do anything
        if (moveDistance > 0 || (player == Enums.Player.Player1 && maxAttackDistance > 0)) 
        {
            _neighbors.Up?.CreateGrid(start, player, moveDistance, minAttackDistance, maxAttackDistance);
            _neighbors.Down?.CreateGrid(start, player, moveDistance, minAttackDistance, maxAttackDistance);
            _neighbors.Right?.CreateGrid(start, player, moveDistance, minAttackDistance, maxAttackDistance);
            _neighbors.Left?.CreateGrid(start, player, moveDistance, minAttackDistance, maxAttackDistance);
        }
    }

    public bool IsCurrentUnitEnemy(Enums.Player player)
    {
        return  CurrentUnit != null && 
                CurrentUnit.Player != player && 
                !CurrentUnit.AlliedWith.Contains(player);
    }

    public void UpdateMoveSpaceState(Enums.Player player, Vector2 cDir, Vector2? nDir)
    {
        var space = _moveSpaces[player];
        if(space != null)
        {
            var moveSpace = (MoveSpace)space;
            moveSpace.MoveState(cDir, nDir);
        }
    }

    void Awake()
    {
        _neighbors = new GridNeighbors(this);
        _moveSpaces = new GridPlayerSpaces();
        _attackSpaces = new GridPlayerSpaces();
        _moveDistance = new Dictionary<Enums.Player, int>();
        _attackDistance = new Dictionary<Enums.Player, int>();
        _activeTile = new Dictionary<Enums.Player, Enums.ActiveTile>();
    }

    void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        _sR = GetComponent<SpriteRenderer>();
        
        foreach(Enums.Player player in Enum.GetValues(typeof(Enums.Player)))
        {
            _moveDistance.Add(player, -1);
        }
    }

    void Update()
    {
        if (!_gotNeighbors)
        {
            GetNeighbors();
            _gotNeighbors = true;
        }

        if (!_pM.GetPlayerInfo(Enums.Player.Player1).MovementPath.Contains(this))
        {
            var space = _moveSpaces[Enums.Player.Player1];
            if (space != null)
            {
                var moveSpace = (MoveSpace)space;
                moveSpace?.ResetSpace();
            }
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
        _neighbors.SetNeighbors(
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


}
