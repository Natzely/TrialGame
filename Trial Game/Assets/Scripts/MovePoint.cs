using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePoint : IEquatable<MovePoint>  
{
    public Vector2 Position { get; private set; }
    public GridBlock GridBlock { get; private set; }
    public bool IsOccupied { get { return GridBlock.IsOccupied; } }
    public UnitController CurrentUnit
    {
        get
        {
            if (GridBlock) return GridBlock.CurrentUnit;
            else return null;
        }
    }

    private bool _hide;

    public MovePoint(GridBlock gridBlock, bool hide = false)
    {
        GridBlock = gridBlock;
        _hide = hide;
        Position = new Vector2(
            gridBlock.Position.x,
            gridBlock.Position.y + (hide ? .2f : 0));
    }

    public void Path_Delete(UnitController uC)
    {
        GridBlock.Path_Delete(uC);
    }

    public void Path_Save(UnitController uC, Color color)
    {
        GridBlock.Path_Save(uC, color);
    }

    public override bool Equals(object mp) => this.Equals(mp as MovePoint);

    private bool Equals(MovePoint mP)
    {
        if (mP is null)
            return false;

        if (ReferenceEquals(this, mP))
            return true;

        if (this.GetType() != mP.GetType())
            return false;

        return (Position == mP.Position);
    }

    public override int GetHashCode() => (GridBlock, _hide).GetHashCode();

    bool IEquatable<MovePoint>.Equals(MovePoint other)
    {
        return Equals(other);
    }

    public static bool operator ==(MovePoint lhs, MovePoint rhs)
    {
        if(lhs is null)
        {
            if (rhs is null)
                return true;

            // Only the left side is null
            return false;
        }
        return lhs.Equals(rhs);
    }

    public static bool operator !=(MovePoint lhs, MovePoint rhs) => !(lhs == rhs);
}
