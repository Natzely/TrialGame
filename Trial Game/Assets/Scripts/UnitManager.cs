using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UnitManager : MonoBehaviour
{
    public Enums.Player Player;
    public List<UnitController> StartingUnits;
    public GridBlock[,] FullGrid;

    private int _gridSizeX;
    private int _gridSizeY;

    public PlayerInfo PlayerInfo { get; private set; }

    public abstract IEnumerable<GridBlock> CreatePath(GridBlock startPos, GridBlock endPos);

    public bool IsGridActive(Vector2 gridPos)
    {
        return PlayerInfo.BlockGrid[(int)gridPos.x, (int)gridPos.y] != null;
    }

    public void UpdateBlockGrid(Vector2 pos, GridBlock gB)
    {
        var grid = PlayerInfo.BlockGrid;
        if (pos.x < grid.GetLength(0) && pos.y < grid.GetLength(0))
        {
            if (gB != null)
                PlayerInfo.ActiveGrids.Add(gB);
            else
                PlayerInfo.ActiveGrids.Remove(gB);

            PlayerInfo.BlockGrid[(int)pos.x, (int)pos.y] = gB;
        }
    }

    public void ResetBlockGrid()
    {
        PlayerInfo.ActiveGrids.ForEach(aG => aG.Disable(Player));
        PlayerInfo.ActiveGrids.Clear();
        PlayerInfo.BlockGrid = new GridBlock[_gridSizeX, _gridSizeY];
    }

    public IEnumerator CreateGridAsync(GridBlock start, Enums.Player player, GridBlock gridBlock, int moveDistance, int minAttackDistance, int maxAttackDistance)
    {
        ResetBlockGrid();
        yield return new WaitUntil(() => true);
        gridBlock.CreateGrid(
            start,
            this,
            moveDistance,
            moveDistance > 0 ? maxAttackDistance : maxAttackDistance + 1
        );
        //PrintPlayerGrid(player);
    }

    public void CreateGrid(GridBlock start, Enums.Player player, GridBlock gridBlock, int moveDistance, int minAttackDistnace, int attackDistance)
    {
        ResetBlockGrid();
        gridBlock.CreateGrid(
            start,
            this,
            moveDistance,
            moveDistance > 0 ? attackDistance : attackDistance + 1
        );
    }

    public virtual void AddUnit(UnitController unit, bool addAtRandom = false)
    {
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

    protected virtual void Awake()
    {
        PlayerInfo = new PlayerInfo();
    }

    private void Start()
    {
        StartCoroutine(GetGridBlocks());
    }

    private IEnumerator GetGridBlocks()
    {
        yield return new WaitUntil(() => FindObjectsOfType<GridBlock>().Length > 0);

        var allGridBlocks = FindObjectsOfType<GridBlock>();
        float minX = allGridBlocks.Min(gb => gb.Position.x);
        float maxX = allGridBlocks.Max(gb => gb.Position.x);
        float minY = allGridBlocks.Min(gb => gb.Position.y);
        float maxY = allGridBlocks.Max(gb => gb.Position.y);

        _gridSizeX = (int)(maxX - minX) + 1;
        _gridSizeY = (int)(maxY - minY) + 1;
        FullGrid = new GridBlock[_gridSizeX, _gridSizeY];

        // Use the highest ABS value between the min and max for the list math 
        // I.E. - minX = -8.5 and maxX = 7.5, gridSizeX = 17 ((maxX - minX) + 1);
        // the '0' index for the grid is -8.5 + 8.5 = 0 andthe '16' index is 7.5 + 8.5
        maxX = Mathf.Max(maxX, Mathf.Abs(minX));
        maxY = Mathf.Max(maxY, Mathf.Abs(minY));

        foreach (GridBlock gb in allGridBlocks)
        {
            int posX = (int)(gb.Position.x + maxX);
            int posY = (int)(gb.Position.y + maxY);

            gb.GridPosition = new Vector2(posX, posY);

            FullGrid[posX, posY] = gb.Unpassable ? null : gb;
        }
    }
}

