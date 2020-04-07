using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyController> EnemyList;
    public float MoveDelay;
    public float DealyTimer;

    public void AddUnit(EnemyController unit)
    {
        if (!EnemyList.Contains(unit))
        {
            EnemyList.Add(unit);
            Debug.Log($"Enemy {unit.gameObject.name} added to the list");
        }
    }

    public void RemoveUnit(EnemyController unit)
    {
        EnemyList.Remove(unit);
        Debug.Log($"Enemy {unit.gameObject.name} removed from the  list");
    }

    void Awake()
    {
        DealyTimer = MoveDelay;
        EnemyList = new List<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(DealyTimer <= 0 && EnemyList.Count > 0)
        {
            var nextEnemy = EnemyList[0];
            if (nextEnemy != null && nextEnemy.gameObject != null)
            {
                var enemy = EnemyList.Dequeue();    // Get next enemy (enemy is removed from the queue) 
                if (enemy.NextAction())             // Check if it can do and action
                    DealyTimer = MoveDelay;         // If an action was made, delay the next move (enemy will itself back to the queue when it's cooldown is done)
                else
                    EnemyList.Add(enemy);           // No action was taken so add the enemy back to the end of the list.
            }
            else if(nextEnemy != null)
                EnemyList.RemoveAt(0);
        }

        if (EnemyList.Count > 0) // Only decrease the next move timer if there are enemies available, otherwise, 
            DealyTimer -= Time.deltaTime; // when all units are on cooldown, it will a unit instantly when one comes of cooldown instead of having a delay.
    }
}
