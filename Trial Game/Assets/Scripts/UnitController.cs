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
    Queue _movePositions;
    Vector2? _nextPoint;
    Vector2? _lastPoint;
    Vector2 _originalPoint;
    Vector2 _attackPos;

    bool _hover;
    bool _selected;
    bool _attack;
    bool _attacking;
    bool _blocked;
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
        _hover = hover;
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
            for(int x = 0; x <= movePoints.Count - 1; x++)
                _movePositions.Enqueue(movePoints[x].Position);
            Moving = true;
            _animator.SetBool("Selected", true);  
        }
    }

    public void CancelMove()
    {
        _movePositions.Clear();
        _nextPoint = null;
        transform.position = _originalPoint;
        Moved = false;
        Moving = false;
        ResetLook();
    }

    public bool CheckAttack(UnitController target = null)
    {
        if (target != null)
            Target = target;

        if (Target != null && Vector2.Distance(Position, Target.Position) <= AttackDistance)
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
        _attack = false;
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
    }

    void Awake()
    {
        _unitState = Enums.UnitState.Idle;
        Moved = false;
        _hover = false;
        _nextPoint = null;
        _movePositions = new Queue();
        _originalPoint = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
        if (Player != Enums.Player.Player1)
            _eM = FindObjectOfType<EnemyManager>();

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
        if(!_movePositions.IsEmpty() || _nextPoint != null)
        {
            if (_unitState != Enums.UnitState.Attacking && _unitState != Enums.UnitState.Hurt)
            {
                if (_nextPoint == null)
                {
                    _nextPoint = (Vector2)_movePositions.Dequeue();

                    LookAt(_nextPoint.Value);
                }
                else
                {
                    Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Value, Speed * Time.deltaTime);

                    transform.position = moveVector;
                    if (transform.position.V2() == _nextPoint.Value)
                    {
                        _lastPoint = _nextPoint;
                        _nextPoint = null;

                        if (_movePositions.IsEmpty())
                        {
                            Moving = false;
                            Moved = true;
                            CheckAttack();
                        }
                    }
                }
            }
        }

        if (!_selected && (Moved || !Moving))
            _animator.SetBool("Selected", false);

        if (_attack && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"))
        {
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
        }
        else if(uC != null)
        {
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
            _movePositions.Clear();
            _blocked = true;
            Target = null;
            ReadyAttack(attackPos);
        }
    }

    private bool CheckForBlock()
    {
        var dir = _nextPoint - Position;
        RaycastHit2D hit = Physics2D.Raycast(Position, dir.Value, (dir.Value.x > 0 ? dir.Value.x : dir.Value.y) + .5f, LayerMask.GetMask("Unit"));
        if (hit.collider != null)
        {
            UnitController uC = hit.transform.gameObject.GetComponent<UnitController>();
            if (uC != null && _movePositions.Contains(uC.CurrentGridBlock.Position) && uC.Player != Player && !AlliedWith.Contains(uC.Player))
            {
                _movePositions.Clear();
                _blocked = true;
                Target = null;
                ReadyAttack(hit.transform.position);

                return true;
            }
        }
        return false;
    }

    private void Attack()
    {
        _unitState = Enums.UnitState.Attacking;
        _animator.SetTrigger("Launch");
    }

    public void DamageResults(bool enemyDestroyed)
    {
        //if(!enemyDestroyed && _blocked)
        //{
        _nextPoint = null;
        _movePositions.Enqueue(_lastPoint);
        //}

        _blocked = false;
    }

    private void GoOnCooldown()
    {
        _originalPoint = transform.position;
        ResetLook();
        _animator.SetBool("Selected", false);
        Hover(false);
        _cooldown = Cooldown * (!Attacked ? .6f : 1);
        _animator.SetBool("Cooldown", OnCooldown = true);
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