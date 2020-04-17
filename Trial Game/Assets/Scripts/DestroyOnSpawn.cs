using UnityEngine;

public class DestroyOnSpawn : MonoBehaviour
{
    public double DestroyTimer;

    private double _destroyTime;
    void Update()
    {
        if (_destroyTime >= DestroyTimer)
            Destroy(this.gameObject);

        _destroyTime += Time.deltaTime;
    }
}
