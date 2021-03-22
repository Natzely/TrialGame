using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridNeighbors : IEnumerable<GridBlock>
{
    Dictionary<Enums.NeighborDirection, GridBlock> _neighbors;
    GridBlock MidGridBlock;

    public GridNeighbors(GridBlock midGrid)
    {
        _neighbors = new Dictionary<Enums.NeighborDirection, GridBlock>();
        MidGridBlock = midGrid;
    }

    public void SetNeighbors(GridBlock up, GridBlock down, GridBlock left, GridBlock right)
    {
        _neighbors.Add(Enums.NeighborDirection.Up, up);
        _neighbors.Add(Enums.NeighborDirection.Down, down);
        _neighbors.Add(Enums.NeighborDirection.Left, left);
        _neighbors.Add(Enums.NeighborDirection.Right, right);
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

        foreach(var gB in _neighbors.Values)
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
        var rUnits = Right?.GetAlliedUnits(player).ToList();
        var lUnits = Left?.GetAlliedUnits(player).ToList();
        var uUnits = Up?.GetAlliedUnits(player).ToList();
        var dUnits = Down?.GetAlliedUnits(player).ToList();

        var totalUnits = rUnits.UnionNull(lUnits).UnionNull(uUnits).UnionNull(dUnits).ToList();
        int count = totalUnits.Count;
        return count;
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
            var behindDir = GridDirections.GetDirection(MidGridBlock.Position, behindGrid.Value);
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
            switch(direction)
            {
                case Enums.NeighborDirection.Up:
                    return Enums.NeighborDirection.Down;
                case Enums.NeighborDirection.Down:
                    return Enums.NeighborDirection.Up;
                case Enums.NeighborDirection.Right:
                    return Enums.NeighborDirection.Left;
                case Enums.NeighborDirection.Left:
                    return Enums.NeighborDirection.Right;
                default:
                    return Enums.NeighborDirection.Error;
            }
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
