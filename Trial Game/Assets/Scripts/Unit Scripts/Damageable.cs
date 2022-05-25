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
    public GameObject DeathObject;
    public float Health = 2;

    [SerializeField] 

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
            if (damager.Unit.Type == Enums.UnitType.Melee && _uC.Type == Enums.UnitType.Horse)
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

            _animator.SetTrigger("Hit");
            _uC.AttackedFrom = damager.Unit.Position;
            HealthText.havePropertiesChanged = true;
            Debug.Log($"{gameObject.name}: Hurt");

            if (Health <= 0)
            {
                _uC.OnUnitInterupt?.Invoke();
                return true;
            }
            else
            {
                DamageText.gameObject.SetActive(false);
                DamageText.Text = calcDamage + "";
                DamageText.gameObject.SetActive(true);
                if (!_uC.TookAction && _uC.MinAttackDistance == damager.Unit.MinAttackDistance)
                    _uC.AttackBackTarget = damager.Unit.CurrentGridBlock.ToMovePoint();
            }
        }

        return false;
    }

    public void Kill()
    {
        Health = 0;
        _animator.SetTrigger("Hit");
    }

    public void DestroyUnit()
    {
        var deathO = Instantiate(DeathObject, transform.position, Quaternion.identity);
        deathO.transform.SetParent(_uC.UnitManager.DeadUnitHolder.transform);
        SpriteRenderer oSR = deathO.GetComponent<SpriteRenderer>();
        oSR.sprite = _uC.SpriteRender.sprite;
        oSR.flipX = _uC.SpriteRender.flipX;
        Destroy(gameObject);
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
