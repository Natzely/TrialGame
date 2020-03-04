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

    void Awake()
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
                var enemy = _enemyList.Dequeue();   // Get next enemy (enemy is removed from the queue) 
                if (enemy.NextAction())             // Check if it can do and action
                    _delayTimer = MoveDelay;        // If an action was made, delay the next move (enemy will itself back to the queue when it's cooldown is done)
                else
                    _enemyList.Add(enemy);          // No action was taken so add the enemy back to the end of the list.
            }
            else if(nextEnemy != null)
                _enemyList.RemoveAt(0);
        }

        if (_enemyList.Count > 0) // Only decrease the next move timer if there are enemies available, otherwise, 
            _delayTimer -= Time.deltaTime; // when all units are on cooldown, it will a unit instantly when one comes of cooldown instead of having a delay.
    }   
}
