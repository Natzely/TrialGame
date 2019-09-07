using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;

    private GridBlock[,] _fullGrid;

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

    public void SetPathMatrix(Enums.Player player, int gridSize)
    {
        if (gridSize > 0)
            PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid = new GridBlock[gridSize * 2 + 1, gridSize * 2 + 1];
        else
            PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid = null;
    }

    public GridBlock GetMatrixSpace(Enums.Player player, Vector2 gridPos)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)gridPos.x, (int)gridPos.y];
    }

    public void UpdatePathMatrix(Enums.Player player, Vector2 pos, GridBlock gB)
    {
        var grid = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
        if (pos.x < grid.GetLength(0) && pos.y < grid.GetLength(0))
                PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)pos.x, (int)pos.y] = gB;
    }

    public GridBlock[,] GetPathMatrix(Enums.Player player)
    {
        if (player == Enums.Player.Player1)
            return PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
        else
            return _fullGrid;
    }

    public PlayerInfo GetPlayerInfo(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault();
    }

    public bool PathMatrixContains(Enums.Player player, MoveSpace ms)
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

        if (player == Enums.Player.Player1)
            grid = GetPlayerInfo(player).MovementGrid;
        else
            grid = _fullGrid;

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

    private void Start()
    {
        StartCoroutine(GetGridBlocks());
    }

    IEnumerator GetGridBlocks()
    {
        yield return new WaitUntil(() => FindObjectsOfType<GridBlock>().Length > 0);

        var allGridBlocks = FindObjectsOfType<GridBlock>();
        float minX = allGridBlocks.Min(gb => gb.Position.x);
        float maxX = allGridBlocks.Max(gb => gb.Position.x);
        float minY = allGridBlocks.Min(gb => gb.Position.y);
        float maxY = allGridBlocks.Max(gb => gb.Position.y);

        int gridSizeX = (int)(maxX - minX) + 1;
        int gridSizeY = (int)(maxY - minY) + 1;
        _fullGrid = new GridBlock[gridSizeX, gridSizeY];

        foreach (GridBlock gb in allGridBlocks)
        {
            int posX = (int)(gb.Position.x + maxX);
            int posY = (int)Mathf.Abs((gb.Position.y + minY));

            gb.FullGridPosition = new Vector2(posX, posY);

            _fullGrid[posX, posY] = gb.MovementCost > 1 ? null : gb;
        }
    }

    [Serializable]
    public class PlayerInfo
    {
        public Enums.Player Player;
        public GridBlock[,] MovementGrid;
        public List<GridBlock> MovementPath;
        public bool DeleteMoveSpace;
        public bool HideGrid;
        public int ResetMoveSpacesAbove;
    }
}
