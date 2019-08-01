using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;   

    public bool GetDeleteMoveSpace(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().DeleteMoveSpace;
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
        PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid = new MoveSpace[gridSize*2+1, gridSize*2+1];
    }

    public MoveSpace GetMatrixItem(Enums.Player player, Vector2 gridPos)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)gridPos.x, (int)gridPos.y];
    }

    public void UpdatePathMatrix(Enums.Player player, Vector2 pos, MoveSpace ms)
    {
        var grid = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
        if (pos.x < grid.GetLength(0) && pos.y < grid.GetLength(0))
            PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid[(int)pos.x, (int)pos.y] = ms;
    }

    public MoveSpace[,] GetPathMatrix(Enums.Player player)
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

    public IEnumerable<MoveSpace> CreatePath(Enums.Player player, MoveSpace msStart, MoveSpace msEnd)
    {
        MoveSpace[,] map = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;

        Location current = null;
        var start = new Location { X = (int)msStart.GridPosition.x, Y = (int)msStart.GridPosition.y };
        var target = new Location { X = (int)msEnd.GridPosition.x, Y = (int)msEnd.GridPosition.y };
        var openList = new List<Location>();
        var closedList = new List<Location>();
        var pathList = new List<MoveSpace>();
        int g = 0;

        // start by adding the original position to the open list
        openList.Add(start);

        while (openList.Count > 0)
        {
            // get the square with the lowest F score
            var lowest = openList.Min(l => l.F);
            current = openList.First(l => l.F == lowest);
            
            // add the current square to the closed list
            closedList.Add(current);

            // remove it from the open list
            openList.Remove(current);

            // if we added the destination to the closed list, we've found a path
            if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                break;

            var adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, map);
            g++;

            foreach (var adjacentSquare in adjacentSquares)
            {
                // if this adjacent square is already in the closed list, ignore it
                if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X
                        && l.Y == adjacentSquare.Y) != null)
                    continue;

                // if it's not in the open list...
                if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                        && l.Y == adjacentSquare.Y) == null)
                {
                    // compute its score, set the parent
                    adjacentSquare.G = g;
                    adjacentSquare.H = ComputeHScore(adjacentSquare.X,
                        adjacentSquare.Y, target.X, target.Y);
                    adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                    adjacentSquare.Parent = current;

                    // and add it to the open list
                    openList.Insert(0, adjacentSquare);
                }
                else
                {
                    // test if using the current G score makes the adjacent square's F score
                    // lower, if yes update the parent because it means it's a better path
                    if (g + adjacentSquare.H < adjacentSquare.F)
                    {
                        adjacentSquare.G = g;
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;
                    }
                }
            }
        }

        MoveSpace lastMS = null;
        Vector2 lastPos = new Vector2(0, 0);
        Vector2 lastDir = new Vector2(0, 0);
        int count = -1;
        foreach (Location l in closedList)
        {
            MoveSpace ms = map[l.X, l.Y];
            pathList.Add(ms);
            if (lastMS != null)
            {
                Vector2 dir = ms.Position - lastPos;
                lastMS.MoveState(lastDir, dir, count);
                lastDir = dir;
            }

            count++;
            lastPos = ms.Position;
            lastMS = ms;
        }

        lastMS.MoveState(lastDir, null, count);

        return PlayerList.Where(p => p.Player == player).FirstOrDefault().MovementPath = pathList;
    }

    private List<Location> GetWalkableAdjacentSquares(int x, int y, MoveSpace[,] map)
    {
        var proposedLocations = new List<Location>()
        {
            new Location { X = x, Y = y - 1 },
            new Location { X = x, Y = y + 1 },
            new Location { X = x - 1, Y = y },
            new Location { X = x + 1, Y = y },
        };

        return proposedLocations.Where(l => l.Y < map.GetLength(0) && l.X < map.GetLength(0) && map[l.X, l.Y] != null).ToList();
    }

    private int ComputeHScore(int x, int y, int targetX, int targetY)
    {
        return Math.Abs(targetX - x) + Math.Abs(targetY - y);
    }

    [Serializable]
    public class PlayerInfo
    {
        public Enums.Player Player;
        public bool DeleteMoveSpace;
        public int ResetMoveSpacesAbove;
        public MoveSpace[,] MovementGrid;
        public List<MoveSpace> MovementPath;
    }

    class Location
    {
        public int X;
        public int Y;
        public int F;
        public int G;
        public int H;
        public Location Parent;
    }
}
