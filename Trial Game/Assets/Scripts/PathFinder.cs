using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathFinder
{
    public static IEnumerable<MoveSpace> CreatePath(Enums.Player player, MoveSpace msStart, MoveSpace msEnd, Space[,] map)
    {
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
            MoveSpace ms = (MoveSpace)map[l.X, l.Y];
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

        return pathList;
    }

    private static List<Location> GetWalkableAdjacentSquares(int x, int y, Space[,] map)
    {
        var proposedLocations = new List<Location>()
        {
            new Location { X = x, Y = y - 1 },
            new Location { X = x, Y = y + 1 },
            new Location { X = x - 1, Y = y },
            new Location { X = x + 1, Y = y },
        };

        return proposedLocations.Where(l => l.Y < map.GetLength(0) && l.X < map.GetLength(0) && map[l.X, l.Y] != null && map[l.X, l.Y].GetType() == typeof(MoveSpace)).ToList();
    }

    private static int ComputeHScore(int x, int y, int targetX, int targetY)
    {
        return Math.Abs(targetX - x) + Math.Abs(targetY - y);
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
