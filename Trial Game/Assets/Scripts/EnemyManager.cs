using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : UnitManager
{
    public float MoveDelay;
    public float DealyTimer;

    public override IEnumerable<GridBlock> CreatePath(GridBlock startPos, GridBlock endPos)
    {
        var pathList = PathFinder.CreatePath(Player, startPos, endPos, FullGrid);

        return pathList;
    }

    protected override void Awake()
    {
        base.Awake();
        DealyTimer = MoveDelay;

        var nonNullUnits = StartingUnits.Where(uC => uC != null).ToList();
        foreach(UnitController uC in nonNullUnits)
        {
            uC.Player = Player;
            uC.UnitManager = this;
            var eC = uC?.gameObject.AddComponent<EnemyController>();
            eC.UnitManager = this;
            eC.UnitController = uC;
        }
    }

    void Update()
    {
        if(DealyTimer <= 0 && PlayerInfo.Units.Count > 0)
        {
            var nextEnemy = PlayerInfo.Units[0];
            if (nextEnemy != null)
            {
                var enemy = PlayerInfo.Units.Dequeue();    // Get next enemy (enemy is removed from the queue)
                var enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController.NextAction())             // Check if it can do and action
                    DealyTimer = MoveDelay;         // If an action was made, delay the next move (enemy will itself back to the queue when it's cooldown is done)
                else
                    PlayerInfo.Units.Add(enemy);           // No action was taken so add the enemy back to the end of the list.
            }
            else
                PlayerInfo.Units.RemoveAt(0);
        }

        if (PlayerInfo.Units.Count > 0) // Only decrease the next move timer if there are enemies available, otherwise, 
            DealyTimer -= Time.deltaTime; // when all units are on cooldown, it will a unit instantly when one comes of cooldown instead of having a delay.
    }
}
