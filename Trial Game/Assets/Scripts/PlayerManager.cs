using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;

    private GridBlock[,] _fullGrid;
    private int _gridSizeX;
    private int _gridSizeY;

    public bool GetDeleteMoveSpace(Enums.Player player)
    {
        var tmp = PlayerList.Where(x => x.Player == player).FirstOrDefault();
        return tmp.DeleteMoveSpace;
    }

    public int GetResetMoveSpace(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().ResetMoveSpacesAbove;
    }

    public void SetResetMoveSpace(Enums.Player player, int resetCount)
    {
        PlayerList.Where(x => x.Player == player).FirstOrDefault().ResetMoveSpacesAbove = resetCount;
    }

    public void SetDeleteMoveSpace(Enums.Player player, bool value)
    {
        PlayerList.Where(x => x.Player == player).FirstOrDefault().DeleteMoveSpace = value;
    }

    public void ResetPathMatrix(Enums.Player player)
    {
        PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid = new GridBlock[_gridSizeX, _gridSizeY];
    }

    public GridBlock GetMovementSpace(Enums.Player player, Vector2 gridPos)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault()?.MovementGrid[(int)gridPos.x, (int)gridPos.y];
    }

    public void UpdateMovementGrid(Enums.Player player, Vector2 pos, GridBlock gB)
    {
        var grid = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
        if (pos.x < grid.GetLength(0) && pos.y < grid.GetLength(0))
                PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)pos.x, (int)pos.y] = gB;
    }

    public GridBlock[,] GetPathMatrix(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault()?.MovementGrid;
    }

    public PlayerInfo GetPlayerInfo(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault();
    }

    public bool PathMatrixContains(Enums.Player player, Space ms)
    {
        var matrix = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
        int mLength = matrix.GetLength(0);
        for(int i = 0; i < mLength; i++)
        {
            for(int ii = 0; ii < mLength; ii++)
            {
                if (matrix[i, ii] == ms)
                    return true;
            }
        }

        return false;
    }

    public void PrintPlayerGrid(Enums.Player player)
    {
        GridBlock[,] grid;

        //if (player == Enums.Player.Player1)
            grid = GetPlayerInfo(player).MovementGrid;
       // else
            //grid = _fullGrid;

        int lengthX = grid.GetLength(0);
        int lengthY = grid.GetLength(1);
        string s = "\n";
        for (int y = 0; y < lengthY; y++)
        {
            for (int x = 0; x < lengthX; x++)
            {
                s += grid[x, y] == null ? "X" : "[]";
            }
            s += "\n";
        }

        Debug.Log(s);
    }

    public IEnumerable<GridBlock> CreatePath(Enums.Player player, GridBlock startPos, GridBlock endPos)
    {
        var pathList = PathFinder.CreatePath(player, startPos, endPos, GetPathMatrix(player));

        if (player == Enums.Player.Player1)
            GetPlayerInfo(player).MovementPath = pathList.ToList();

        return pathList;
    }

    public Vector2? GetNextUnit(Enums.Player player, UnitController unit)
    {
        if (unit == null)
        {
            var nextUnit = GetPlayerInfo(player).Units.Where(u => !u.OnCooldown && !u.Moving && !u.Attacked).FirstOrDefault();
            if (nextUnit != null)
            {
                return nextUnit.transform.position;
            }
        }
        else
        {
            var unitList = GetPlayerInfo(player).Units.ToList();
            var coolDownList = unitList.Where(u => !u.OnCooldown).ToList();
            if (coolDownList.Count > 0)
            {
                var unitIndex = unitList.IndexOf(unit) + 1;
                if (unitIndex >= unitList.Count)    
                    unitIndex = 0;

                var nextUnit = unitList.Skip(unitIndex).FirstOrDefault(u => !u.OnCooldown);
                if (nextUnit != null)
                    return nextUnit.Position;
            }
        }

        return null;
    }

    public IEnumerator CreateGridAsync(Enums.Player player, GridBlock gridBlock, int moveDistance, int attackDistance)
    {
        ResetPathMatrix(player);
        yield return new WaitUntil(() => true);
        gridBlock.CreateGrid(
            player,
            moveDistance,
            moveDistance > 0 ? attackDistance : attackDistance + 1,
            true
        );
        //PrintPlayerGrid(player);
    }
    public void CreateGrid(Enums.Player player, GridBlock gridBlock, int moveDistance, int attackDistance)
    {
        ResetPathMatrix(player);
        gridBlock.CreateGrid(
            player,
            moveDistance,
            moveDistance > 0 ? attackDistance : attackDistance + 1,
            true
        );
    }

    private void Start()
    {
        StartCoroutine(GetGridBlocks());
    }

    public void AddPlayerUnit(Enums.Player player, UnitController unit)
    {
        GetPlayerInfo(player).Units.Add(unit);
    }

    public void RemovePlayerUnit(Enums.Player player, UnitController unit)
    {
        GetPlayerInfo(player).Units.Remove(unit);
    }

    IEnumerator GetGridBlocks()
    {
        yield return new WaitUntil(() => FindObjectsOfType<GridBlock>().Length > 0);

        var allGridBlocks = FindObjectsOfType<GridBlock>();
        float minX = allGridBlocks.Min(gb => gb.Position.x);
        float maxX = allGridBlocks.Max(gb => gb.Position.x);
        float minY = allGridBlocks.Min(gb => gb.Position.y);
        float maxY = allGridBlocks.Max(gb => gb.Position.y);

        _gridSizeX = (int)(maxX - minX) + 1;
        _gridSizeY = (int)(maxY - minY) + 1;
        _fullGrid = new GridBlock[_gridSizeX, _gridSizeY];

        foreach (GridBlock gb in allGridBlocks)
        {
            int posX = (int)(gb.Position.x + maxX);
            int posY = (int)Mathf.Abs((gb.Position.y + minY));

            gb.GridPosition = new Vector2(posX, posY);

            _fullGrid[posX, posY] = gb.Unpassable ? null : gb;
        }
    }

    [Serializable]
    public class PlayerInfo
    {
        public Enums.Player Player;
        public GridBlock[,] MovementGrid;
        public List<GridBlock> MovementPath;
        public List<UnitController> Units;
        public bool DeleteMoveSpace;
        public bool HideGrid;
        public bool ResetNonPlayerGrid;
        public int ResetMoveSpacesAbove;
    }
}
