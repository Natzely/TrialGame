using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AttackResults : MonoBehaviour
{
    [SerializeField] private CursorController Cursor;
    [SerializeField] private GameObject PlayerArrow;
    [SerializeField] private GameObject EnemyArrow;
    [SerializeField] private TextMeshProUGUI PlayerDamage;
    [SerializeField] private TextMeshProUGUI EnemyDamage;
    [SerializeField] private float AnimationTime;

    private RectTransform _rectT;
    private Vector2 _baseSize;
    private bool _showing;

    public void Show(bool show, UnitController playerUnit = null, UnitController enemyUnit = null, GridBlock attackFromGB = null)
    {
        if (show)
        {
            int playerDmg = Mathf.FloorToInt(Mathf.Max(playerUnit.CurrentDamage / enemyUnit.Defense, 1));
            PlayerDamage.text = $"{playerDmg} DMG";
            if (enemyUnit.CheckAttack(attackFromGB, true) && playerDmg < enemyUnit.CurrentHealth)
            {
                EnemyArrow.SetActive(true);
                int enemyDmg = NewEnemyDamage(enemyUnit, playerDmg);
                int moddedDmg = Mathf.FloorToInt(Mathf.Max(enemyDmg / playerUnit.Defense, 1));
                EnemyDamage.text = $"{moddedDmg} DMG";
            }
            else
                EnemyArrow.SetActive(false);
            gameObject.SetActive(true);
            _showing = true;
        }
        else
        {
            gameObject.SetActive(false);
            _showing = false;
            _rectT.sizeDelta = new Vector2(0, _baseSize.y);
        }
    }

    private void Awake()
    {
        _rectT = GetComponent<RectTransform>();
        _baseSize = _rectT.sizeDelta;
        _rectT.sizeDelta = new Vector2(0, _baseSize.y);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(_showing)
        {
            var timePer = Time.unscaledDeltaTime / AnimationTime;
            var sizeAdd = timePer * _baseSize.x;
            var newSizeX = Mathf.Clamp(sizeAdd + _rectT.sizeDelta.x, 0, _baseSize.x);
             _rectT.sizeDelta = new Vector2(newSizeX, _baseSize.y);
            if(newSizeX == _baseSize.x)
                _showing = false;
        }
    }

    private int NewEnemyDamage(UnitController enemyUnit, int PlayerDamage)
    {
        //Mathf.Max(1, Mathf.FloorToInt(Damage * (_damagable.Health / Damageable.MAXHEALTH))); // how damage is calculated
        float newEnemyHealth = enemyUnit.CurrentHealth - PlayerDamage;
        float damageMod = newEnemyHealth / Damageable.MAXHEALTH;
        int newEnemyDamage = Mathf.Max(1, Mathf.FloorToInt(enemyUnit.Damage * damageMod));
        return newEnemyDamage;
    }
}
