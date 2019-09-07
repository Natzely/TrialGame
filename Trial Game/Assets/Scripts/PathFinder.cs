﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathFinder
{
    public static IEnumerable<GridBlock> CreatePath(Enums.Player player, GridBlock msStart, GridBlock msEnd, GridBlock[,] map)
    {
        Location current = null;
        Location start = null;
        Location target = null;

        if (player == Enums.Player.Player1)
        {
            start = new Location { X = (int)msStart.PlayerGridPosition.x, Y = (int)msStart.PlayerGridPosition.y };
            target = new Location { X = (int)msEnd.PlayerGridPosition.x, Y = (int)msEnd.PlayerGridPosition.y };
        }
        else
        {
            start = new Location { X = (int)msStart.FullGridPosition.x, Y = (int)msStart.FullGridPosition.y };
            target = new Location { X = (int)msEnd.FullGridPosition.x, Y = (int)msEnd.FullGridPosition.y };
        }

        var openList = new List<Location>();
        var closedList = new List<Location>();
        var pathList = new List<GridBlock>();
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

            var adjacentSquares = GetWalkableAdjacentSquares(player, current.X, current.Y, map);
            g++;

            foreach (Location adjacentSquare in adjacentSquares)
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

        Vector2? lastDir = null;
        Vector2? dir = null;
        GridBlock pB = null;

        while (current != null)
        {
            GridBlock gB = map[current.X, current.Y];
            pathList.Add(gB);

            if (current.Parent != null)
            {
                pB = map[current.Parent.X, current.Parent.Y];

                dir = gB.Position - pB.Position;
                // Since the first lastDir is null, it will create an arrow spacefd
                if (player == Enums.Player.Player1)
                    gB.UpdateMoveSpaceState(player, dir.Value, lastDir);
            }
            else if (player == Enums.Player.Player1)
            {
                // Reached the first space, make it the start
                gB.UpdateMoveSpaceState(player, new Vector2(0, 0), lastDir);
            }

            lastDir = dir;
            current = current.Parent;
        }

        pathList.Reverse();
        return pathList;
    }

    private static List<Location> GetWalkableAdjacentSquares(Enums.Player player, int x, int y, GridBlock[,] map)
    {
        var proposedLocations = new List<Location>()
        {
            new Location { X = x, Y = y - 1 },
            new Location { X = x, Y = y + 1 },
            new Location { X = x - 1, Y = y },
            new Location { X = x + 1, Y = y },
        };

        return proposedLocations.Where(l =>
            l.X >= 0 && l.Y >= 0 &&
            l.X < map.GetLength(0) && l.Y < map.GetLength(1) &&
            map[l.X, l.Y] != null &&
            (map[l.X, l.Y].CurrentUnit == null || map[l.X, l.Y].CurrentUnit.Player == player) &&
            !map[l.X, l.Y].Unpassable
         ).ToList();
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