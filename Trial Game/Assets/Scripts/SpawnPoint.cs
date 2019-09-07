using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public List<SpawnPoint> OtherSpawns;
    public GameObject UnitSpawn;
    public float SpawnIntervalMin;
    public float SpawnIntervalMax;

    [HideInInspector]
    public GridBlock GridBlock;

    float _timeTillSpawn;
    List<int> _spawnsLeft;

    // Start is called before the first frame update
    void Start()
    {
        _timeTillSpawn = Random.Range(0, SpawnIntervalMin);
        _spawnsLeft = Enumerable.Range(0, OtherSpawns.Count).ToList();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_timeTillSpawn <= 0 && GridBlock != null && _spawnsLeft.Count > 0)
        {
            int rand = Random.Range(0, _spawnsLeft.Count);
            int spawnN = _spawnsLeft.GetAndRemove(rand); 

            var spawn = OtherSpawns[spawnN];
            var unit = Instantiate(UnitSpawn, transform.position, Quaternion.identity);
            UnitController uC = unit.GetComponent<UnitController>();
            uC.StartPos = GridBlock;
            uC.EndPos = spawn.GridBlock;    
            _timeTillSpawn = Random.Range(SpawnIntervalMin, SpawnIntervalMax);
        }

        _timeTillSpawn -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gridBlock = collision.gameObject.GetComponent<GridBlock>();
        if(gridBlock != null)
            GridBlock = gridBlock;
    }
}
