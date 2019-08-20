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

    private GridPlayerSpaces _moveSpaces;
    private GridPlayerSpaces _attackSpaces;
    private GridPlayerSpaces _spaces;
    private GridNeighbors _neighbors;
    private GameObject _space;
    private SpriteRenderer _sR;
    private int _moveDistance;
    private bool _gotNeighbors;
    private bool _showingGrid;

    public void CreateGrid(Enums.Player player, int moveDistance, int attackDistance, Vector2 gridPos)
    {
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
            _spaces = _moveSpaces;
            _attackSpaces[player]?.Disable();
        }
        else
        {
            _space = AttackSpace;
            _spaces = _attackSpaces;
            attackDistance -= 1;
        }

        if (_spaces[player] == null)
        {
            var gO = Instantiate(_space, transform.position, Quaternion.identity);
            _spaces[player] = gO.GetComponent<Space>();
            _spaces[player].Player = player;
        }

        _spaces[player].Enable();
        _spaces[player].GridPosition = gridPos;

        // If there aren't any more move or attack spaces, dont ask neighbors to do anything
        if (moveDistance > 0 || attackDistance > 0)
        {
            _neighbors.Up?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x, gridPos.y - 1));
            _neighbors.Down?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x, gridPos.y + 1));
            _neighbors.Right?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x + 1, gridPos.y));
            _neighbors.Left?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x - 1, gridPos.y));
        }
    }

    private void Start()
    {
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
    }

    public void Destroy()
    {
        Destroy(gameObject);
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
        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, 1f, LayerMask.GetMask("MapGrid"));
        if (hit.collider != null)
        {
            GameObject rhgo = hit.transform.gameObject;
            GridBlock grid = rhgo.GetComponent<GridBlock>();

            return grid;
        }

        return null;
    }
}
