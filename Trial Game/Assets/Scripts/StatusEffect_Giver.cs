using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class StatusEffect_Giver : MonoBehaviour
{
    public Enums.Player AffectedPlayers;
    public Enums.UnitStatusEffect UnitStatusEffects;
    public Enums.GridStatusEffect GridStatusEffects;
}
