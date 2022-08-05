using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathFinder
{
    private const int MOVEMENTCOSTMODIFIER = 1;

    public static IEnumerable<MovePoint> CreatePath(Enums.Player player, GridBlock gbStart, GridBlock gbTarget, GridBlock[,] map)
    {
        Location current = null;
        Location start = null;
        Location target = null;
        Location bestLoc = new Location() { G = 9999, H = 9999, F = 9999 };

        start = new Location { X = (int)gbStart.GridPosition.x, Y = (int)gbStart.GridPosition.y };
        target = new Location { X = (int)gbTarget.GridPosition.x, Y = (int)gbTarget.GridPosition.y };

        var openList = new List<Location>();
        var closedList = new List<Location>();
        var gridList = new List<GridBlock>();
        var pathList = new List<MovePoint>();
        int g = 0;

        // start by adding the original position to the open list
        openList.Add(start);

        while (openList.Count > 0)
        {
            // Check if the target has been added to the list
            current = openList.FirstOrDefault(l => l.H == 0);
            if (current == null)
            {
                // get the square with the lowest F score
                var lowest = openList.Min(l => l.F);
                current = openList.FirstOrDefault(l => l.F == lowest);
            }

            // add the current square to the closed list
            closedList.Add(current);

            // remove it from the open list
            openList.Remove(current);

            // if we added the destination to the closed list, we've found a path
            if (current.X == target.X && current.Y == target.Y)
                break;

            var adjacentSquares = GetWalkableAdjacentSquares(player, current.X, current.Y, map, gbTarget);
            g++;

            foreach (Location adjacentSquare in adjacentSquares)
            {
                int asX = adjacentSquare.X;
                int asY = adjacentSquare.Y;

                // Skip if the square has a unit that doesn't belong to the player and isn't the target of movement;
                if (map[asX, asY].CurrentUnit != null && map[asX, asY].CurrentUnit.Player != player &&
                    !(asX == target.X && asY == target.Y))
                    continue;

                // if this adjacent square is already in the closed list, ignore it
                if (closedList.FirstOrDefault(l => l.X == asX
                        && l.Y == asY) != null)
                    continue;

                // if it's not in the open list...
                if (openList.FirstOrDefault(l => l.X == asX
                        && l.Y == asY) == null)
                {
                    GridBlock gb = map[asX, asY];
                    // This is to pursade the pathing to use paths that don't use blocks with units in them.
                    int scoreAddition = gb.CurrentUnit ? 1 : 0;
                    // compute its score, set the parent
                    adjacentSquare.G = g;
                    adjacentSquare.H = ComputeHScore(asX, asY, target.X, target.Y) + scoreAddition;
                    try
                    {
                        var currentUnit = gbStart.CurrentUnit;
                        var gridMove = map[asX, asY];
                        var type = gridMove.Type;
                        int moveCost = currentUnit.CheckGridMoveCost(type);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H + (moveCost * MOVEMENTCOSTMODIFIER);
                        adjacentSquare.Parent = current;
                    }
                    catch
                    {
                        Debug.Log("");
                    }

                    //Debug.Log($"Current Square | {adjacentSquare.X},{adjacentSquare.Y} | G: {adjacentSquare.G} H: {adjacentSquare.H} F: {adjacentSquare.F}");

                    // Update bestLoc incase it's needed
                    if (adjacentSquare.H < bestLoc.H && !gb.CurrentUnit)
                        bestLoc = adjacentSquare;

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
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;// - map[asX, asY].MovementCost;
                        adjacentSquare.Parent = current;
                    }
                }
            }
        }

        // Check if the target was reached, if the target was a far away attack space it 
        // messxes up the pathing since it can't make a path to it. Just use the next best 
        // green space to path to.
        if (current != null && player == Enums.Player.Player1)
        {
            GridBlock gB = map[current.X, current.Y];
            if (gB != gbTarget)
            {
                GridBlock reachableTarget = map[bestLoc.X, bestLoc.Y];
                var reachablePath = CreatePath(player, gbStart, reachableTarget, map);
                return reachablePath;
            }
        }

        Vector2? lastDir = null;
        Vector2? dir = null;
        GridBlock pB = null;

        while (current != null)
        {
            GridBlock gB = map[current.X, current.Y];

            if ((player == Enums.Player.Player1 && gB.ActiveSpace != Enums.ActiveSpace.Move) ||
                (player == Enums.Player.Player2 && gB == gbTarget))
            {
                current = current.Parent;
                continue;
            }


            gridList.Add(gB);

            if (current.Parent != null)
            {
                pB = map[current.Parent.X, current.Parent.Y];

                dir = gB.Position - pB.Position;
                // Since the first lastDir is null, it will create an arrow spacefd
                if (player == Enums.Player.Player1)
                    gB.UpdatePathState(dir.Value, lastDir);
            }
            else if (player == Enums.Player.Player1)
            {
                // Reached the first space, make it the start
                gB.UpdatePathState(new Vector2(0, 0), lastDir);
            }

            lastDir = dir;
            current = current.Parent;
        }

        gridList.Reverse();
        foreach (GridBlock gB in gridList)
            pathList.Add(gB.ToMovePoint());

        return pathList;

    }

    private static List<Location> GetWalkableAdjacentSquares(Enums.Player player, int x, int y, GridBlock[,] map, GridBlock target)
    {
        List<Location> returnList = new List<Location>();
        var proposedLocations = new List<Location>()
        {
            new Location { X = x, Y = y - 1 },
            new Location { X = x, Y = y + 1 },
            new Location { X = x - 1, Y = y },
            new Location { X = x + 1, Y = y },
        };

        foreach (var loc in proposedLocations)
        {
            if (loc.X < 0 || loc.Y < 0)
                continue;
            if (loc.X >= map.GetLength(0) || loc.Y >= map.GetLength(1))
                continue;
            var gridBlock = map[loc.X, loc.Y];
            if (gridBlock == null || gridBlock.Unpassable)
                continue;
            //if (player == Enums.Player.Player2 && gridBlock.IsCurrentUnitEnemy(player) && gridBlock.CurrentUnit != target)
              //  continue;
            if (player == Enums.Player.Player1 && gridBlock.ActiveSpace == Enums.ActiveSpace.Attack && gridBlock != target)
                continue;
            if (player == Enums.Player.Player1 && gridBlock == target && gridBlock.CurrentUnit)
                continue;
            var aS = gridBlock.ActiveSpace;
            if (player == Enums.Player.Player1 && aS == Enums.ActiveSpace.Inactive)
                continue;

            returnList.Add(loc);
        }

        returnList.Shuffle();
        return returnList;
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
