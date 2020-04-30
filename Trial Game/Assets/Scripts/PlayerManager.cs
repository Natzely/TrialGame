﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : UnitManager
{
    public GameObject PauseScreen;
    public float ActionTimer;    

    private List<UnitController> _nextUnitList;
    HashSet<GridBlock> _activeGrid;
    private bool _pause;
    private bool _isGamePaused;
    private double _actionTimer;
    private double _lastRealTime;

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

    public void PrintPlayerGrid(Enums.Player player)
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

    public void UpdateActiveGrid(GridBlock updateFrom)
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
                gB.SetGrid(null, mParams.Item1, mParams.Item2, mParams.Item3);
            }
            else
            {
                gB.SetGrid(null, -1, -1, -1);
            }
        }
    }

    public UnitController GetNextUnit(Enums.Player player, UnitController unit)
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

        _actionTimer = 0;

        var nonNullUnits = StartingUnits.Where(uC => uC != null).ToList();
        foreach (UnitController uC in nonNullUnits)
        {
            uC.Player = Player;
            uC.UnitManager = this;
        }
    }

    private void Update()
    {
        _pause = Input.GetButtonUp("Pause");

        if (_actionTimer <= 0)
        {
            if (_pause)
            {
                if (_isGamePaused)
                {
                    Debug.Log("Unpause Game");
                    Time.timeScale = 1;
                    PauseScreen.SetActive(false);
                    _isGamePaused = false;

                }
                else
                {
                    Debug.Log("Pause Game");
                    Time.timeScale = 0;
                    PauseScreen.SetActive(true);
                    _isGamePaused = true;
                }

                _actionTimer = ActionTimer;
            }
        }
        else
        {
            _actionTimer -= Time.realtimeSinceStartup - _lastRealTime;
        }

        _lastRealTime = Time.realtimeSinceStartup;
    }

    private UnitController GetNextUnitOnCooldown(UnitController afterUnit)
    {
        var nextUnitPossible = _nextUnitList.Where(u => u != null && !u.Moving && !u.Attacked);
        if (nextUnitPossible.Count() > 0)
        {
            var orderByCooldownLeft = nextUnitPossible.OrderBy(u => u.Cooldown).ToList();
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

    //public void PlayerUnitMoveDown(UnitController unit)
    //{
    //    PlayerInfo.Units.Remove(unit);
    //    PlayerInfo.Units.Add(unit);
    //}
}
