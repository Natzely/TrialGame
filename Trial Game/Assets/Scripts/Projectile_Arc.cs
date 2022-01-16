using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Arc : Projectile
{
    public float ArcHeight;
    public Vector2 TempDestination;

    private Vector2 _arcVertex;
    private Vector2 _destination;
    private bool _launch;
    private float _a;

    public override void Launch(Vector2 destination)
    {
        _destination = destination;
        _arcVertex = new Vector2(
            (_destination.x + _startPos.x) / 2,
            _startPos.y + ArcHeight);

        float x = transform.position.x;
        float y = transform.position.y;
        _a = (y - _arcVertex.y) / Mathf.Pow((x - _arcVertex.x), 2);
        _launch = true;

    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if(TempDestination != Vector2.zero)
        {
            Launch(TempDestination);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (_launch)
        {
            float newX = Mathf.MoveTowards(transform.position.x, _destination.x, Speed * Time.deltaTime);
            float newY = _a * Mathf.Pow((newX - _arcVertex.x), 2) + _arcVertex.y;
            transform.position = new Vector2(newX, newY);
        }
    }
}
