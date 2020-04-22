﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum Player
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3,
    }

    public enum CursorState
    {
        Default  = 0,
        Selected = 1,
    }

    public enum UnitState
    {
        Idle       = 0,
        Selected   = 1,
        Attacking  = 2,
        Hurt       = 3,
    }

    public enum PathState
    {
        Start    = 0,
        Head     = 1,
        Straight = 2,
        Curve    = 3,
    }

    public enum PathDirection
    {
        Start = 0,
        Left  = 1,
        Right = 2,
        Up    = 3,
        Down  = 4,
    }

    public enum TileType
    {
        Grass = 0,
        Rock  = 1,
        Water = 2,
    }

    public enum ActiveSpace
    {
        Inactive = -1,
        Move     = 0,
        Attack   = 1,
    }

    public enum GridBlockType
    {
        Stone = 0,
        Grass = 1,
        Water = 2,
    }

    public enum NeighborDirection
    {
        Up    = 0,
        Down  = 1,
        Left  = 2,
        Right = 3,
        Error = -1,
    }
}
