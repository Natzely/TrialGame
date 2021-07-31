using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;

public class GridNeighbors : IEnumerable<GridBlock>
{
    private readonly Dictionary<Enums.NeighborDirection, GridBlock> _neighbors;
    private readonly GridBlock _midGridBlock;

    public GridNeighbors(GridBlock midGrid)
    {
        _neighbors = new Dictionary<Enums.NeighborDirection, GridBlock>();
        _midGridBlock = midGrid;
        SetNeighbors();
    }

    public void SetNeighbors()// GridBlock up, GridBlock down, GridBlock left, GridBlock right)
    {
        _neighbors.Add(Enums.NeighborDirection.Up, GetNeighbor(Vector2.up));// up);
        _neighbors.Add(Enums.NeighborDirection.Down, GetNeighbor(Vector2.down));// down);
        _neighbors.Add(Enums.NeighborDirection.Left, GetNeighbor(Vector2.left));// left);
        _neighbors.Add(Enums.NeighborDirection.Right, GetNeighbor(Vector2.right));// right);
    }

    //private void GetNeighbors()
    //{
    //    _neighbors.SetNeighbors(
    //        GetNeighbor(Vector2.up),
    //        GetNeighbor(Vector2.down),
    //        GetNeighbor(Vector2.left),
    //        GetNeighbor(Vector2.right)
    //    );
    //}

    private GridBlock GetNeighbor(Vector2 dir)
    {
        var neighborPostion = _midGridBlock.Position + dir;
        var neighborName = String.Format(Strings.GridblockName, neighborPostion.x, neighborPostion.y);
        var neighbor = GameObject.Find(neighborName);
        //Vector2 startPos = _midGridBlock.transform.position.V2();
        //RaycastHit2D hit = Physics2D.Raycast(startPos, dir, 1f, LayerMask.GetMask("GridBlock"));
        //if (hit.collider != null)
        if(neighbor != null)
        {
            GridBlock grid = neighbor.GetComponent<GridBlock>();
            return grid;
        }

        return null;
    }

    public GridBlock Up
    {
        get { return _neighbors[Enums.NeighborDirection.Up]; }
    }

    public GridBlock Down
    {
        get { return _neighbors[Enums.NeighborDirection.Down]; }
    }

    public GridBlock Left
    {
        get { return _neighbors[Enums.NeighborDirection.Left]; }
    }

    public GridBlock Right
    {
        get { return _neighbors[Enums.NeighborDirection.Right]; }
    }

    public GridBlock GetBestMoveNeighbor()
    {
        GridBlock rGB = null;
        (List<Enums.GridBlockType> faveTerrain, int move, int min, int max) param = (new List<Enums.GridBlockType>(), -1, -1, -1);

        foreach (var gB in _neighbors.Values)
        {
            if (gB != null)
            {
                var values = gB.GetPlayerMoveParams();
                if (values.MoveDistance > param.move)
                {
                    param = values;
                    rGB = gB;
                }
                else if (values.MaxAttackDis > param.max)
                {
                    param = values;
                    rGB = gB;
                }
            }
        }

        return rGB;
    }

    public int GetAlliedUnits(Enums.Player player)
    {
        var rUnits = CheckAlliedUnits(Right, player).ToList();
        var lUnits = CheckAlliedUnits(Left, player).ToList();
        var uUnits = CheckAlliedUnits(Up, player).ToList();
        var dUnits = CheckAlliedUnits(Down, player).ToList();

        var totalUnits = rUnits.UnionNull(lUnits).UnionNull(uUnits).UnionNull(dUnits).ToList();
        int count = totalUnits.Count;
        return count;
    }

    private IEnumerable<UnitController> CheckAlliedUnits(GridBlock gB, Enums.Player player)
    {
        if (gB)
            return gB.GetAlliedUnits(player);
        else
            return Enumerable.Empty<UnitController>();
    }

    public IEnumerable<GridBlock> OrderByDistance(GridBlock from, bool onlyAvailable = false)
    {
        if (from == null)
            return null;

        var neighbors = _neighbors.Values.Where(n => n != null).ToList();
        if (onlyAvailable)
            neighbors = AvailableNeighbors().ToList();
        var orderedNeighbors = neighbors.OrderBy(g => g.transform.position.GridDistance(from.transform.position)).ToList();

        return orderedNeighbors;
    }

    public IEnumerable<GridBlock> AvailableNeighbors(Vector2? behindGrid = null)
    {
        if (behindGrid == null)
            return _neighbors
                .Where(g => 
                    g.Value != null && 
                    g.Value.CurrentUnit == null &&
                    !g.Value.Unpassable)
                .Select(x => x.Value);
        else
        {
            // behindGrid is used to show which grid the unit is trying to move away from
            var behindDir = GridDirections.GetDirection(_midGridBlock.Position, behindGrid.Value);
            var oppDir = GridDirections.OppositeOff(behindDir);
            List<GridBlock> aNeighbors = new List<GridBlock>();
            // Put the opposite direction first if available
            GridBlock oppGrid = _neighbors[oppDir];
            if (oppGrid.CurrentUnit == null && !oppGrid.Unpassable)
                aNeighbors.Add(_neighbors[oppDir]);
            // Add the rest of the grids
            aNeighbors.AddRange(
                _neighbors
                .Where(n => 
                    n.Key != behindDir && 
                    n.Key != oppDir && 
                    n.Value != null && 
                    n.Value.CurrentUnit == null &&
                    !n.Value.Unpassable).Select(n => n.Value));
            return aNeighbors;
        }
    }

    public IEnumerator<GridBlock> GetEnumerator()
    {
        return _neighbors.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static class GridDirections
    {
        public static Enums.NeighborDirection OppositeOff(Enums.NeighborDirection direction)
        {
            return direction switch
            {
                Enums.NeighborDirection.Up    => Enums.NeighborDirection.Down,
                Enums.NeighborDirection.Down  => Enums.NeighborDirection.Up,
                Enums.NeighborDirection.Right => Enums.NeighborDirection.Left,
                Enums.NeighborDirection.Left  => Enums.NeighborDirection.Right,
                _                             => Enums.NeighborDirection.Error,
            };
        }

        public static Enums.NeighborDirection GetDirection(Vector2 main, Vector2 offset)
        {
            Vector2 dif = offset - main;

            if (dif.y > 0)
                return Enums.NeighborDirection.Up;
            else if (dif.y < 0)
                return Enums.NeighborDirection.Down;
            else if (dif.x > 0)
                return Enums.NeighborDirection.Right;
            else
                return Enums.NeighborDirection.Left;

        }
    }
}
