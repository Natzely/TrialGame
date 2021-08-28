using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 4;

    private Rigidbody2D _rb2D;
    private SpriteRenderer _sR;
    private Vector2 _startPos;
    private float _maxDis;
    private bool _destroy;

    public void Launch(Vector2 direction, float force, float maxDistance)
    {
        if (direction.y > 0)
            _sR.sortingOrder = 0;
        _rb2D.AddForce(force * Speed * direction);
        _maxDis = maxDistance;
    }

    // Start is called before the first frame update
    void Awake()
    {
        _rb2D = GetComponent<Rigidbody2D>();
        _sR = GetComponent<SpriteRenderer>();
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(Vector2.Distance(_startPos, transform.position)) > _maxDis ||
            _destroy)
            Destroy(gameObject);
    }

    public void Destroy()
    {
        _destroy = true;
    }
}
