using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Utility
{
    public static double RoundAwayFromZero(double f)
    {
        if (f > 0.0)
            return Math.Ceiling(f);
        else if (f < 0.0)
            return Math.Floor(f);
        return f;
    }

    public static Vector2 UITilePosition(RectTransform rT, Transform t)
    {
        return new Vector2(rT.rect.width * (t.position.x) * rT.localScale.x, rT.rect.height * (t.position.y) * rT.localScale.y);
    }

    public static GameObject FindObjectWithName(string name)
    {
        var objects = Resources.FindObjectsOfTypeAll(typeof(GameObject)).ToList();
        objects = objects.Where(o => o.name == name).ToList();
        return objects.FirstOrDefault() as GameObject;
    }
}
