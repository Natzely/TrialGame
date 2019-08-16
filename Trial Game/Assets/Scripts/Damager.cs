using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public int Damage = 1;

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
