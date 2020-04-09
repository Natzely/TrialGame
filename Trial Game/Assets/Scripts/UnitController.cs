using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public delegate void OnDeath();
    public OnDeath OnUnitDeath;

    public AudioSource WalkingAudioSource;
    public AudioSource HurtAudioSource;
    public AudioSource AttackAudioSource;

    public List<Enums.Player> AlliedWith;
    public Enums.Player Player = Enums.Player.Player1;
    public GameObject Projectile;
    public EnemyController EnemyController;
    public Vector2 ColliderSizeMoving;
    public Vector2 ColliderSizeIdle;
    public bool OnCooldown = false;
    public int MoveDistance = 4;
    public int MinAttackDistance = 0;
    public int MaxAttackDistance = 2;
    public float Speed = 5;
    public float Cooldown = 5;
    public float AttackSpeed = 5;


    [HideInInspector]
    public UnitController Target { get; set; }

    [HideInInspector]
    public GridBlock CurrentGridBlock { get; private set; }

    [HideInInspector]
    public double DistanceFromCursor { get; private set; }
    
    public bool Moved { get; internal set; }
    public bool Moving { get; internal set; }
    public bool Attacked { get; internal set; }
    public GridBlock StartPos { get; set; }
    public GridBlock EndPos { get; set; }

    Enums.UnitState _unitState;
    PlayerManager _pM;
    EnemyManager _eM;
    CursorController _cC;
    Animator _animator;
    SpriteRenderer _sR;
    BoxCollider2D _bC;
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
        if (_cooldown <= 0 && !Moved && movePoints.Count > 0)
        {
            for (int x = 0; x < movePoints.Count; x++)
            {
                var point = movePoints[x];
                _movePositions.Enqueue(point);
            }

            _nextPoint = _movePositions.Dequeue();
            Moving = true;
            _bC.size = ColliderSizeMoving;
            _animator.SetBool("Selected", true);  
        }
    }

    public void CancelMove()
    {
        _animator.SetBool("Selected", false);
        _movePositions.Clear();
        transform.position = _originalPoint.Position;
        _nextPoint = null;
        _pastPositions.Clear();
        Moved = false;
        Moving = false;
        _bC.size = ColliderSizeIdle;
        ResetLook();
    }

    public bool CheckAttack(UnitController target = null)
    {
        if (Target == null && target == null)
            return false;

        if (target != null)
            Target = target;

        var dis = Position.GridDistance(Target.Position);
        if (Target != null && dis <= MaxAttackDistance && dis >= MinAttackDistance)
        {
            ReadyAttack(Target.Position);
            return true;
        }

        return false;
    }

    public void EnterAttackState()
    {
        PlayerUnitLog($"{gameObject.name} enters attack state");
        LookAt(_attackPos);
        _unitState = Enums.UnitState.Attacking;
        Attacked = true;
        Vector2 dir = _attackPos - transform.position.V2();
        dir.Normalize();
        GameObject projObj = Instantiate(Projectile, (Vector2)transform.position/* + (dir * .5f)*/, Quaternion.identity);
        Damager damager = projObj.GetComponent<Damager>();
        damager.Player = Player;
        damager.Parent = this;

        Projectile tmpProjectile = projObj.GetComponent<Projectile>();
        var tmpDir = _attackPos - transform.position.V2();
        tmpDir.Normalize();
        tmpProjectile.Launch(tmpDir, AttackSpeed, MaxAttackDistance);
    }

    public void ExitAttackState()
    {
        PlayerUnitLog($"{gameObject.name} exits attack state");
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

    public bool IsEnemy(Enums.Player player)
    {
        return Player != player && 
               !AlliedWith.Contains(player);
    }

    public void DamageResults(bool enemyDestroyed)
    {
        if (_blocked)
            FindGoodPreviousSpot();

        _blocked = false;
    }

    public void Reset()
    {
        PlayerUnitLog($"{gameObject.name} Reset");
        Select(false);
        Hover(false);
        _nextPoint = null;
        _cC = null;
        _pastPositions.Clear();
        Moved = false;
        Moving = false;
        Attacked = false;
        Target = null;
        _bC.size = ColliderSizeIdle;
        ResetLook();
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
        _bC = GetComponent<BoxCollider2D>();
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

        _bC.size = ColliderSizeIdle;
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
                    if (_nextPoint.CurrentUnit != null && _movePositions.IsEmpty())
                    {
                        if (CurrentGridBlock.CurrentUnit == this)
                            _nextPoint = null;
                        else
                            FindGoodPreviousSpot();
                    }
                    else
                        LookAt(_nextPoint.Position);
                }
                else
                {
                    _nextPoint = null;
                }

                if (_movePositions.IsEmpty() && _nextPoint == null)
                {
                    PlayerUnitLog($": {gameObject.name} ends move");
                    Moving = false;
                    Moved = true;
                }
            }
        }

        if(Target != null && _movePositions.IsEmpty() && _nextPoint == null)
        {
            PlayerUnitLog($": {gameObject.name} checks for attack");
            CheckAttack();
        }
         
        if (!_selected && !Moving)
            _animator.SetBool("Selected", false);

        if (_attack && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch"))
        {
            PlayerUnitLog($": {gameObject.name} attacks");
            _attack = false;
            Attack();
        }

        if(Attacked)
        PlayerUnitLog($"{gameObject.name} Cooldown Conditions - " +
            $"State Idle = {_unitState} | " +
            $"Moved = {Moved} | " +
            $"Attacked = {Attacked}");
        if (_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
            _animator.speed = Mathf.Clamp((1 - _cooldown / Cooldown), .2f, 1) * 3 + .1f;
            if (_cooldown <= 0)
            {
                PlayerUnitLog($": {gameObject.name} ends move");
                
                _animator.speed = 1;
                _animator.SetBool("Cooldown", OnCooldown = false);

                //if (_cC != null && _cC.CurrentUnit == null)
                //{
                //    Debug.Log("Current Unit this");
                //    _cC.CurrentUnit = this;
                //}

                _eM?.AddUnit(EnemyController);
            }
        }
        else if (_unitState == Enums.UnitState.Idle && (Moved || Attacked))// && _nextPoint == null)))
        {
            PlayerUnitLog($": {gameObject.name} goes on cooldown");
            GoOnCooldown();
        }
        else if (_unitState == Enums.UnitState.Idle && !OnCooldown && !Moving && !Moved && !Attacked)
        {
            _eM?.AddUnit(EnemyController);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gO = collision.gameObject;
        GridBlock gB = gO.GetComponent<GridBlock>();
        UnitController uC = gO.GetComponent<UnitController>();
        CursorController cc = gO.GetComponent<CursorController>();

        if(gB != null)
        {
            CurrentGridBlock = gB;
            if (_originalPoint == null)
                _originalPoint = gB;
        }
        else if(uC != null && uC.Player != Player && !uC.AlliedWith.Contains(Player) && MinAttackDistance == 0)
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
        else if(cc != null)
        {
            _cC = cc;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //CursorController cc = collision.gameObject.GetComponent<CursorController>();
        //if (cc != null && !_selected && !Moving && !Attacked)
        //{
        //    if (cc.CurrentUnit == this)
        //    {
        //        cc.CurrentUnit = null;
        //        Debug.Log("Current Unit Null");
        //    }
        //    _cC = null;
        //    Reset();
        //}
    }

    private void OnDestroy()
    {
        _pM?.RemoveUnit(Player, this);
        _eM?.RemoveUnit(EnemyController);    
    }

    private void OnCursorMove(Object sender, CursorMoveEventArgs e)
    {
        DistanceFromCursor = transform.position.GridDistance(e.Position);
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
        Target = null;
        _unitState = Enums.UnitState.Attacking;
        _animator.SetTrigger("Launch");
    }

    private void FindGoodPreviousSpot()
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

    private void GoOnCooldown()
    {
        Reset();
        _pM.PlayerUnitMoveDown(Player, this);
        _originalPoint = CurrentGridBlock;
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

    private void PlayerUnitLog(string msg)
    {
        //if (Player == Enums.Player.Player1)
        //    Debug.Log(msg);
    }
}