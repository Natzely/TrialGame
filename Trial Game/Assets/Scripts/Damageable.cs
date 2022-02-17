using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public const int MAXHEALTH = 10;

    public GameObject DeathSoundObject;
    public Damage_TextEffect DamageText;
    public TextMeshProUGUI HealthText;
    public float Health = 2;

    private Animator _animator;
    private UnitController _uC;

    /// <summary>
    /// Handles damage given to a unit. Returns false if the unit still have health left and 
    /// true when the unit is destroyed.
    /// </summary>
    /// <param name="damager"></param>
    /// <returns></returns>
    public bool Damage(Damager damager)
    {
        if (damager.Damage > 0)
        {
            if (damager.Parent.Type == Enums.UnitType.Melee && _uC.Type == Enums.UnitType.Horse)
                _uC.IncreaseMeeleAttackCount();

            int calcDamage = Mathf.FloorToInt(Mathf.Max(damager.Damage / _uC.Defense, 1));
            Health -= calcDamage;
            if (Health > 0)
            {
                HealthText.gameObject.SetActive(true);
                HealthText.text = Health.ToString();
            }
            else
            {
                HealthText.enabled = false;
            }
            //if (Health / _maxHealth < .34)
            //    HealthText.color = Colors.Health_Low;
            //else if (Health / _maxHealth < .67)
            //    HealthText.color = Colors.Health_Half;
            HealthText.havePropertiesChanged = true;

            if (Health <= 0)
            {
                Vector2 posDif = (transform.position - damager.transform.position) * -1; // Flip it because if its coming from top, the we would want to look up instead of down.
                posDif.Normalize();
                _animator.SetFloat("Look X", posDif.x);
                _animator.SetFloat("Look Y", posDif.y);
                _animator.SetTrigger("Hit");
                _uC.OnUnitInterupt?.Invoke();
                return true;
            }
            else
            {
                _animator.SetTrigger("Hit");
                DamageText.gameObject.SetActive(false);
                DamageText.Text = calcDamage + "";
                DamageText.gameObject.SetActive(true);
                if (!_uC.TookAction && _uC.MinAttackDistance == damager.Parent.MinAttackDistance)
                    _uC.Target = damager.Parent.CurrentGridBlock.ToMovePoint();
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
        if (_uC.Player != Enums.Player.Player1)
            HealthText.alignment = TextAlignmentOptions.TopRight;
        HealthText.havePropertiesChanged = true;
    }
}
