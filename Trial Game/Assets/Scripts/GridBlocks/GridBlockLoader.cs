using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridBlock))]
public class GridBlockLoader : MonoBehaviour
{
    [SerializeField] private GridBlock GridBlock;
    [SerializeField] private float MinWaitTime;
    [SerializeField] private float MaxWaitTime;

    private float _randomWaitTime;

    private void Awake()
    {
        _randomWaitTime = Random.Range(MinWaitTime, MaxWaitTime);
    }

    // Update is called once per frame
    void Update()
    {
        _randomWaitTime -= Time.deltaTime;
        if (_randomWaitTime <= 0)
        {
            GridBlock.Initialize();
            Destroy(this);
        }
    }
}
