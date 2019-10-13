using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public float MoveDelay;

    private List<EnemyController> _enemyList;
    private float _delayTimer;

    public void AddUnit(EnemyController unit)
    {
        _enemyList.Add(unit);
    }

    // Start is called before the first frame update
    void Start()
    {
        _delayTimer = MoveDelay;
        _enemyList = new List<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_delayTimer <= 0 && _enemyList.Count > 0)
        {
            var nextEnemy = _enemyList[0];
            if (nextEnemy != null && nextEnemy.gameObject != null)
            {
                StartCoroutine(_enemyList[0].NextAction());
                _enemyList.RemoveAt(0);
                _delayTimer = MoveDelay;
            }
            else
                _enemyList.RemoveAt(0);
        }

        _delayTimer -= Time.deltaTime;
    }   
}
