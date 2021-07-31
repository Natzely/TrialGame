using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlockCosts : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private float GrassSpeedMultiplyer;
    [SerializeField] [Range(0, 2)] private float RoadSpeedMultiplyer;
    [SerializeField] [Range(0, 1)] private float TreeSpeedMultiplyer;
    [SerializeField] [Range(0, 1)] private float WaterSpeedMultiplyer;

    [SerializeField] private int GrassMoveCost;
    [SerializeField] private int RoadMoveCost;
    [SerializeField] private int TreeMoveCost;
    [SerializeField] private int WaterMoveCost;

    private Dictionary<Enums.GridBlockType, float> _unitMultiplyers;
    private Dictionary<Enums.GridBlockType, int> _unitMoveCosts;

    public void Start()
    {
        _unitMultiplyers = new Dictionary<Enums.GridBlockType, float>()
        {
            { Enums.GridBlockType.Grass, GrassSpeedMultiplyer },
            { Enums.GridBlockType.Stone, RoadSpeedMultiplyer },
            { Enums.GridBlockType.Water, WaterSpeedMultiplyer },
            { Enums.GridBlockType.Tree, TreeSpeedMultiplyer },
        };

        _unitMoveCosts = new Dictionary<Enums.GridBlockType, int>()
        {
            { Enums.GridBlockType.Grass, GrassMoveCost },
            { Enums.GridBlockType.Stone, RoadMoveCost },
            { Enums.GridBlockType.Water, WaterMoveCost },
            { Enums.GridBlockType.Tree, TreeMoveCost },
        };
    }

    public float GetGridblockMultiplyer(Enums.GridBlockType gridType)
    {
        _unitMultiplyers.TryGetValue(gridType, out float speedCost);
        return speedCost;
    }

    public int GetGridblockMoveCost(Enums.GridBlockType gridType)
    {
        _unitMoveCosts.TryGetValue(gridType, out int moveCost);
        return moveCost;
    }
}
