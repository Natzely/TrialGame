using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public const int MAXHEALTH = 10;

    public GameObject DeathSoundObject;
    public TextMeshProUGUI HealthText;
    public float Health = 2;

    private PlayerManager _pM;
    private Animator _animator;
    private UnitController _uC;
    private float _maxHealth;

    public bool Damage(Damager damager)
    {
        if (damager.Damage > 0)
        {
            int calcDamage = Mathf.FloorToInt(Mathf.Max(damager.Damage / _uC.Defense, 1));
            Health -= calcDamage;
            HealthText.text = Health.ToString();
            if (Health / _maxHealth < .34)
                HealthText.color = Colors.Health_Low;
            else if (Health / _maxHealth < .67)
                HealthText.color = Colors.Health_Half;
            HealthText.havePropertiesChanged = true;

            if (Health <= 0)
            {
                Instantiate(DeathSoundObject, transform.position, Quaternion.identity);
                _uC.OnUnitInterupt?.Invoke();
                Destroy(gameObject);
                return true;
            }
            else
            {
                _animator.SetTrigger("Hit");
                if (!_uC.TookAction && _uC.MinAttackDistance == damager.Parent.MinAttackDistance)
                    _uC.Target = damager.Parent;
            }
        }

        return false;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _uC = GetComponent<UnitController>();
        HealthText.text = Health.ToString();
        HealthText.color = Colors.Health_Full;
        _maxHealth = Health;
        if (_uC.Player != Enums.Player.Player1)
            HealthText.alignment = TextAlignmentOptions.TopRight;
        HealthText.havePropertiesChanged = true;
    }
}
