using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors
{
    public static Color Player1       = new Color(.6f, 1f, .6f);
    public static Color Player2       = new Color(.6f, .6f, 1f);
    public static Color AttackSpace   = new Color(.8f, 0f, 0f);
    public static Color32 Health_Full = new Color32(0, 255, 0, 255);
    public static Color32 Health_Half = new Color32(255, 200, 0, 255);
    public static Color32 Health_Low  = new Color32(200, 0, 25, 255);

    // Minimap Colors 
    public static Color Player_Idle     = new Color(0,.8f,0);
    public static Color Player_Moving   = new Color(0, 1, 0);
    public static Color Player_Cooldown = new Color(0, .5f, 0);
    public static Color Enemy_Idle      = new Color(.8f, 0, 0);
    public static Color Enemy_Moving    = new Color(1, 0, 0);
    public static Color Enemy_Cooldown  = new Color(.5f, 0, 0);

    //Unit Info Colors
    public static Color UnitInfo_Blank    = new Color(0, 0, 0, .4f);
    public static Color UnitInfo_Friendly = new Color(0, .5f, 0, .4f);
    public static Color UnitInfo_Enemy    = new Color(.5f, 0, 0, .4f);

    //UI Colors
    public static Color Button_Selected   = new Color(0, 0, 0, .5f);
    public static Color Button_Deselected = new Color(0, 0, 0, 0);

    // Path Colors
    private static List<Color> PathColors = new List<Color>()
    {
        new Color(0, 1, 0, .5f),
        new Color(0, 0, 1, .5f),
        new Color(1, 0, 0, .5f),
    };
    public static int pathColorCount = 0;

    public static Color GetPathColor()
    {
        if (pathColorCount >= PathColors.Count)
            pathColorCount = 0;

        var color = PathColors[pathColorCount];
        pathColorCount++;
        return color;
    }
}
