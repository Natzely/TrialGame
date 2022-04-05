using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Arc : Projectile
{
    public float ArcHeightAdjustment;
    public float SizeScale;
    public Vector2 TempDestination;

    private float YDiff { get { return Mathf.Abs(_destination.y - _startPos.y); } }

    private Vector2 _arcVertex;
    private Vector2 _destination;
    private bool _launch;
    private float _a;
    private float _arc_a;
    private float _arc_b;
    private float _arc_c;
    private float _arcDis;

    public override void Launch(Vector2 destination)
    {
        float ArcHeight = ArcHeightAdjustment * transform.position.V2().GridDistance(destination);

        _destination = destination;
        _arcVertex = new Vector2(
            (_destination.x + _startPos.x) / 2,
            Mathf.Max(_startPos.y, _destination.y) + ArcHeight);

        _arcDis = Mathf.Abs(_arcVertex.x - transform.position.x);

        float[,] matrix = new float[3, 4]
        {
            { Mathf.Pow(_startPos.x,2), _startPos.x, 1, _startPos.y },
            { Mathf.Pow(_destination.x,2), _destination.x, 1, _destination.y },
            { Mathf.Pow(_arcVertex.x,2), _arcVertex.x, 1, _arcVertex.y }
        };

        if(Utility.LinearEquationSolver.Solve(matrix))
        {
            _arc_a = matrix[0, 3];
            _arc_b = matrix[1, 3];
            _arc_c = matrix[2, 3];
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
            float newY = (_arc_a * Mathf.Pow(newX, 2)) + (_arc_b * newX) + _arc_c;
            float sizeDif = (1 - Mathf.Abs(transform.position.x - _arcVertex.x) / _arcDis) * SizeScale;
            //float newY = _a * Mathf.Pow((newX - _arcVertex.x), 2) + (_arcVertex.y);
            Vector2 newPos = new Vector2(newX, newY);

            float xDiff = newX - transform.position.x;
            float yDiff = newY - transform.position.y;
            float rotation = Mathf.Atan2(yDiff, xDiff) * Mathf.Rad2Deg;

            transform.SetPositionAndRotation(newPos, Quaternion.Euler(0, 0, rotation));
            transform.localScale = new Vector3(1 + sizeDif, 1 + sizeDif, 0);
        }
    }
}
