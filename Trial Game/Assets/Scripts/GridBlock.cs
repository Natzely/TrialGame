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

    private Vector2 _gridPos;
    public Vector2 PlayerGridPosition
    {
        get
        {
            return _gridPos;
        }
        set
        {
            _gridPos = value;
            _pM.UpdatePathMatrix(Enums.Player.Player1, value, this);
        }
    }

    public bool SpaceActive
    {
        get { return _moveSpaces[Enums.Player.Player1].gameObject.activeSelf ||
                     _attackSpaces[Enums.Player.Player1].gameObject.activeSelf;}
    }

    public Vector2 FullGridPosition { get; set; }

    private PlayerManager _pM;
    private GridPlayerSpaces _moveSpaces;
    private GridPlayerSpaces _attackSpaces;
    private GridNeighbors _neighbors;
    private GameObject _space;
    private SpriteRenderer _sR;
    private int _moveDistance;
    private bool _gotNeighbors;
    private bool _showingGrid;

    public Vector2 Position
    {
        get { return transform.position; }
    }

    public void CreateGrid(Enums.Player player, int moveDistance, int attackDistance, Vector2 gridPos)
    {
        GridPlayerSpaces spaces;
        moveDistance -= MovementCost;

        // if the space is visited again through a better path, reset it.
        if (_moveDistance < moveDistance && _attackSpaces.PlayerSpaceEnabled(player))
            _attackSpaces[player].Disable();
        else if (_moveDistance < moveDistance && _moveSpaces.PlayerSpaceEnabled(player))
            _moveSpaces[player].Disable();
        else if (_moveSpaces.PlayerSpaceEnabled(player))
            return;

        _moveDistance = moveDistance;

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
            attackDistance -= 1;
        }

        if (spaces[player] == null)
        {
            var gO = Instantiate(_space, transform.position, Quaternion.identity);
            spaces[player] = gO.GetComponent<Space>();
            spaces[player].Player = player;
            spaces[player].ParentGridBlock = this;
        }

        spaces[player].Enable();
        PlayerGridPosition = gridPos;

        // If there aren't any more move or attack spaces, dont ask neighbors to do anything
        if (moveDistance > 0 || attackDistance > 0)
        {
            _neighbors.Up?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x, gridPos.y - 1));
            _neighbors.Down?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x, gridPos.y + 1));
            _neighbors.Right?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x + 1, gridPos.y));
            _neighbors.Left?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x - 1, gridPos.y));
        }
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

    private void Start()
    {
        _pM = FindObjectOfType<PlayerManager>();
        _neighbors = new GridNeighbors();
        _sR = GetComponent<SpriteRenderer>();
        _moveSpaces = new GridPlayerSpaces();
        _attackSpaces = new GridPlayerSpaces();
        transform.parent = null;
    }

    private void Update()
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
                moveSpace.ResetSpace();
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
