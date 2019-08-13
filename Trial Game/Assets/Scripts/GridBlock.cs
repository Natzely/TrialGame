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

    private Dictionary<Enums.Player, Space> _moveSpaces;
    private Dictionary<Enums.Player, Space> _attackSpaces;
    private Dictionary<Enums.Player, Space> _spaces;
    private GridNeighbors _neighbors;
    private GameObject _space;
    private SpriteRenderer _sR;
    private bool _gotNeighbors;
    private bool _showingGrid;

    public void CreateGrid(Enums.Player player, int moveDistance, int attackDistance, Vector2 gridPos)
    {
        if ((moveDistance <= 0 && attackDistance == 0) || 
            (_moveSpaces.ContainsKey(player) && _moveSpaces[player].Enabled))
            return;

        if (Unpassable)
            moveDistance = 0;

        if (moveDistance > 0)
        {
            _space = MoveSpace;
            _spaces = _moveSpaces;
            moveDistance -= MovementCost;
            if (_attackSpaces.ContainsKey(player))
                _attackSpaces[player].Disable();
        }
        else
        {
            _space = AttackSpace;
            _spaces = _attackSpaces;
            attackDistance -= 1;
        }

        if (!_spaces.ContainsKey(player))
        {
            var gO = Instantiate(_space, transform.position, Quaternion.identity);
            _spaces[player] = gO.GetComponent<Space>();
            _spaces[player].Player = player;
        }

        if (_spaces.ContainsKey(player))
        {
            try
            {
                _spaces[player].Enable();
                _spaces[player].GridPosition = gridPos;
            }
            catch
            {
                Debug.Log("");
            }
        }
        

        _neighbors.Up?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x, gridPos.y - 1));
        _neighbors.Down?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x, gridPos.y + 1));
        _neighbors.Right?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x + 1, gridPos.y));
        _neighbors.Left?.CreateGrid(player, moveDistance, attackDistance, new Vector2(gridPos.x - 1, gridPos.y));
    }

    private void Start()
    {
        _neighbors = new GridNeighbors();
        _sR = GetComponent<SpriteRenderer>();
        _moveSpaces = new Dictionary<Enums.Player, Space>();
        _attackSpaces = new Dictionary<Enums.Player, Space>();
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
