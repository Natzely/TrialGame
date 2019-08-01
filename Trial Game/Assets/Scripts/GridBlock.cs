using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    public GameObject MoveSpace;
    public int MovementCost = 0;
    public bool Unpassable = false;

    private Dictionary<Enums.Player, MoveSpace> _movesSpaces;
    private Dictionary<Vector2, GridBlock> _neighbors;
    private SpriteRenderer _sR;
    private bool _gotNeighbors;
    private bool _showingGrid;
    private int _distance;

    public void CreateGrid(Enums.Player player, int distance, Vector2 gridPos)
    {
        distance -= MovementCost;
               
        if (Unpassable || distance < 0)
            return;

        _distance = distance;

        if (!_movesSpaces.ContainsKey(player))
        {
            var gO = Instantiate(MoveSpace, transform.position, Quaternion.identity);
            _movesSpaces[player] = gO.GetComponent<MoveSpace>();
            _movesSpaces[player].Player = player;
        }
        else
            _movesSpaces[player].Enable();

        _movesSpaces[player].GridPosition = gridPos;

        if (distance > 0)
        {
            _neighbors[Vector2.up]?.CreateGrid(player, distance, new Vector2(gridPos.x, gridPos.y - 1));
            _neighbors[Vector2.down]?.CreateGrid(player, distance, new Vector2(gridPos.x, gridPos.y + 1));
            _neighbors[Vector2.right]?.CreateGrid(player, distance, new Vector2(gridPos.x + 1, gridPos.y));
            _neighbors[Vector2.left]?.CreateGrid(player, distance, new Vector2(gridPos.x - 1, gridPos.y));
        }
    }

    private void Start()
    {
        _neighbors = new Dictionary<Vector2, GridBlock>();
        _sR = GetComponent<SpriteRenderer>();
        _movesSpaces = new Dictionary<Enums.Player, MoveSpace>();
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
        GetNeighbor(Vector2.up);
        GetNeighbor(Vector2.down);
        GetNeighbor(Vector2.left);
        GetNeighbor(Vector2.right);
    }

    private void GetNeighbor(Vector2 dir)
    {
        Vector2 startPos = transform.position.V2();
        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, 1f, LayerMask.GetMask("MapGrid"));
        if (hit.collider != null)
        {
            GameObject rhgo = hit.transform.gameObject;
            GridBlock grid = rhgo.GetComponent<GridBlock>();
            if (grid != null)
                _neighbors[dir] = grid;
        }
    }
}
