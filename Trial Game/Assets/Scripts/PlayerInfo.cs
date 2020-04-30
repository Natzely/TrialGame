using System.Collections.Generic;

public class PlayerInfo
{
    public GridBlock[,] BlockGrid;
    public HashSet<GridBlock> ActiveGrid;
    public List<GridBlock> MovementPath;
    public List<UnitController> Units;
    public bool DeleteMoveSpace;
    public bool HideGrid;

    public PlayerInfo()
    {
        ActiveGrid = new HashSet<GridBlock>();
        MovementPath = new List<GridBlock>();
        Units = new List<UnitController>();
    }
}
