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
    [Tooltip("0-100")]
    [Range(0, 100)]
    public float BlockChance;
    [Tooltip("0")]
    [Range(0, 1)]
    public float DamageReduction;

    public bool CanBlock { get; set; }

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

            float calcDamage = Mathf.Max(damager.Damage / _uC.Defense, 1);
            float blockNum = Random.Range(1, 100);
            bool blocked = (blockNum <= BlockChance) && CanBlock;

            if (blocked)
                calcDamage = Mathf.CeilToInt(calcDamage * (1 - DamageReduction));

            calcDamage = Mathf.FloorToInt(calcDamage);

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

            if (blocked && Health > 0)
                _animator.SetTrigger("Block");
            else
                _animator.SetTrigger("Hit");

            _uC.AttackedFrom = damager.Unit.Position;
            HealthText.havePropertiesChanged = true;

            ShowDamage(calcDamage);
            if (Health <= 0)
            {
                _uC.OnUnitInterupt?.Invoke();
                TimeStopHandler.Instance.Death();
                return true;
            }
            else
            {
                TimeStopHandler.Instance.Violence();
                if ((!_uC.TookAction || _uC.InfoType == Enums.UnitInfo.Soldier) && _uC.CheckAttack(damager.Unit.CurrentGridBlock, true))// Position.GridDistance(damager.Unit.Position) <= _uC.MaxAttackDistance)
                    _uC.AttackBackTarget = damager.Unit.CurrentGridBlock.ToMovePoint();
            }
        }

        return false;
    }

    private void ShowDamage(float damage)
    {
        DamageText.gameObject.SetActive(false);
        DamageText.Text = damage + "";
        DamageText.gameObject.SetActive(true);
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
