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
    [SerializeField] [Range(0, 1)] private float GroundSpeedMultiplyer;
    [SerializeField] [Range(0, 1)] private float PathwaySpeedMultiplyer;
    [SerializeField] [Range(0, 1)] private float WallSpeedMultiplyer;

    [SerializeField] private int GrassMoveCost;
    [SerializeField] private int RoadMoveCost;
    [SerializeField] private int TreeMoveCost;
    [SerializeField] private int WaterMoveCost;
    [SerializeField] private int GroundMoveCost;
    [SerializeField] private int PathwayMoveCost;
    [SerializeField] private int WallMoveCost;

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
            { Enums.GridBlockType.Ground, GroundSpeedMultiplyer },
            { Enums.GridBlockType.Pathway, PathwaySpeedMultiplyer },
            { Enums.GridBlockType.Wall, WallSpeedMultiplyer },
        };

        _unitMoveCosts = new Dictionary<Enums.GridBlockType, int>()
        {
            { Enums.GridBlockType.Grass, GrassMoveCost },
            { Enums.GridBlockType.Stone, RoadMoveCost },
            { Enums.GridBlockType.Water, WaterMoveCost },
            { Enums.GridBlockType.Tree, TreeMoveCost },
            { Enums.GridBlockType.Ground, GroundMoveCost },
            { Enums.GridBlockType.Pathway, PathwayMoveCost },
            { Enums.GridBlockType.Wall, WallMoveCost },
        };
    }

    public float GetGridblockMultiplyer(Enums.GridBlockType gridType)
    {
        if (!_unitMultiplyers.TryGetValue(gridType, out float speedCost))
            speedCost = 1;
        return speedCost;
    }

    public int GetGridblockMoveCost(Enums.GridBlockType gridType)
    {
        _unitMoveCosts.TryGetValue(gridType, out int moveCost);
        return moveCost;
    }
}
