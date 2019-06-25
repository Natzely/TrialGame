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
        Start = -1,
        Left = 0,
        Right = 1,
        Up = 3,
        Down = 4,
    }
}
