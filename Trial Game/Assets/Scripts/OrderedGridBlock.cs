using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class OrderedGridBlock
{
    public GridBlock GridBlock;
    public double OrderedValue;

    public OrderedGridBlock(GridBlock gridBlock, double ordereValue)
    {
        GridBlock = gridBlock;
        OrderedValue = ordereValue;
    }
}
