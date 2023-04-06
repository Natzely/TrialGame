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
        CursorMenu = 4,
    }

    public enum GridBlockType
    {
        Stone = 0,
        Grass = 1,
        Water = 2,
        Tree = 3,
        Ground = 4,
        Wall = 5,
        Bridge = 6,
        Pathway = 7,
    }

    //public enum NeighborDirection
    //{
    //    Up = 0,
    //    Down = 1,
    //    Left = 2,
    //    Right = 3,
    //    Error = -1,
    //}

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

    [System.Serializable, System.Flags]
    public enum Player
    {
        None = 0,
        Player1 = 1,
        Player2 = 2,
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
        Blocking = 7,
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
        Units = 5,
    }

    public enum UI_TitleButtonType
    {
        Load = 0,
        Quit = 1,
        Level_Done = 2,
        Level_Development = 3,
        Start = 4,
        Language = 5,
    }

    public enum UI_ConfirmButtonType
    {
        Confirm = 0,
        Cancel = 1,
    }

    public enum PauseState
    {
        Main = 0,
        Controls = 1,
        Restart = 2,
        Quit = 3,
        UnitInfo = 4,
    }

    public enum TitleState
    {
        Main = 0,
        Levels = 1,
    }

    [System.Serializable, System.Flags]
    public enum CursorMenuState
    {
        None = 0,
        Move = 1,
        Attack = 2,
        Hide = 4,
        Reveal = 8,
    }

    public enum PlayerSides
    {
        Aztec = 0,
        Spanish = 1,
    }

    public enum UI_SideSelectionButtonType
    {
        Ready = 0,
        Cancel = 1,
    }

    [System.Serializable, System.Flags]
    public enum UnitStatusEffect
    {
        None = 0,
        Rage = 2,
    }

    [System.Serializable, System.Flags]
    public enum GridStatusEffect
    {
        None = 0,
        Rage = 2,
    }

    public enum Scenerio
    {
        Rage = 1,
        Brawl = 2,
    }

    public enum GameState
    {
        SideSelection = 0,
        Play = 1,
        Pause = 2,
        Results = 3,
        TimeStop = 4,
    }

    public enum Language
    {
        English = 0,
        Spanish = 1,
    }

    public enum HTPButton
    {
        Moving         = 0,
        Selecting      = 1,
        Cancel         = 2,
        NextUnit       = 3,
        MoveAttack     = 4,
        AttackDirectly = 5,
        UnitCooldown   = 6,
        Menu           = 7,
        Minimap        = 8,
        DamageResults  = 9,
        UnitInfo       = 10,
        TimeStop       = 11,
        UnitGlance     = 12,
    }

    public enum UnitInfo
    {
        Warrior = 0,
        Spear = 1,
        Captor = 2,
        Cavalry = 3,
        Musket = 4,
        Soldier = 5,
        None = 6,
    }

    public enum UnitStat
    {
        Health = 0,
        Move = 1,
        Speed = 2,
        Attack = 3,
        Defense = 4,
        Range = 5,
        Cooldwon = 6,
    }
}

