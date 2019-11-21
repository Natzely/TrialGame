using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public delegate void OnDeath();
    public OnDeath OnUnitDeath;

    public List<Enums.Player> AlliedWith;
    public Enums.Player Player = Enums.Player.Player1;
    public GameObject Projectile;
    public EnemyController EnemyController;
    public bool OnCooldown = false;
    public int MoveDistance = 4;
    public int AttackDistance = 2;
    public float Speed = 5;
    public float Cooldown = 5;
    public float AttackSpeed = 5;

    [HideInInspector]
    public GridBlock CurrentGridBlock { get; private set; }

    [HideInInspector]
    public UnitController Target { get; set; }
    
    public bool Moved { get; internal set; }
    public bool Moving { get; internal set; }
    public bool Attacked { get; internal set; }
    public GridBlock StartPos { get; set; }
    public GridBlock EndPos { get; set; }

    Enums.UnitState _unitState;
    PlayerManager _pM;
    EnemyManager _eM;
    Animator _animator;
    SpriteRenderer _sR;
    Queue<GridBlock> _movePositions;
    Stack<GridBlock> _pastPositions;
    GridBlock _nextPoint;
    GridBlock _originalPoint;
    Vector2 _attackPos;

    bool _selected;
    bool _attack;
    bool _blocked;
    bool _hurt;
    float _cooldown;
    float _defaultLook;
    float _lookX;
    float _lookY;

    public Vector3 Position
    {
        get
        {
            try
            {
                return transform.position;
            }
            catch
            {
                return new Vector3(0, 0, 0);
            }
        }
    }

    public void Hover(bool hover)
    {
        _animator.SetBool("Hover", hover);
    }

    public void Select(bool select)
    {
        if (_cooldown <= 0)
        {
            _unitState = select ? Enums.UnitState.Selected : Enums.UnitState.Idle;
            _selected = select;
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
            for (int x = 0; x < movePoints.Count; x++)
            {
                var point = movePoints[x];
                _movePositions.Enqueue(point);
            }
            _nextPoint = _movePositions.Dequeue();
            Moving = true;
            _animator.SetBool("Selected", true);  
        }
    }

    public void CancelMove()
    {
        _movePositions.Clear();
        transform.position = _originalPoint.Position;
        Reset();
    }

    public bool CheckAttack(UnitController target = null)
    {
        if (Target == null && target == null)
            return false;

        if (target != null)
            Target = target;

        var dis = Vector2.Distance(Position, Target.Position);
        double checkedDistance = Math.Round(dis, 2);
        if (Target != null && checkedDistance <= AttackDistance)
        {
            ReadyAttack(Target.Position);
            return true;
        }

        return false;
    }

    public void EnterAttackState()
    {
        LookAt(_attackPos);
        _unitState = Enums.UnitState.Attacking;
        Attacked = true;
        Vector2 dir = _attackPos - transform.position.V2();
        GameObject projObj = Instantiate(Projectile, (Vector2)transform.position + (dir * .5f), Quaternion.identity);
        Damager damager = projObj.GetComponent<Damager>();
        damager.Player = Player;
        damager.Parent = this;

        Projectile tmpProjectile = projObj.GetComponent<Projectile>();
        var tmpDir = _attackPos - transform.position.V2();
        tmpDir.Normalize();
        tmpProjectile.Launch(tmpDir, AttackSpeed, AttackDistance);
    }

    public void ExitAttackState()
    {
        _unitState = Enums.UnitState.Idle;
    }

    public void EnterHurtState()
    {
        _unitState = Enums.UnitState.Hurt;
    }

    public void ExitHurtState()
    {
        _unitState = Enums.UnitState.Idle;
        if (!Attacked)
            CollisionClear();
    }

    void Awake()
    {
        _unitState = Enums.UnitState.Idle;
        Moved = false;
        _nextPoint = null;
        _movePositions = new Queue<GridBlock>();
        _pastPositions = new Stack<GridBlock>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
        if (Player != Enums.Player.Player1)
            _eM = FindObjectOfType<EnemyManager>();

        if (!OnCooldown)
            _eM?.AddUnit(EnemyController);


        switch (Player)
        {
            case Enums.Player.Player2:
                _defaultLook = -1;
                _sR.color = Colors.Player2;
                break;
            default:
                _defaultLook = 1; 
                _sR.color = Colors.Player1;
                break;
        }

        ResetLook();

        _pM.AddPlayerUnit(Player, this);
    }

    void Update()
    {
        if (_nextPoint != null && _unitState != Enums.UnitState.Attacking && _unitState != Enums.UnitState.Hurt)
        {
            Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Position, Speed * Time.deltaTime);

            transform.position = moveVector;
            if (transform.position.V2() == _nextPoint.Position)
            {
                _pastPositions.Push(_nextPoint);
                if (!_movePositions.IsEmpty())
                {
                    _nextPoint = _movePositions.Dequeue();
                    if (_nextPoint.CurrentUnit != null && _nextPoint.CurrentUnit.Player == Player && _movePositions.IsEmpty())
                        _nextPoint = null;
                    else
                        LookAt(_nextPoint.Position);
                }
                else
                    _nextPoint = null;

                if (_movePositions.IsEmpty() && _nextPoint == null)
                {
                    Moving = false;
                    Moved = true;
                    CheckAttack();
                }
            }
        }

        if (!_selected && (Moved || !Moving))
            _animator.SetBool("Selected", false);

        if (_attack && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"))
        {
            _attack = false;
            Attack();
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

                _eM?.AddUnit(EnemyController);
            }
        }
        else if (_unitState == Enums.UnitState.Idle && (Moved || (Attacked && _nextPoint == null)))
        {
            GoOnCooldown();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gO = collision.gameObject;
        GridBlock gB = gO.GetComponent<GridBlock>();
        UnitController uC = gO.GetComponent<UnitController>();

        if(gB != null)
        {
            CurrentGridBlock = gB;
            if (_originalPoint == null)
                _originalPoint = gB;
        }
        else if(uC != null && uC.Player != Player && !uC.AlliedWith.Contains(Player))
        {
            _blocked = true;
            var colDir = collision.gameObject.transform.position - transform.position;
            if(Mathf.Abs(colDir.x) > Mathf.Abs(colDir.y))
            {
                UnitCollision(collision.gameObject.transform.position, colDir.x, _lookX);
            }
            else
            {
                UnitCollision(collision.gameObject.transform.position, colDir.y, _lookY);
            }
        }
    }

    private void UnitCollision(Vector3 attackPos, float colDir, float lookDir)
    {
        float dirRound = Mathf.Round(colDir);
        if (dirRound == lookDir)
        {
            CollisionClear();
            ReadyAttack(attackPos);
        }
    }

    private void CollisionClear()
    {
        Target = null;
        _movePositions.Clear();
        DamageResults(false);
    }

    private void Attack()
    {
        _unitState = Enums.UnitState.Attacking;
        _animator.SetTrigger("Launch");
    }

    public void DamageResults(bool enemyDestroyed)
    {
        if (_blocked)
        {
            foreach (var pos in _pastPositions)
            {
                _movePositions.Enqueue(pos);
                if (pos.CurrentUnit == null)
                {
                    _nextPoint = _movePositions.Dequeue();
                    break;
                }
            }
        }

        _blocked = false;
    }

    private void GoOnCooldown()
    {
        Reset();
        _originalPoint = CurrentGridBlock;
        _animator.SetBool("Selected", false);
        Hover(false);
        _cooldown = Cooldown * (!Attacked ? .6f : 1);
        _animator.SetBool("Cooldown", OnCooldown = true);
    }

    private void Reset()
    {
        _nextPoint = null;
        _pastPositions.Clear();
        Moved = false;
        Moving = false;
        ResetLook();
    }

    private void ResetLook()
    {
        _animator.SetFloat("Look X", _defaultLook);
        _animator.SetFloat("Look Y", 0);
    }

    private void LookAt(Vector2 lookAt)
    {
        float x = lookAt.x - transform.position.x;
        float y = lookAt.y - transform.position.y;
        _animator.SetFloat("Look X", _lookX = (x == 0 ? _defaultLook / 2 : x > 0 ? 1 : -1));
        _animator.SetFloat("Look Y", _lookY = (y == 0 ? 0 : y > 0 ? 1 : -1));
    }

    //IEnumerator Deselect()
    //{
    //    yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"));
    //    yield return new WaitUntil(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"));
    //    Select(false);
    //}
}