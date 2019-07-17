using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum Player
    {
        Player1 = 11,
        Player2 = 12,
    }

    public enum CursorState
    {
        Default,
        Moving,
        Attacking,
    }

    public enum PathState
    {
        Head,
        Straight,
        Curve,
        Start,
    }

    public enum PathDirection
    {
        Start = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
        End = 5,
    }

    public enum TileType
    {
        Grass,
        Rock,
        Water,
    }
}
