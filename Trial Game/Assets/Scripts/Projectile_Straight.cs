using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Straight : Projectile
{
    public float MaxDistance;

    private Rigidbody2D _rb2D;

    public override void Launch(Vector2 destination)
    {
        Vector2 direction = destination - transform.position.V2();
        direction.Normalize();
        //if (direction.y > 0)
        //    _sR.sortingOrder = 0;
        _rb2D.AddForce(Speed * direction);

        float rot = 0;
        if (direction.x != 0)
            rot = direction.x > 0 ? 0 : 180;
        else
            rot = direction.y * 90;
        transform.Rotate(Vector3.forward, rot);
    }

    protected override void Awake()
    {
        base.Awake();
        _rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Vector2.Distance(transform.position.V2(), _startPos) > MaxDistance)
            Destroy(gameObject);
    }
}
