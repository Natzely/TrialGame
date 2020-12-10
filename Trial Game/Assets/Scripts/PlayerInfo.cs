using System.Collections.Generic;

public class PlayerInfo
{
    public GridBlock[,] BlockGrid { get; set; }
    public HashSet<GridBlock> ActiveGrid { get; set; }
    public List<GridBlock> MovementPath { get; set; }
    public List<UnitController> Units { get; set; }
    public UnitController SelectedUnit { get; set; }
    public bool DeleteMoveSpace { get; set; }
    public bool HideGrid { get; set; }

    public PlayerInfo()
    {
        ActiveGrid = new HashSet<GridBlock>();
        MovementPath = new List<GridBlock>();
        Units = new List<UnitController>();
    }
}
