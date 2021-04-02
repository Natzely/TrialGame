using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : UnitManager
{
    public static float ACTIONTIMER = .1f;

    public bool HideActiveGrid { get; private set; }
    public float MinimapSquareSize
    {
        get 
        {
            float x = 0;
            float y = 0;
            if(Camera.main.aspect >= 1.7f)
            {
                x = 16;
                y = 9;
            }

            float x2 = FullGrid.GetLength(0);
            float y2 = FullGrid.GetLength(1);

            float x1 = x / x2 / 1.1f;
            float y1 = y / y2 / 1.1f;
            return Mathf.Min(x1, y1);
        }
    }

    private List<UnitController> _nextUnitList;

    public bool GetDeleteMoveSpace()
    {
        return PlayerInfo.DeleteMoveSpace;
    }

    public GridBlock[,] GetGridMatrix()
    {
        return PlayerInfo.BlockGrid;
    }

    public bool GridMatrixContains(GridBlock gB)
    {
        var matrix = PlayerInfo.BlockGrid;
        int mLength = matrix.GetLength(0);
        for(int i = 0; i < mLength; i++)
        {
            for(int ii = 0; ii < mLength; ii++)
            {
                if (matrix[i, ii] == gB)
                    return true;
            }
        }

        return false;
    }

    public void PrintPlayerGrid()
    {
        GridBlock[,] grid;

        grid = PlayerInfo.BlockGrid;

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

    public void UpdateBlockGrid(Vector2 pos, GridBlock gB, bool showOnGrid)
    {
        var grid = PlayerInfo.BlockGrid;
        if (pos.x < grid.GetLength(0) && pos.y < grid.GetLength(0))
        {
            if (showOnGrid)
                PlayerInfo.ActiveGrid.Add(gB);
            else
            {
                PlayerInfo.ActiveGrid.Remove(gB);
                gB = null;
            }
            PlayerInfo.BlockGrid[(int)pos.x, (int)pos.y] = gB;
        }
    }

    public void ActiveGrid_Update(GridBlock updateFrom)
    {
        var orderList = PlayerInfo.ActiveGrid
            .OrderByDescending(p => p.PlayerMoveDistance)
            .ThenByDescending(p => p.PlayerAttackDistance)
            .ToList();

        var loopList = orderList.Skip(1).ToList();
        loopList.Remove(updateFrom);

        foreach(GridBlock gB in loopList)
        {
            var bestN = gB.Neighbors.GetBestMoveNeighbor();
            if (bestN != null)
            { 
                var mParams = bestN.GetPlayerMoveParams();
                gB.SetGrid(bestN, mParams.FavorableTerrain, mParams.MoveDistance, mParams.MinAttackDis, mParams.MaxAttackDis);
            }
            else
            {
                gB.SetGrid(null, new List<Enums.GridBlockType>(), -1, -1, -1);
            }
        }
    }

    // When player selects a space to move to, hide the current move grid to show an attack grid
    public void ActiveGrid_Hide()
    {
        HideActiveGrid = true;
    }

    // If a player cancels after the above, show the move grid again.
    public void ActiveGrid_Show()
    {
        HideActiveGrid = false;
    }

    public UnitController GetNextUnit(UnitController unit)
    {
        if(_nextUnitList == null) // Grab all units that are already in the map at the start.
        {
            _nextUnitList = new List<UnitController>();
            var playerUnits = PlayerInfo.Units;
            foreach(var u in playerUnits)
            {
                var random = new System.Random();
                int insertAt = random.Next(_nextUnitList.Count());
                _nextUnitList.Insert(insertAt, u);
            }
        }

        if (AvailableUnits <= 0)
            return null;

        if (unit == null) // Select first available unit in the list 
        {
            var nextUnitPossible = _nextUnitList.Where(u => u != null && !u.OnCooldown && !u.Moving && !u.Attacked);
            if (nextUnitPossible.Count() > 0)
                return nextUnitPossible.First();
            else
                return GetNextUnitOnCooldown(unit);
        }
        else // Select the next unit based on the unit that is currently selected.
        {
            var coolDownList = _nextUnitList.Where(u => u != null && !u.OnCooldown && !u.Moving && !u.Attacked).ToList();
            if (coolDownList.Count > 0)
            {
                var unitIndex = coolDownList.IndexOf(unit) + 1;
                if (unitIndex >= coolDownList.Count)
                    unitIndex = 0;

                var nextUnit = coolDownList.Skip(unitIndex).FirstOrDefault(u => !u.OnCooldown);
                return nextUnit;
            }
            else
                return GetNextUnitOnCooldown(unit);
        }
    }

    public override IEnumerable<GridBlock> CreatePath(GridBlock startPos, GridBlock endPos)
    {
        var pathList = PathFinder.CreatePath(Player, startPos, endPos, GetGridMatrix());
        PlayerInfo.MovementPath = pathList.ToList();

        return pathList;
    }

    protected override void Awake()
    {
        base.Awake();

        var nonNullUnits = _startingUnits.Where(uC => uC != null).ToList();
        foreach (UnitController uC in nonNullUnits)
        {
            uC.Speed *= _globalVariables.UnitSpeedModifier;
            uC.Cooldown *= _globalVariables.UnitCooldownModifier;
            uC.Player = Player;
            uC.UnitManager = this;
        }
    }



    private UnitController GetNextUnitOnCooldown(UnitController afterUnit)
    {
        var nextUnitPossible = _nextUnitList.Where(u => u != null && !u.Moving && !u.Attacked);
        if (nextUnitPossible.Count() > 0)
        {
            var orderByCooldownLeft = nextUnitPossible.OrderBy(u => u.CooldownTimer).ToList();
            if (afterUnit == null)
                return nextUnitPossible.FirstOrDefault();
            else
            {
                var unitIndex = _nextUnitList.IndexOf(afterUnit) + 1;
                if (unitIndex >= _nextUnitList.Count)
                    unitIndex = 0;

                var nextUnit = _nextUnitList.Skip(unitIndex).FirstOrDefault();
                return nextUnit;
            }

        }

        return null;
    }

    public override void AddUnit(UnitController unit, bool addAtRandom = false)
    {
        if (_nextUnitList != null)
            _nextUnitList.Add(unit);

        base.AddUnit(unit);
    }
}
