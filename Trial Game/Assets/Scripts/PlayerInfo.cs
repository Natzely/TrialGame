using System.Collections.Generic;
using System.Linq;

public class PlayerInfo
{
    public GridBlock[,] BlockGrid { get; set; }
    public HashSet<GridBlock> ActiveGrid { get; set; }
    public List<MovePoint> MovementPath { get; set; }
    public List<UnitController> Units { get; set; }
    public UnitController SelectedUnit { get; set; }
    public bool DeleteMoveSpace { get; set; }
    public bool HideGrid { get; set; }

    public PlayerInfo()
    {
        ActiveGrid = new HashSet<GridBlock>();
        MovementPath = new List<MovePoint>();
        Units = new List<UnitController>();
    }

    public bool MovementPathContains(GridBlock gb)
    {
        return MovementPath.Any(mp => mp.Position == gb.Position);
    }
}
