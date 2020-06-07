using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public Enums.Player Player;
    public UnitController Parent;
    public int Damage = 1;

    private SpriteRenderer _sR;

    private void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if(Parent.MaxAttackDistance == 1)
        {
            _sR.enabled = false;
        }
    }

    private void Update()
    {
        if (Vector2.Distance(Parent.Position, transform.position) >= Parent.MaxAttackDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject tmpObj = collision.gameObject;
        Damageable damageable = tmpObj.GetComponent<Damageable>();
        UnitController uC = tmpObj.GetComponent<UnitController>();
        
        if(uC != null && damageable != null && uC.Player != Player)// && !uC.AlliedWith.Contains(Player))
        {
            Damage = Mathf.FloorToInt(Damage * CalculateBonusDamage(uC));
            damageable.Damage(this);
            //Parent.DamageResults(damageable.Damage(this));
            Destroy(gameObject);
        }
    }

    private float CalculateBonusDamage(UnitController uC)
    {
        float bonusDamageMult = 1; 
        if (Parent.Type == Enums.UnitType.Horse)
        {
            if (Parent.Moving)
            {
                bonusDamageMult = 2;
                if (uC.LookAtXY.x == Parent.LookAtXY.x && uC.LookAtXY.y == Parent.LookAtXY.y)
                    bonusDamageMult = 2.5f;
            }
        }
        else if (Parent.Type == Enums.UnitType.Melee && uC.Type == Enums.UnitType.Horse)
        {
            if (-uC.LookAtXY.x != Parent.LookAtXY.x || -uC.LookAtXY.y != Parent.LookAtXY.y)
            {
                bonusDamageMult = 1.5f;
                if (uC.MeleeAttackedCount == 2)
                    bonusDamageMult = 2f;
                if (uC.Moving)
                    bonusDamageMult = 2.5f;
            }
        }
        else if (Parent.Type == Enums.UnitType.Range && uC.Moving)
        {
            if (uC.Type == Enums.UnitType.Melee)
                bonusDamageMult = 1.5f;
            else if (uC.Type == Enums.UnitType.Horse)
                bonusDamageMult = 2.0f;
        }

        return bonusDamageMult;
    }
}
