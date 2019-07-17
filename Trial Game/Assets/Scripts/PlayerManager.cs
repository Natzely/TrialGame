using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;

    public bool GetDeleteMoveSpace(Enums.Player player)
    {
        return PlayerList.Where(x => x.Player == player).FirstOrDefault().DeleteMoveSpace;
    }

    [Serializable]
    public class PlayerInfo
    {
        public Enums.Player Player;
        public bool DeleteMoveSpace;
    }
}
