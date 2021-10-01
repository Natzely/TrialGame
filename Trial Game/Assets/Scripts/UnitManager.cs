using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class UnitManager : MonoBehaviour
{
    public Enums.Player Player;
    public GameObject UnitHolder;
    public PolygonCollider2D CursorBoundaries;
    public static bool DebugLog = true;
    
    public GridBlock[,] FullGrid { get; set; }

    public int AvailableUnits { get { return PlayerInfo.Units.Where(u => u.Available).Count(); } }

    protected GlobalVariables _globalVariables;
    protected HashSet<UnitController> _startingUnits;

    private static string _debugFilePath;

    public PlayerInfo PlayerInfo { get; private set; }

    protected private int _gridSizeX;
    protected private int _gridSizeY;

    public abstract IEnumerable<MovePoint> CreatePath(GridBlock startPos, GridBlock endPos);

    public void ResetBlockGrid()
    {
        PlayerInfo.ActiveGrid.ToList().ForEach(aG => aG.Disable());
        PlayerInfo.ActiveGrid.Clear();
        PlayerInfo.MovementPath.Clear();
        PlayerInfo.BlockGrid = new GridBlock[_gridSizeX, _gridSizeY];
    }

    public virtual void AddUnit(UnitController unit, bool addAtRandom = false)
    {
        if (PlayerInfo.Units.Contains(unit))
            return;

        if (addAtRandom)
        {
            var r = new System.Random();
            int index = r.Next(PlayerInfo.Units.Count());
            PlayerInfo.Units.Insert(index, unit);
        }
        else
            PlayerInfo.Units.Add(unit);
    }

    public void RemoveUnit(UnitController unit)
    {
        if (unit)
            PlayerInfo.Units.Remove(unit);
    }

    public static void Log(string msg)
    {
        if (DebugLog)
        {
            using StreamWriter writer = File.AppendText(_debugFilePath);
            writer.WriteLine(msg);
        }
    }

    protected virtual void Awake()
    {
        PlayerInfo = new PlayerInfo();
        _startingUnits = new HashSet<UnitController>(UnitHolder.GetComponentsInChildren<UnitController>());
        _globalVariables = FindObjectOfType<GlobalVariables>();
        _debugFilePath = Application.persistentDataPath + "/Debug.txt";
    }

    private void Start()
    {

    }
}

