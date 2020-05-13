using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class UnitController : MonoBehaviour, ILog
{
    public delegate void OnDeath();
    public OnDeath OnUnitInterupt;

    public AudioSource AttackAudioSource;
    public AudioSource HurtAudioSource;
    public AudioSource WalkingAudioSource;

    public Enums.Player Player = Enums.Player.Player1;
    public GameObject OffCooldownObject;
    public GameObject Projectile;
    public EnemyController EnemyController;
    public Vector2 ColliderSizeMoving;
    public Vector2 ColliderSizeIdle;
    public bool OnCooldown = false;
    public float AttackSpeed;
    public float CooldownTimer;
    public float Cooldown;
    public float Speed;
    public float Damage;
    public float Defense;
    public int MaxAttackDistance;
    public int MinAttackDistance;
    public int MoveDistance;

    public UnitManager UnitManager { get; set; }
    public UnitController Target { get; set; }
    public GridBlock CurrentGridBlock { get; private set; }
    public double DistanceFromCursor { get; private set; }
    public bool Attacked { get; private set; }
    public bool Moved { get; private set; }
    public bool Moving { get; private set; }
    public bool Blocked { get; private set; }

    public bool TookAction { get { return OnCooldown || Moving || Moved || Attacked; } }

    private Enums.UnitState _unitState;
    private CursorController _cC;
    private Animator _animator;
    private SpriteRenderer _sR;
    private BoxCollider2D _bC;
    private Queue<GridBlock> _movePositions;
    private Stack<GridBlock> _pastPositions;
    private GridBlock _nextPoint;
    private GridBlock _originalPoint;
    private Damageable _damagable;
    private Vector2 _attackPos;

    private bool _tasked;
    private bool _selected;
    private bool _attack;
    private float _defaultLook;
    private float _lookX;
    private float _lookY;

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
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _animator.SetBool("Hover", hover);
        Log("----------------------------------------");
    }

    public void Select(bool select)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (Cooldown <= 0)
        {
            _unitState = select ? Enums.UnitState.Selected : Enums.UnitState.Idle;
            _selected = select;
        }
        Log("----------------------------------------");
    }

    public void ReadyAttack(Vector2 pos)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _attack = true;
        _attackPos = pos;
        Log("----------------------------------------");
    }

    public void MoveTo(List<GridBlock> movePoints)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Log($"Number of moves: {movePoints.Count}");
        if (Cooldown <= 0 && !Moved && movePoints.Count > 0)
        {
            _tasked = true; // Task has been given

            for (int x = 0; x < movePoints.Count; x++)
            {
                var point = movePoints[x];
                _movePositions.Enqueue(point);
            }

            _nextPoint = _movePositions.Dequeue();
            LookAt(_nextPoint.Position);
            Log("Moving");
            Moving = true;
            _bC.size = ColliderSizeMoving;
            _animator.SetBool("Selected", true);
        }
        Log("----------------------------------------");
    }

    public void CancelMove()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _animator.SetBool("Selected", false);
        _movePositions.Clear();
        transform.position = _originalPoint.Position;
        _nextPoint = null;
        _pastPositions.Clear();
        Moved = false;
        Moving = false;
        _bC.size = ColliderSizeIdle;
        ResetLook();
        Log("----------------------------------------");
    }

    public bool CheckAttack(UnitController target = null)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (Target == null && target == null)
        {
            Log("No target found");
            Log("----------------------------------------");
            return false;
        }

        if (target != null)
        {
            Target = target;
            _tasked = true;
        }

        var dis = Position.GridDistance(Target.Position);
        if (Target != null && dis <= MaxAttackDistance && dis >= MinAttackDistance)
        {
            Log("Target found");
            ReadyAttack(Target.Position);
            Log("----------------------------------------");
            return true;
        }

        Log("Target not the right distance to attack");
        Log("----------------------------------------");
        return false;
    }

    public void EnterAttackState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (_selected)
            OnUnitInterupt?.Invoke();

        LookAt(_attackPos);
        _unitState = Enums.UnitState.Attacking;
        Attacked = true;
        Vector2 dir = _attackPos - transform.position.V2();
        dir.Normalize();
        GameObject projObj = Instantiate(Projectile, (Vector2)transform.position, Quaternion.identity);
        Damager damager = projObj.GetComponent<Damager>();
        damager.Player = Player;
        damager.Parent = this;
        damager.Damage = Mathf.Max(1, Mathf.FloorToInt(Damage * (_damagable.Health / Damageable.MAXHEALTH)));

        Projectile tmpProjectile = projObj.GetComponent<Projectile>();
        var tmpDir = _attackPos - transform.position.V2();
        tmpDir.Normalize();
        tmpProjectile.Launch(tmpDir, AttackSpeed, MaxAttackDistance);
        Log("----------------------------------------");
    }

    public void ExitAttackState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _unitState = Enums.UnitState.Idle;
        Log("----------------------------------------");
    }

    public void EnterHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (_selected)
            OnUnitInterupt?.Invoke();
        _unitState = Enums.UnitState.Hurt;
        Log("----------------------------------------");
    }

    public void ExitHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _unitState = Enums.UnitState.Idle;
        if (Blocked)
            CollisionClear();
        if (!TookAction && !_tasked)
            CheckAttack();
        Log("----------------------------------------");
    }

    public bool IsEnemy(Enums.Player player)
    {
        return Player != player;// && 
               //!AlliedWith.Contains(player);
    }

    public void DamageResults(bool enemyDestroyed)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (Blocked)
            FindGoodPreviousSpot();

        Blocked = false;
        Log("----------------------------------------");
    }

    public void Reset()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Select(false);
        Hover(false);
        _nextPoint = null;
        _pastPositions.Clear();
        _tasked = false;
        Moved = false;
        Moving = false;
        Attacked = false;
        Target = null;
        _bC.size = ColliderSizeIdle;
        ResetLook();
        Log("----------------------------------------");
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _bC = GetComponent<BoxCollider2D>();
        _damagable = GetComponent<Damageable>();

        _unitState = Enums.UnitState.Idle;
        _movePositions = new Queue<GridBlock>();
        _pastPositions = new Stack<GridBlock>();
        _nextPoint = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Player == Enums.Player.Player1)
        {
            _cC = FindObjectOfType<CursorController>();
            _cC.OnCursorMoveEvent += OnCursorMove;
        }

        if (!OnCooldown)
            UnitManager?.AddUnit(this, true);

        switch (Player)
        {
            case Enums.Player.Player2:
                _defaultLook = -1;
                _sR.color = Colors.Player2;
                break;
            default:
                _defaultLook = 1; 
                break;
        }

        gameObject.name = $"P{((int)Player) + 1}_" + gameObject.name;

        _bC.size = ColliderSizeIdle;
        ResetLook();

        if (OffCooldownObject != null)
            Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
    }

    void Update()
    {
        if (_nextPoint != null && _unitState != Enums.UnitState.Attacking && _unitState != Enums.UnitState.Hurt)
        {
            Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Position, Speed * Time.deltaTime);

            transform.position = moveVector;
            if (transform.position.V2() == _nextPoint.Position)
            {
                Log("Arrived at point");
                _pastPositions.Push(_nextPoint);
                if (!_movePositions.IsEmpty())
                {
                    Log("Get next point");
                    _nextPoint = _movePositions.Dequeue();
                    Log("Check if last move is empty");
                    if (_nextPoint.CurrentUnit != null && _nextPoint.CurrentUnit.Player == Player && _movePositions.IsEmpty())
                    {
                        Log("Next point is not empty");
                        if (CurrentGridBlock.CurrentUnit == this)
                        {
                            Log("Stop at current spot");
                            _nextPoint = null;
                        }
                        else
                        {
                            Log("Find a better previous spot");
                            FindGoodPreviousSpot();
                        }
                    }
                    else
                    {
                        Log("Next point is not last point or last point is empty");
                        LookAt(_nextPoint.Position);
                    }
                }
                else
                {
                    Log("Last move");
                    _nextPoint = null;
                }

                if (_movePositions.IsEmpty() && _nextPoint == null)
                {
                    Log($"Ends move");
                    Moving = false;
                    Moved = true;
                }
            }
        }

        if(Target != null && _movePositions.IsEmpty() && _nextPoint == null)
        {
            Log($"checks for attack");
            CheckAttack();
        }
         
        if (!_selected && !Moving)
            _animator.SetBool("Selected", false);

        if (_attack && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"))
        {
            Log($"attacks");
            _attack = false;
            Attack();
            if (Blocked)
                FindGoodPreviousSpot();
        }

        //if(Attacked)
        //    Log($"Cooldown Conditions - " +
        //    $"State Idle = {_unitState} | " +
        //    $"Moved = {Moved} | " +
        //    $"Attacked = {Attacked}");

        if (Cooldown > 0)
        {
            Cooldown -= Time.deltaTime;
            _animator.speed = Mathf.Clamp((1 - Cooldown / CooldownTimer), .2f, 1) * 3 + .1f;
            if (Cooldown <= 0)
            {
                Log($"Come of cooldwon");
                
                _animator.speed = 1;
                _animator.SetBool("Cooldown", OnCooldown = false);

                //if (_cC != null && _cC.CurrentUnit == null)
                //{
                //    Debug.Log("Current Unit this");
                //    _cC.CurrentUnit = this;
                //}

                if (OffCooldownObject != null)
                {
                    Log("Create cooldown object");
                    Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
                }

                if (Player != Enums.Player.Player1) // This is for non player units to make sure all units are looped through
                    UnitManager?.AddUnit(this);
            }
        }
        else if (_nextPoint == null && _unitState == Enums.UnitState.Idle && _tasked && (Moved || Attacked))// && _nextPoint == null)))
        {
            Log($"goes on cooldown");
            GoOnCooldown();
        }
        else if (Player != Enums.Player.Player1 && _nextPoint == null && _unitState == Enums.UnitState.Idle && !OnCooldown && !Moving && !Moved && !Attacked)
        {
            if (!UnitManager.PlayerInfo.Units.Contains(this))
            {
                Log($"added to unit list");
                UnitManager.AddUnit(this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        var gO = collision.gameObject;
        GridBlock gB = gO.GetComponent<GridBlock>();
        UnitController uC = gO.GetComponent<UnitController>();

        if(gB != null)
        {
            Log("Gridblock entered");
            if (CurrentGridBlock?.CurrentUnit == this)
            {
                Log("Updating last Gridblock");
                CurrentGridBlock.CurrentUnit = null;
            }

            CurrentGridBlock = gB;
            if (_originalPoint == null)
                _originalPoint = gB;
        }
        else if(uC != null && uC.Player != Player && MinAttackDistance == 1)// && !uC.AlliedWith.Contains(Player))
        {
            Log("Collided with enemy unit");
            Blocked = true;
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
        Log("----------------------------------------");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (_cC != null && collision.gameObject == _cC.gameObject && !_selected && !Moving && !Moved && !Attacked)
        {
            Hover(false);
        }
        Log("----------------------------------------");
    }

    private void OnDestroy()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        UnitManager.RemoveUnit(this);
        if (_cC != null)
            _cC.OnCursorMoveEvent -= OnCursorMove;
        Log("----------------------------------------");
    }

    private void OnCursorMove(object sender, CursorMoveEventArgs e)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        DistanceFromCursor = transform.position.GridDistance(e.Position);
        Log("----------------------------------------");
    }

    private void UnitCollision(Vector3 attackPos, float colDir, float lookDir)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        float dirRound = Mathf.Round(colDir);
        if (dirRound == lookDir)
        {
            CollisionClear();
            ReadyAttack(attackPos);
        }
        Log("----------------------------------------");
    }

    private void CollisionClear()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Target = null;
        _movePositions.Clear();
        DamageResults(false);
        Log("----------------------------------------");
    }

    private void Attack()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Target = null;
        _unitState = Enums.UnitState.Attacking;
        _animator.SetTrigger("Launch");
        Log("----------------------------------------");
    }

    private void FindGoodPreviousSpot()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        foreach (var pos in _pastPositions)
        {
            _movePositions.Enqueue(pos);
            if (pos.CurrentUnit == null || pos.CurrentUnit == this)
            {
                _nextPoint = _movePositions.Dequeue();
                break;
            }
        }
        Log("----------------------------------------");
    }

    private void GoOnCooldown()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Reset();
        //_pM?.PlayerUnitMoveDown(this);
        _originalPoint = CurrentGridBlock;
        Cooldown = CooldownTimer * (!Attacked ? 1 : 1.4f);
        _animator.SetBool("Cooldown", OnCooldown = true);
        Log("----------------------------------------");
    }

    private void ResetLook()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _animator.SetFloat("Look X", _defaultLook);
        _animator.SetFloat("Look Y", 0);
        Log("----------------------------------------");
    }

    private void LookAt(Vector2 lookAt)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        float x = lookAt.x - transform.position.x;
        float y = lookAt.y - transform.position.y;
        if (x != y)
        {
            //_animator.SetFloat("Look X", _lookX = (x == 0 ? _defaultLook / 2 : x > 0 ? 1 : -1)); // I do the "default / 2" to make sure they en 
            _animator.SetFloat("Look X", _lookX = (x == 0 ? 0 : x > 0 ? 1 : -1)); // I do the "default / 2" to make sure they en 
            _animator.SetFloat("Look Y", _lookY = (y == 0 ? 0 : y > 0 ? 1 : -1));
        }
        Log("----------------------------------------");
    }

    public void Log(string msg)
    {
        UnitManager.Log($"{gameObject.name} | {msg}");
    }
}