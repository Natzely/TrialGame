using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitInfoButton : InfoButton, IMoveHandler, ICancelHandler
{
    public Enums.UnitInfo Type;
    public UnitInfoAH Parent;
    public RuntimeAnimatorController Animator;
    public Vector4 ImageSize;
    public bool Flip;
    public string HP = "10";
    public string Movement;
    public string Speed;
    public string Attack;
    public string Defense;
    public string Range = "1";
    public string Cooldown;

    public Dictionary<Enums.UnitStat, string> Stats { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        Stats = new Dictionary<Enums.UnitStat, string>()
        {
            { Enums.UnitStat.Health, HP },
            { Enums.UnitStat.Speed, Speed },
            { Enums.UnitStat.Move, Movement },
            { Enums.UnitStat.Attack, Attack },
            { Enums.UnitStat.Defense, Defense },
            { Enums.UnitStat.Range, Range },
            { Enums.UnitStat.Cooldwon, Cooldown },
        };
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (!SilentSelect)
            Parent.PlaySelectSound();
        else
            SilentSelect = false;
    }

    public void OnMove(AxisEventData eventData)
    {
        if (eventData.moveDir == MoveDirection.Right)
            Parent.CurrentUnitStat.Select();
    }

    public void OnCancel(BaseEventData eventData)
    {
        Parent.OnCancel(eventData);
    }
}

