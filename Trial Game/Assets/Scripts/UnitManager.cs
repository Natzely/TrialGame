using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UnitManager : MonoBehaviour
{
    public Enums.Player Player;
    public GameObject UnitHolder;
    public PolygonCollider2D CursorBoundaries;
    public bool DebugLog;
    
    [HideInInspector] public GridBlock[,] FullGrid;

    protected List<UnitController> _startingUnits;
    
    private int _gridSizeX;
    private int _gridSizeY;

    public PlayerInfo PlayerInfo { get; private set; }

    public abstract IEnumerable<GridBlock> CreatePath(GridBlock startPos, GridBlock endPos);

    public void ResetBlockGrid()
    {
        PlayerInfo.ActiveGrid.ToList().ForEach(aG => aG.Disable(Player));
        PlayerInfo.ActiveGrid.Clear();
        PlayerInfo.BlockGrid = new GridBlock[_gridSizeX, _gridSizeY];
    }

    public virtual void AddUnit(UnitController unit, bool addAtRandom = false)
    {
        if (PlayerInfo.Units.Contains(unit))
            return;

        if (addAtRandom)
        {
            var r = new System.Random();
            int index = r.Next(PlayerInfo.Units.Count());
            PlayerInfo.Units.Insert(index, unit);
        }
        else
            PlayerInfo.Units.Add(unit);
    }

    public void RemoveUnit(UnitController unit)
    {
        PlayerInfo.Units.Remove(unit);
    }

    public void Log(string msg)
    {
        if (DebugLog)
            Debug.Log(msg);
    }

    protected virtual void Awake()
    {
        PlayerInfo = new PlayerInfo();
        _startingUnits = UnitHolder.GetComponentsInChildren<UnitController>().ToList();
    }

    private void Start()
    {
        StartCoroutine(GetGridBlocks());
    }

    private IEnumerator GetGridBlocks()
    {
        yield return new WaitUntil(() => FindObjectsOfType<GridBlock>().Length > 0); // This is to wait until the GridBlock scripts are available to use

        float minX = CursorBoundaries.points[1].x;
        float maxX = CursorBoundaries.points[3].x;
        float minY = CursorBoundaries.points[1].y;
        float maxY = CursorBoundaries.points[3].y;

        _gridSizeX = (int)(maxX - minX) + 1;
        _gridSizeY = (int)(maxY - minY) + 1;
        FullGrid = new GridBlock[_gridSizeX, _gridSizeY];

        // Use the highest ABS value between the min and max for the list math 
        // I.E. - minX = -8.5 and maxX = 7.5, gridSizeX = 17 ((maxX - minX) + 1);
        // the '0' index for the grid is -8.5 + 8.5 = 0 andthe '16' index is 7.5 + 8.5
        maxX = Mathf.Max(maxX, Mathf.Abs(minX));
        maxY = Mathf.Max(maxY, Mathf.Abs(minY));

        var allGridBlocks = FindObjectsOfType<GridBlock>();
        foreach (GridBlock gb in allGridBlocks)
        {
            if (gb != null && gb.Position.InsideSquare(new Vector2(minX, minY), new Vector2(maxX, maxY)))
            {
                int posX = (int)(gb.Position.x + maxX);
                int posY = (int)(gb.Position.y + maxY);

                gb.GridPosition = new Vector2(posX, posY);

                FullGrid[posX, posY] = gb.Unpassable ? null : gb;
            }
        }

        ResetBlockGrid();
    }
}

