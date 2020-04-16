using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPlayerSpaces
{
    Dictionary<Enums.Player, GridSpace> _spaces;

    public GridPlayerSpaces()
    {
        _spaces = new Dictionary<Enums.Player, GridSpace>();
    }

    public GridSpace this[Enums.Player player]
    {
        get
        {
            if (_spaces.ContainsKey(player))
                return _spaces[player];
            else
                return null;
        }
        set
        {
            _spaces[player] = value;
        }
    }

    public bool ContainsPlayer(Enums.Player player)
    {
        return _spaces.ContainsKey(player);
    }

    public bool GetSpaceEnabled(Enums.Player player)
    {
        if (_spaces.ContainsKey(player))
            return _spaces[player].SpaceActive;

        return false;
    }


}
