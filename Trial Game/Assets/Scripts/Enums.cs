using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum Player
    {
        Player1 = 11,
        Player2 = 12,
        Player3 = 13,
        Player4 = 14,
    }

    public enum CursorState
    {
        Default,
        Moving,
        Attacking,
    }

    public enum PathState
    {
        Start = 0,
        Head = 1,
        Straight = 2,
        Curve = 3,
    }

    public enum PathDirection
    {
        Start = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
    }

    public enum TileType
    {
        Grass,
        Rock,
        Water,
    }
}
