using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public List<PlayerInfo> PlayerList;

    [Serializable]
    public class PlayerInfo
    {
        public Enums.Player Player;
        public bool DeleteMoveSpace;
    }
}
