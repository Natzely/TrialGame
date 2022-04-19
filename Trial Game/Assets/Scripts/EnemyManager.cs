using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class EnemyManager : UnitManager
{
    public PlayerManager PlayerManager;
    [Tooltip("How long to wait until the EnemyManager moves another unit")]
    public float MoveDelay;
    [Tooltip("Active timer until the next enemy move")]
    public float DealyTimer;

    public override IEnumerable<MovePoint> CreatePath(GridBlock startPos, GridBlock endPos)
    {
        try
        {
            var pathList = PathFinder.CreatePath(Player, startPos, endPos, PlayerManager.FullGrid);
            return pathList;
        }
        catch (Exception ex)
        {
            Debug.Log("Error creating path");
        }
        return null;
    }

    public override void InitializeUnits()
    {
        base.InitializeUnits();

        var nonNullUnits = Units.Where(uC => uC != null).ToList();
        foreach (UnitController uC in nonNullUnits)
        {
            if (uC)
            {
                uC.gameObject.name = $"P{((int)Player)}_" + uC.gameObject.name;
                uC.enabled = true;
                uC.Player = Player;
                uC.Speed *= _globalVariables.UnitSpeedModifier;
                uC.Cooldown *= _globalVariables.UnitCooldownModifier;
                uC.UnitManager = this;
                var eC = uC.gameObject.AddComponent<EnemyController>();
                eC.UnitManager = this;
                eC.UnitController = uC;
                uC.BoxCollider.enabled = true;
                uC.DefaultLook = -1;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        DealyTimer = MoveDelay;
    }

    protected void Start()
    {
        if (InitializeUnitsAtStart)
            InitializeUnits();
    }

    void Update()
    {
        if (DealyTimer <= 0 && PlayerInfo.Units.Count > 0)
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
