using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public float Health = 2;

    Animator _animator;

    public void Damage(float damage)
    {
        if (damage > 0)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Destroy(gameObject);
            }
            else
                _animator.SetTrigger("Hit");
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
    }
}
