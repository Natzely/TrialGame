using System.Collections.Generic;

public class PlayerInfo
{
    public GridBlock[,] BlockGrid;
    public List<GridBlock> ActiveGrids;
    public List<GridBlock> MovementPath;
    public List<UnitController> Units;
    public bool DeleteMoveSpace;
    public bool HideGrid;

    public PlayerInfo()
    {
        ActiveGrids = new List<GridBlock>();
        MovementPath = new List<GridBlock>();
        Units = new List<UnitController>();
    }
}
