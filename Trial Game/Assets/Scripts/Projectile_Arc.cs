using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Arc : Projectile
{
    public float ArcHeight;

    private Vector2 _arcVertex;
    private Vector2 _destination;
    private float _a;

    public override void Launch(Vector2 destination)
    {
        _destination = destination;
        _arcVertex = new Vector2(
            _destination.x - _startPos.x,
            _startPos.y + ArcHeight);

        float x = transform.position.x;
        float y = transform.position.y;
        _a = (y - _arcVertex.y) / Mathf.Pow((x - _arcVertex.x), 2);

    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        float newX = Mathf.MoveTowards(transform.position.x, _destination.x, Speed * Time.deltaTime);
        float newY = _a * Mathf.Pow((newX - _arcVertex.x),2) + _arcVertex.y;
        transform.position = new Vector2(newX, newY);
    }

    private void FindACo()
    {
    }
}
