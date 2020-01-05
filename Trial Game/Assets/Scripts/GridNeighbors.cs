﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridNeighbors : IEnumerable<GridBlock>
{
    List<GridBlock> _neighbors;

    public GridNeighbors()
    {
        _neighbors = new List<GridBlock>();
    }

    public void SetNeighbors(GridBlock up, GridBlock down, GridBlock left, GridBlock right)
    {
        _neighbors.Add(up);
        _neighbors.Add(down);
        _neighbors.Add(left);
        _neighbors.Add(right);
    }

    public GridBlock Up
    {
        get { return _neighbors[0]; }
    }

    public GridBlock Down
    {
        get { return _neighbors[1]; }
    }

    public GridBlock Left
    {
        get { return _neighbors[2]; }
    }

    public GridBlock Right
    {
        get { return _neighbors[3]; }
    }

    public IEnumerable<GridBlock> OrderByDistance(GridBlock from)
    {
        try
        {
            return _neighbors
                  .Where  (g => g != null)
                  .OrderBy(g => Vector3.Distance(g.transform.position, from.transform.position));
        }
        catch
        {
            Debug.LogError("");
            return null;
        }
    }

    public IEnumerator<GridBlock> GetEnumerator()
    {
        return _neighbors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
