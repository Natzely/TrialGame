using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Arc : Projectile
{
    public float ArcHeight;
    public Vector2 TempDestination;

    private float YDiff { get { return Mathf.Abs(_destination.y - _startPos.y); } }

    private Vector2 _arcVertex;
    private Vector2 _destination;
    private bool _launch;
    private float _a;
    private float arc_a;
    private float arc_b;
    private float arc_c;

    public override void Launch(Vector2 destination)
    {
        _destination = destination;
        _arcVertex = new Vector2(
            (_destination.x + _startPos.x) / 2,
            Mathf.Max(_startPos.y, _destination.y) + ArcHeight);

        float[,] matrix = new float[3, 4]
        {
            { Mathf.Pow(_startPos.x,2), _startPos.x, 1, _startPos.y },
            { Mathf.Pow(_destination.x,2), _destination.x, 1, _destination.y },
            { Mathf.Pow(_arcVertex.x,2), _arcVertex.x, 1, _arcVertex.y }
        };

        if(Utility.LinearEquationSolver.Solve(matrix))
        {
            arc_a = matrix[0, 3];
            arc_b = matrix[1, 3];
            arc_c = matrix[2, 3];
        }
        //float x = transform.position.x;
        //float y = transform.position.y;
        //_a = (y - _arcVertex.y) / Mathf.Pow((x - _arcVertex.x), 2);
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
            float newY = (arc_a * Mathf.Pow(newX, 2)) + (arc_b * newX) + arc_c; 
            //float newY = _a * Mathf.Pow((newX - _arcVertex.x), 2) + (_arcVertex.y);
            Vector2 newPos = new Vector2(newX, newY);

            float xDiff = newX - transform.position.x;
            float yDiff = newY - transform.position.y;
            float rotation = Mathf.Atan2(yDiff, xDiff) * Mathf.Rad2Deg;

            transform.SetPositionAndRotation(newPos, Quaternion.Euler(0, 0, rotation));
        }
    }
}
