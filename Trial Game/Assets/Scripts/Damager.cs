﻿using System.Collections;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject tmpObj = collision.gameObject;
        Damageable damageable = tmpObj.GetComponent<Damageable>();
        UnitController uC = tmpObj.GetComponent<UnitController>();
        
        if(uC != null && damageable != null && uC.Player != Player)// && !uC.AlliedWith.Contains(Player))
        {
            Parent.DamageResults(damageable.Damage(this));
            Destroy(gameObject);
        }
    }
}
