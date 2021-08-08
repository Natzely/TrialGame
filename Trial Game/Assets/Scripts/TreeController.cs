using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Projectile"))
        {
            var projScript = collision.gameObject.GetComponent<Projectile>();
            if(projScript)
            {
                projScript.Destroy();
            }
        }
    }
}
