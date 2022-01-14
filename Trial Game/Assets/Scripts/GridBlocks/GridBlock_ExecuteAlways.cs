using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridBlock_ExecuteAlways : MonoBehaviour
{
    [SerializeField] private GameObject ObjectToSpawn;

    private GameObject _spawnedObject;

    private void Awake()
    {
        if (ObjectToSpawn != null)
        {
            Vector3 instPos = transform.position + new Vector3(0, .35f, 0);
            _spawnedObject = Instantiate(ObjectToSpawn, instPos, transform.rotation, transform);
        }
    }

    private void Update()
    {
        _spawnedObject.transform.position = transform.position + new Vector3(0, .35f, 0);
    }

    private void OnDestroy()
    {
        if(_spawnedObject != null)
        DestroyImmediate(_spawnedObject);
    }
}
