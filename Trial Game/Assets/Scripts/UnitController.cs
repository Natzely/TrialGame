using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public Enums.Player Player = Enums.Player.Player1;
    public GameObject Projectile;
    public bool OnCooldown = false;
    public int TotalMoves = 4;
    public int AttackDistance = 2;
    public float Speed = 5;
    public float Cooldown = 5;
    public float AttackSpeed = 5;
    
    public bool Moved { get; internal set; }
    public bool Moving { get; internal set; }
    public bool Attacked { get; internal set; }
    public GridBlock StartPos { get; set; }
    public GridBlock EndPos { get; set; }

    Animator _animator;
    SpriteRenderer _sR;
    Queue _moveToPoints;
    Vector2? _nextPoint;
    Vector2 _lastPoint;
    Vector2 _originalPoint;
    Vector2 _attackPos;
    PlayerManager _pm;

    bool _hover;
    bool _selected;
    bool _attack;
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
            if (!Moving)
            {
                Selected();
            }
        }
    }

    public void ReadyAttack(Vector2 pos)
    {
        _attack = true;
        _attackPos = pos;
    }

    public void MoveTo(List<GridBlock> movePoints)
    {
        if (_cooldown <= 0 && !Moved)
        {
            for(int x = 0; x <= movePoints.Count - 1; x++)
                _moveToPoints.Enqueue(movePoints[x].Position);
            Moving = true;
        }
    }

    public void CancelMove()
    {
        _moveToPoints.Clear();
        _nextPoint = null;
        transform.position = _originalPoint;
        Moved = false;
        Moving = false;
        ResetLook();
    }

    public Vector3 Position
    {
        get { return transform.position; }
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _pm = FindObjectOfType<PlayerManager>();

        Moved = false;
        _hover = false;
        _selected = false;
        _nextPoint = null;
        _moveToPoints = new Queue();
        _originalPoint = transform.position;

        Player = (Enums.Player)gameObject.layer;
        switch (Player)
        {
            case Enums.Player.Player2:
                _animator.SetFloat("Look X", -1);
                _sR.color = Colors.Player2;
                break;
            default:
                _animator.SetFloat("Look X", 1);
                _sR.color = Colors.Player1;
                break;
        }
    }

    void Update()
    {
        if(!_moveToPoints.IsEmpty() || _nextPoint != null)
        {
            if (_nextPoint == null)
            {
                _nextPoint = (Vector2)_moveToPoints.Dequeue();
                _lastPoint = transform.position;

                LookAt(_nextPoint.Value);
            }
            
            Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Value, Speed * Time.deltaTime);

            transform.position = moveVector;
            if (transform.position.V2() == _nextPoint.Value)
            {
                _nextPoint = null;

                if(_moveToPoints.IsEmpty())
                {
                    Moving = false;
                    Moved = true;
                    if (_selected == false && !_attack)
                        Selected();
                }

            }
        }

        if (_moveToPoints.IsEmpty() && _nextPoint == null && _attack)
        {
            Attack();
            _attack = false;
            Selected();
        }

        if (Player != Enums.Player.Player1 && _moveToPoints.IsEmpty() && _nextPoint == null && !_animator.GetBool("Fixed"))
        {
            var path = _pm.CreatePath(Player, StartPos, EndPos).ToList();
            MoveTo(path);
            _animator.SetFloat("Speed", Speed);
        }

        if (_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
            _animator.speed = Mathf.Clamp((1 - _cooldown / Cooldown), .2f, 1) * 3 + .1f;
            if (_cooldown <= 0)
            {
                _animator.speed = 1;
                _animator.SetBool("Cooldown", OnCooldown = false);
                Moved = false;
                Attacked = false;
                Moving = false;
            }
        }
    }

    private void Selected()
    {
        if (Player == Enums.Player.Player1)
        {
            _animator.SetBool("Selected", _selected);

            if (!_selected && (Moved || Attacked))
            {
                GoOnCooldown();
            }
        }
        else
        {
            _animator.SetFloat("Speed", 0);
            _animator.SetBool("Fixed", true);
        }
    }

    private void Attack()
    {
        LookAt(_attackPos);

        Vector2 dir = new Vector2(_animator.GetFloat("Look X"), _animator.GetFloat("Look Y"));
        GameObject projObj = Instantiate(Projectile, (Vector2)transform.position + (dir * .5f), Quaternion.identity);
        projObj.layer = gameObject.layer;

        Projectile tmpProjectile = projObj.GetComponent<Projectile>();
        var tmpDir = _attackPos - transform.position.V2();
        tmpDir.Normalize();
        tmpProjectile.Launch(tmpDir, AttackSpeed, AttackDistance);

        Attacked = true;
        _animator.SetTrigger("Launch");
        StartCoroutine(Deselect());
    }

    private void GoOnCooldown()
    {
        _originalPoint = transform.position;
        ResetLook();
        _animator.SetBool("Selected", false);
        Hover(false);
        _cooldown = Cooldown;
        _animator.SetBool("Cooldown", OnCooldown = true);
    }

    private void ResetLook()
    {
        _animator.SetFloat("Look X", Player == Enums.Player.Player1 ? 1 : -1);
        _animator.SetFloat("Look Y", 0);
    }

    private void LookAt(Vector2 lookAt)
    {
        float x = lookAt.x - transform.position.x;
        float y = lookAt.y - transform.position.y;
        _animator.SetFloat("Look X", x == 0 ? 0 : x > 0 ? 1 : -1);
        _animator.SetFloat("Look Y", y == 0 ? 0 : y > 0 ? 1 : -1);
    }

    IEnumerator Deselect()
    {
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"));
        yield return new WaitUntil(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"));
        Select(false);
    }
}

