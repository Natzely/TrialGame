using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public float Health = 2;

    private PlayerManager _pM;
    private Animator _animator;
    private UnitController _uC;

    public void Damage(float damage)
    {
        if (damage > 0)
        {
            Health -= damage;
            if (Health <= 0)
            {
                _pM.RemovePlayerUnit(_uC.Player, _uC);
                Destroy(gameObject);
            }
            else
                _animator.SetTrigger("Hit");
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _pM = FindObjectOfType<PlayerManager>();
        _uC = GetComponent<UnitController>();

    }
}
