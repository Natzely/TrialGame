using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public int Damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject tmpObj = collision.gameObject;
        Damageable damageable = tmpObj.GetComponent<Damageable>();
        
        if(tmpObj.tag != "Cursor" && tmpObj.layer != gameObject.layer && damageable != null)
        {
            damageable.Damage(Damage);
            Destroy(gameObject);
        }
    }
}
