using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums : MonoBehaviour
{
    public enum ActiveSpace
    {
        Inactive = 0,
        Move = 1,
        Attack = 2,
    }

    public enum CursorState
    {
        Default = 0,
        Selected = 1,
        Attack = 2,
        Null = 3,
        OnlyAttack = 4,
    }

    public enum GridBlockType
    {
        Stone = 0,
        Grass = 1,
        Water = 2,
    }

    public enum NeighborDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        Error = -1,
    }

    public enum PathDirection
    {
        Start = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
    }

    public enum PathState
    {
        Start = 0,
        Head = 1,
        Straight = 2,
        Curve = 3,
    }

    public enum Player
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3,
    }

    public enum TileType
    {
        Grass = 0,
        Rock = 1,
        Water = 2,
    }

    public enum UnitState
    {
        Idle = 0,
        Selected = 1,
        Moving = 2,
        Attacking = 3,
        Hurt = 4,
        PlusAction = 5,
        Cooldown = 6,
    }

    public enum UnitType
    {
        Melee = 0,
        Range = 1,
        Horse = 2,
    }

    public enum UI_PauseButtonType
    {
        Continue = 0,
        Restart = 1,
        Controls = 2,
        Quit = 3,
        Controls_OK = 4,
    }

    public enum PauseState
    {
        Main = 0,
        Controls = 1,
    }
}

