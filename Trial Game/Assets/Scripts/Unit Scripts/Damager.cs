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
        UnitController enemyUC = tmpObj.GetComponent<UnitController>();
        
        if(enemyUC != null && damageable != null && enemyUC.Player != Player)// && !uC.AlliedWith.Contains(Player))
        {
            Damage = Mathf.FloorToInt(Damage * CalculateBonusDamage(enemyUC));
            if (damageable.Damage(this))
                StatusHandler.ClearStatus(Enums.UnitStatusEffect.Rage);
            Destroy(gameObject);
        }
    }

    private float CalculateBonusDamage(UnitController enemyUC)
    {
        float bonusDamageMult = 1; 
        if (Unit.Type == Enums.UnitType.Horse)
        {
            if (Unit.Moving)
            {
                bonusDamageMult = 2;
                if (enemyUC.LookDirVector.x == Unit.LookDirVector.x && enemyUC.LookDirVector.y == Unit.LookDirVector.y)
                    bonusDamageMult = 2.5f;
            }
        }
        else if (Unit.Type == Enums.UnitType.Melee && enemyUC.Type == Enums.UnitType.Horse)
        {
            if (-enemyUC.LookDirVector.x != Unit.LookDirVector.x || -enemyUC.LookDirVector.y != Unit.LookDirVector.y)
            {
                bonusDamageMult = 1.5f;
                if (enemyUC.MeleeAttackedCount == 2)
                    bonusDamageMult = 2f;
                if (enemyUC.Moving)
                    bonusDamageMult = 2.5f;
            }
        }
        else if (Unit.Type == Enums.UnitType.Range && enemyUC.Moving)
        {
            if (enemyUC.Type == Enums.UnitType.Melee)
                bonusDamageMult = 1.5f;
            else if (enemyUC.Type == Enums.UnitType.Horse)
                bonusDamageMult = 2.0f;
        }

        return bonusDamageMult;
    }
}
