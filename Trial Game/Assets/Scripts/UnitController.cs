﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public Enums.Player Player;
    public Transform Holder;
    public GameObject Projectile;
    public bool OnCooldown = false;
    public int TotalMoves = 4;
    public float Speed = 5;
    public float Cooldown = 5;
    public float AttackDistance = 5;
    public float AttackSpeed = 5;

    private bool _moved;
    public bool Moved
    {
        get { return _moved; }
    }

    Animator _animator;
    SpriteRenderer _sR;
    Queue _moveToPoints;
    Vector3? _nextPoint;
    Vector3 _lastPoint;
    Vector2 _originalPoint;

    bool _hover;
    bool _selected;
    bool _attacked;
    float _cooldown;

    public void Hover(bool hover)
    {
        _hover = hover;
        _animator.SetBool("Hover", hover);
    }

    public void Select(bool select)
    {
        if (_cooldown <= 0)
        {
            _selected = select;
            _animator.SetBool("Selected", select);
            if (!_selected && (_moved || _attacked))
            {
                GoOnCooldown();
            }
        }
    }

    public void Attack(Vector2 pos)
    {
        GameObject projObj = Instantiate(Projectile, (Vector2)Holder.position + (Vector2.up * .5f) + (Vector2.right *.5f), Quaternion.identity);
        projObj.layer = gameObject.layer;

        Projectile tmpProjectile = projObj.GetComponent<Projectile>();
        var tmpDir = pos - Holder.position.V2();
        tmpDir.Normalize();
        tmpProjectile.Launch(tmpDir, AttackSpeed, AttackDistance);

        _attacked = true;
        _animator.SetTrigger("Launch");
        Select(false);
    }

    public void MoveTo(List<Vector2> movePoints)
    {
        if (_cooldown <= 0 && !_moved)
        {
            while (movePoints.Count > 0)
                _moveToPoints.Enqueue(movePoints.Dequeue());
        }
    }

    public void CancelMove()
    {
        _moveToPoints.Clear();
        _nextPoint = null;
        Holder.position = _originalPoint;
        _moved = false;
    }

    public Transform GetHolder()
    {
        return Holder;
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();

        _hover = false;
        _selected = false;
        _moved = false;
        _nextPoint = null;
        _moveToPoints = new Queue();
        _originalPoint = Holder.position;

        Player = (Enums.Player)gameObject.layer;
        switch (Player)
        {
            case Enums.Player.Player2:
                _animator.SetFloat("Look X", -1);
                _sR.color = new Color(.6f, .6f, 1f);
                break;
            default:
                _animator.SetFloat("Look X", 1);
                _sR.color = new Color(.6f, 1f, .6f);
                break;
        }
    }

    void Update()
    {
        if(_moveToPoints.Count > 0 || _nextPoint != null)
        {
            if (_nextPoint == null)
            {
                Debug.Log("Grabbing next point");
                _nextPoint = (Vector2)_moveToPoints.Dequeue();
                _lastPoint = Holder.position;

                float x = _nextPoint.Value.x - _lastPoint.x;
                float y = _nextPoint.Value.y - _lastPoint.y;
                _animator.SetFloat("Look X", x == 0 ? 0 : x > 0 ? 1 : -1);
                _animator.SetFloat("Look Y", y == 0 ? 0 : y > 0 ? 1 : -1);
            }
            
            Vector2 moveVector = Vector2.MoveTowards(Holder.position, _nextPoint.Value, Speed * Time.deltaTime);

            Holder.position = moveVector;
            if (Holder.position == _nextPoint.Value)
            {
                _nextPoint = null;

                if(_moveToPoints.IsEmpty())
                {
                    _animator.SetFloat("Look X", Player == Enums.Player.Player1 ? 1 : -1);
                    _animator.SetFloat("Look Y", 0);
                    _moved = true;
                }

            }
        }

        if(_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
            _animator.speed = Mathf.Clamp((1 - _cooldown / Cooldown), .2f, 1) * 3 + .1f;
            if (_cooldown <= 0)
            {
                _animator.speed = 1;
                _animator.SetBool("Cooldown", OnCooldown = false);
                _moved = false;
            } 
        }
    }

    private void GoOnCooldown()
    {
        _originalPoint = Holder.position;
        _animator.SetBool("Selected", false);
        _cooldown = Cooldown;
        _animator.SetBool("Cooldown", OnCooldown = true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
    }
}

