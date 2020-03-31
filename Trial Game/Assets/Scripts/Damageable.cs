using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public GameObject DeathSoundObject;
    public float Health = 2;

    private PlayerManager _pM;
    private Animator _animator;
    private UnitController _uC;

    public bool Damage(float damage)
    {
        if (damage > 0)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Instantiate(DeathSoundObject, transform.position, Quaternion.identity);
                _uC.OnUnitDeath?.Invoke();
                Destroy(gameObject);
                return true;
            }
            else
                _animator.SetTrigger("Hit");
        }

        return false;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _pM = FindObjectOfType<PlayerManager>();
        _uC = GetComponent<UnitController>();
    }
}
