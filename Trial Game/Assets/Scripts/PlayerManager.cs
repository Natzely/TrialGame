using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : UnitManager
{
    public static float ACTIONTIMER = .1f;
    public GameObject Minimap_UnitIcons;
    public GameObject Minimap_TileIcons;

    public CursorController Cursor { get { return _cC; } }

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
    private CursorController _cC;

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
                //var (uC, moveDistance) = bestN.GetPlayerMoveParams();
                gB.SetGrid(bestN, bestN.PlayerActiveUnit);//, moveDistance);
            }
            else
            {
                gB.SetGrid(null, null);
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

    public override IEnumerable<MovePoint> CreatePath(GridBlock startPos, GridBlock endPos)
    {
        var pathList = PathFinder.CreatePath(Player, startPos, endPos, GetGridMatrix());
        PlayerInfo.MovementPath = pathList.ToList();

        return pathList;
    }

    public override void InitializeUnits()
    {
        base.InitializeUnits();
        var nonNullUnits = Units.Where(uC => uC != null).ToList();
        foreach (UnitController uC in nonNullUnits)
        {
            uC.enabled = true;
            uC.gameObject.name = $"P{((int)Player)}_" + uC.gameObject.name;
            uC.enabled = true;
            uC.Player = Player;
            uC.Speed *= _globalVariables.UnitSpeedModifier;
            uC.Cooldown *= _globalVariables.UnitCooldownModifier;
            uC.UnitManager = this;
            uC.BoxCollider.enabled = true;
            uC.DefaultLook = 1;
            uC.HiddenOverlay.SetActive(true);
        }

    }

    private void Start()
    {
        StartCoroutine(GetGridBlocks());
        _cC = FindObjectOfType<CursorController>();

        if (InitializeUnitsAtStart)
            InitializeUnits();
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

    private IEnumerator GetGridBlocks()
    {
        yield return new WaitUntil(() => FindObjectsOfType<GridBlock>().Length > 0); // This is to wait until the GridBlock scripts are available to use

        float minX = CursorBoundaries.points[1].x;
        float maxX = CursorBoundaries.points[3].x;
        float minY = CursorBoundaries.points[1].y;
        float maxY = CursorBoundaries.points[3].y;

        _gridSizeX = (int)(maxX - minX) + 1;
        _gridSizeY = (int)(maxY - minY) + 1;
        FullGrid = new GridBlock[_gridSizeX, _gridSizeY];

        // Use the highest ABS value between the min and max for the list math 
        // I.E. - minX = -8.5 and maxX = 7.5, gridSizeX = 17 ((maxX - minX) + 1);
        // the '0' index for the grid is -8.5 + 8.5 = 0 andthe '16' index is 7.5 + 8.5
        maxX = Mathf.Max(maxX, Mathf.Abs(minX));
        maxY = Mathf.Max(maxY, Mathf.Abs(minY));

        var allGridBlocks = FindObjectsOfType<GridBlock>();
        foreach (GridBlock gb in allGridBlocks)
        {
            if (gb != null && gb.Position.InsideSquare(new Vector2(minX, minY), new Vector2(maxX, maxY)))
            {
                int posX = (int)(gb.Position.x + maxX);
                int posY = (int)(gb.Position.y + maxY);

                gb.GridPosition = new Vector2(posX, posY);
                gb.PlayerManager = this;
                gb.Cursor = _cC;

                try
                {
                    FullGrid[posX, posY] = gb.Unpassable ? null : gb;
                }
                catch (Exception ex)
                {
                    DebugLogger.Instance.Log(ex.Message);
                }
            }
        }

        ResetBlockGrid();
    }
}
