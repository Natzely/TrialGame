using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public GameObject MoveSpace;
    public GameObject AttackSpace;
    public int MovementCost = 0;
    public bool Unpassable = false;

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

    private PlayerManager _pM;
    private GridPlayerSpaces _moveSpaces;
    private GridPlayerSpaces _attackSpaces;
    private GridNeighbors _neighbors;
    private GameObject _space;
    private SpriteRenderer _sR;
    Dictionary<Enums.Player, int> _moveDistance;
    Dictionary<Enums.Player, int> _attackDistance;
    Dictionary<Enums.Player, Enums.ActiveTile> _activeTile;
    private bool _gotNeighbors;
    private bool _showingGrid;

    public Enums.ActiveTile ActiveSpace(Enums.Player player)
    {
        //if (_moveSpaces.ContainsPlayer(player) && _moveSpaces[player].gameObject.activeSelf)
        //    return _moveSpaces[player];
        //else if (_attackSpaces.ContainsPlayer(player) && _attackSpaces[player].gameObject.activeSelf)
        //    return _attackSpaces[player];

        //return null;

        return _activeTile[player];
    }

    public void CreateGrid(Enums.Player player, int moveDistance, int attackDistance, bool start = false)
    {
        GridPlayerSpaces spaces;
        moveDistance -= MovementCost;

        if (CurrentUnit != null && !start && !(player == Enums.Player.Player1 && CurrentUnit.Player == Enums.Player.Player1))// && CurrentUnit.Player != player) TODO: Figure out how to not allow enemy units how to stack on each other.
            moveDistance = -1;

        if (_pM.GetMovementSpace(player, GridPosition) != null && 
            _moveDistance.ContainsKey(player) && _moveDistance[player] > moveDistance)
            return;

        if (moveDistance < 0)
        {
            _attackDistance[player] = --attackDistance;
            _activeTile[player] = Enums.ActiveTile.Attack;
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
            else
            {
                _space = AttackSpace;
                spaces = _attackSpaces;
            }

            if (spaces[player] == null)
            {
                var gO = Instantiate(_space, transform.position, Quaternion.identity);
                spaces[player] = gO.GetComponent<Space>();
                spaces[player].Player = player;
                spaces[player].ParentGridBlock = this;
            }

            spaces[player].Enable();
        }
        else
        {
            _moveDistance[player] = moveDistance;
        }

        if (moveDistance >= 0 || player == Enums.Player.Player1)
            _pM.UpdateMovementGrid(player, GridPosition, this);

        // If there aren't any more move or attack spaces, dont ask neighbors to do anything
        if (moveDistance > 0 || (player == Enums.Player.Player1 && attackDistance > 0)) 
        {
            _neighbors.Up?.CreateGrid(player, moveDistance, attackDistance);
            _neighbors.Down?.CreateGrid(player, moveDistance, attackDistance);
            _neighbors.Right?.CreateGrid(player, moveDistance, attackDistance);
            _neighbors.Left?.CreateGrid(player, moveDistance, attackDistance);
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
        _neighbors = new GridNeighbors();
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
