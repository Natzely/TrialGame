using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;
    [HideInInspector] public GameObject PauseScreen;
    [HideInInspector] public bool DebugOn;
    [HideInInspector] public float ActionTimer;

    private GridBlock[,] _fullGrid;
    private List<UnitController> _nextUnitList;
    private bool _pause;
    private bool _isGamePaused;
    private int _gridSizeX;
    private int _gridSizeY;
    private double _actionTimer;
    private double _lastRealTime;

    public bool GetDeleteMoveSpace(Enums.Player player)
    {
        var tmp = PlayerList.Where(x => x.Player == player).FirstOrDefault();
        return tmp.DeleteMoveSpace;
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
        if (player == Enums.Player.Player1)
            return PlayerList.Where(x => x.Player == player).FirstOrDefault()?.MovementGrid;
        else
            return _fullGrid;
    }

    public PlayerInfo GetPlayerInfo(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault();
    }

    public bool PathMatrixContains(Enums.Player player, GridBlock gB)
    {
        var matrix = PlayerList.Where(x => x.Player == player).FirstOrDefault().MovementGrid;
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
        if(_nextUnitList == null) // Grab all units that are already in the map at the start.
        {
            _nextUnitList = new List<UnitController>();
            var playerUnits = GetPlayerInfo(player).Units;
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
            if(nextUnitPossible.Count() > 0)
                return nextUnitPossible.First().transform.position;
            else
                return GetNextUnitOnCooldown();
        }
        else // Select the next unit based on the unit that is currently selected.
        {
            var coolDownList = _nextUnitList.Where(u => !u.OnCooldown && !u.Moving && !u.Attacked).ToList();
            if (coolDownList.Count > 0)
            {
                var unitIndex = coolDownList.IndexOf(unit) + 1;
                if (unitIndex >= coolDownList.Count)
                    unitIndex = 0;

                var nextUnit = coolDownList.Skip(unitIndex).FirstOrDefault(u => !u.OnCooldown);
                if (nextUnit != null)
                    return nextUnit.Position;
            }
            else
                return GetNextUnitOnCooldown();
        }

        return null;
    }

    public IEnumerator CreateGridAsync(GridBlock start, Enums.Player player, GridBlock gridBlock, int moveDistance, int minAttackDistance, int maxAttackDistance)
    {
        ResetPathMatrix(player);
        yield return new WaitUntil(() => true);
        gridBlock.CreateGrid(
            start,
            player,
            moveDistance,
            minAttackDistance,
            moveDistance > 0 ? maxAttackDistance : maxAttackDistance + 1
        );
        //PrintPlayerGrid(player);
    }

    public void CreateGrid(GridBlock start, Enums.Player player, GridBlock gridBlock, int moveDistance, int minAttackDistnace, int attackDistance)
    {
        ResetPathMatrix(player);
        gridBlock.CreateGrid(
            start,
            player,
            moveDistance,
            minAttackDistnace,
            moveDistance > 0 ? attackDistance : attackDistance + 1
        );
    }

    private void Awake()
    {
        _actionTimer = 0;
    }

    private void Start()
    {
        StartCoroutine(GetGridBlocks());
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

    private Vector2? GetNextUnitOnCooldown()
    {
        var nextUnitPossible = _nextUnitList.Where(u => !u.Moving && !u.Attacked);
        if (nextUnitPossible.Count() > 0)
        {
            return nextUnitPossible.First().transform.position;
        }

        return null;
    }

    public void AddPlayerUnit(Enums.Player player, UnitController unit)
    {
        GetPlayerInfo(player).Units.Add(unit);
        if (_nextUnitList != null)
            _nextUnitList.Add(unit);
    }

    public void RemoveUnit(Enums.Player player, UnitController unit)
    {
        GetPlayerInfo(player).Units.Remove(unit);
    }

    public void PlayerUnitMoveDown(Enums.Player player, UnitController unit)
    {
        GetPlayerInfo(player).Units.Remove(unit);
        GetPlayerInfo(player).Units.Add(unit);
    }

    private IEnumerator GetGridBlocks()
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

        // Use the highest ABS value between the min and max for the list math 
        // I.E. - minX = -8.5 and maxX = 7.5, gridSizeX = 17 ((maxX - minX) + 1);
        // the '0' index for the grid is -8.5 + 8.5 = 0 andthe '16' index is 7.5 + 8.5
        maxX = Mathf.Max(maxX, Mathf.Abs(minX));
        maxY = Mathf.Max(maxY, Mathf.Abs(minY));

        foreach (GridBlock gb in allGridBlocks)
        {
            int posX = (int)(gb.Position.x + maxX);
            int posY = (int)(gb.Position.y + maxY);

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
    }
}
