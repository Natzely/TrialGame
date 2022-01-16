using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float Speed = 4;

    protected SpriteRenderer _sR;
    protected Vector2 _startPos;
    protected bool _destroy;

    public abstract void Launch(Vector2 destination);

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
        _startPos = transform.position;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_destroy)
            Destroy(gameObject);
    }

    public void Destroy()
    {
        _destroy = true;
    }
}
