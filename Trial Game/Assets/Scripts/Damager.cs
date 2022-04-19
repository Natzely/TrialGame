using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public Enums.Player Player;
    public UnitController Unit;
    public int Damage = 1;

    public StatusEffect_Handler StatusHandler { get; set; }

    private SpriteRenderer _sR;
    private Projectile_Straight _projectile;

    private void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
        _projectile = GetComponent<Projectile_Straight>();
    }

    private void Start()
    {
        if(_projectile && _projectile.MaxDistance == 1)
            _sR.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject tmpObj = collision.gameObject;
        Damageable damageable = tmpObj.GetComponent<Damageable>();
        UnitController uC = tmpObj.GetComponent<UnitController>();
        
        if(uC != null && damageable != null && uC.Player != Player)// && !uC.AlliedWith.Contains(Player))
        {
            Damage = Mathf.FloorToInt(Damage * CalculateBonusDamage(uC));
            if (damageable.Damage(this))
                StatusHandler.ClearStatus(Enums.UnitStatusEffect.Rage);
            Destroy(gameObject);
        }
    }

    private float CalculateBonusDamage(UnitController uC)
    {
        float bonusDamageMult = 1; 
        if (Unit.Type == Enums.UnitType.Horse)
        {
            if (Unit.Moving)
            {
                bonusDamageMult = 2;
                if (uC.LookDirVector.x == Unit.LookDirVector.x && uC.LookDirVector.y == Unit.LookDirVector.y)
                    bonusDamageMult = 2.5f;
            }
        }
        else if (Unit.Type == Enums.UnitType.Melee && uC.Type == Enums.UnitType.Horse)
        {
            if (-uC.LookDirVector.x != Unit.LookDirVector.x || -uC.LookDirVector.y != Unit.LookDirVector.y)
            {
                bonusDamageMult = 1.5f;
                if (uC.MeleeAttackedCount == 2)
                    bonusDamageMult = 2f;
                if (uC.Moving)
                    bonusDamageMult = 2.5f;
            }
        }
        else if (Unit.Type == Enums.UnitType.Range && uC.Moving)
        {
            if (uC.Type == Enums.UnitType.Melee)
                bonusDamageMult = 1.5f;
            else if (uC.Type == Enums.UnitType.Horse)
                bonusDamageMult = 2.0f;
        }

        return bonusDamageMult;
    }
}
