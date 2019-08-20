using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;   

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
        PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid = new Space[gridSize*2+1, gridSize*2+1];
    }

    public Space GetMatrixSpace(Enums.Player player, Vector2 gridPos)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)gridPos.x, (int)gridPos.y];
    }

    public void UpdatePathMatrix(Enums.Player player, Vector2 pos, Space ms)
    {
        var grid = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
        if (pos.x < grid.GetLength(0) && pos.y < grid.GetLength(0))
                PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)pos.x, (int)pos.y] = ms;
    }

    public Space[,] GetPathMatrix(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
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
        int length = GetPlayerInfo(player).MovementGrid.GetLength(0);
        string s = "\n";
        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                s += GetPlayerInfo(player).MovementGrid[x, y] == null ? "X" : "[]";
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    public IEnumerable<MoveSpace> CreatePath(Enums.Player player, MoveSpace startPos, MoveSpace endPos)
    {
        var pathList = PathFinder.CreatePath(player, startPos, endPos, GetPathMatrix(player));
        return GetPlayerInfo(player).MovementPath = pathList.ToList();
    }

    [Serializable]
    public class PlayerInfo
    {
        public Enums.Player Player;
        public bool DeleteMoveSpace;
        public int ResetMoveSpacesAbove;
        public Space[,] MovementGrid;
        public List<MoveSpace> MovementPath;
    }
}
