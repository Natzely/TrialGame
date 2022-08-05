using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class UnitManager : MonoBehaviour
{
    public Enums.Player Player;
    public LevelManager SceneManager;
    public GameObject UnitHolder;
    public GameObject DeadUnitHolder;
    public PolygonCollider2D CursorBoundaries;
    [SerializeField] protected bool InitializeUnitsAtStart;

    public PlayerInfo PlayerInfo { get; private set; }
    public GridBlock[,] FullGrid { get; set; }
    public List<UnitController> Units
    {
        get { return PlayerInfo.Units; }
        private set { PlayerInfo.Units = value; }
    }
    public bool IsBlockGridActive
    {
        get { return PlayerInfo.ActiveGrid.Count > 0; }
    }
    public int AvailableUnits { get { return Units.Where(u => u.Available).Count(); } }
    public int TotalUnits { get { return Units.Where(u => u).Count(); } }

    private static string _debugFilePath;

    protected private int _gridSizeX;
    protected private int _gridSizeY;

    public abstract IEnumerable<MovePoint> CreatePath(GridBlock startPos, GridBlock endPos);

    public virtual void InitializeUnits()
    {
        Units = new List<UnitController>(UnitHolder.GetComponentsInChildren<UnitController>());
    }

    public void KillUnit(InputAction.CallbackContext context)
    {
        if (context.performed && Units.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, TotalUnits);
            UnitController rUnit = Units[random];
            Damageable unitDmg = rUnit.GetComponent<Damageable>();
            unitDmg.Kill();
        }
    }

    public void ResetBlockGrid()
    {
        PlayerInfo.ActiveGrid.ToList().ForEach(aG => aG.Disable());
        PlayerInfo.ActiveGrid.Clear();
        PlayerInfo.MovementPath.Clear();
        PlayerInfo.BlockGrid = new GridBlock[_gridSizeX, _gridSizeY];
    }

    public virtual void AddUnit(UnitController unit, bool addAtRandom = false)
    {
        if (Units.Contains(unit))
            return;

        if (addAtRandom)
        {
            var r = new System.Random();
            int index = r.Next(Units.Count());
            Units.Insert(index, unit);
        }
        else
            Units.Add(unit);
    }

    public virtual void RemoveUnit(UnitController unit)
    {
        if (unit && Units.Contains(unit))
            Units.Remove(unit);

        if(Units.Count() <= 0 && SceneManager.GameState != Enums.GameState.Results)
        {
            SceneManager.FinishScene(this);
        }
    }

    protected virtual void Awake()
    {
        PlayerInfo = new PlayerInfo();
        _debugFilePath = Application.persistentDataPath + "/Debug.txt";
    }
}

