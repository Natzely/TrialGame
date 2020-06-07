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
}
