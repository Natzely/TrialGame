using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UnitController : MonoBehaviour, ILog
{
    public int MAXMOVEAFTERATTACK = 3;
    public float PLUSACTIONTIME = 7.5f;
    public delegate void OnDeath();
    public OnDeath OnUnitInterupt;

    public Enums.Player Player = Enums.Player.Player1;
    public Enums.UnitType Type = Enums.UnitType.Melee;

    public AudioSource AttackAudioSource;
    public AudioSource HurtAudioSource;
    public AudioSource WalkingAudioSource;

    public GameObject OffCooldownObject;
    public GameObject Projectile;
    public Image MinimapIconImage;
    public Image MapTileImage;
    public EnemyController EnemyController;
    public TextMeshProUGUI PlusActionText;
    public TextMeshProUGUI CooldownReduction;
    public List<Enums.GridBlockType> FavorableTerrain;
    public Vector2 ColliderSizeMoving;
    public Vector2 ColliderSizeIdle;
    public bool OnCooldown = false;
    public float AttackSpeed;
    public float Cooldown;
    public float CooldownTimer;
    public float PlusAction;
    public float Speed;
    public float Damage;
    public float Defense;
    public int MaxAttackDistance;
    public int MinAttackDistance;
    public int MoveDistance;

    public UnitManager UnitManager { get; set; }
    public GridBlock Target { get; set; }
    public GridBlock CurrentGridBlock { get; private set; }
    public Vector2 LookDirVector { get { return new Vector2(_lookX, _lookY); } }
    public bool Attacked { get; private set; }
    public bool Moved { get; private set; }
    public bool Moving { get; private set; }
    public bool Blocked { get; private set; }
    public bool TookAction { get { return OnCooldown || Moving || Moved || Attacked; } }
    public double DistanceFromCursor { get; private set; }
    public int AdjustedMoveDistance { get { return _unitState == Enums.UnitState.PlusAction ? Mathf.Clamp(MoveDistance - _pastPositions.Count, 1, 3) : MoveDistance; } }
    public int AdjustedMaxAttackDistance { get { return _unitState == Enums.UnitState.PlusAction ? 0 : MaxAttackDistance; } }
    public int AdjustedMinAttackDistance { get { return _unitState == Enums.UnitState.PlusAction ? 0 : MinAttackDistance; } }
    public int MeleeAttackedCount { get; private set; }
    public int LookDirIndex
    {
        get
        {
            if (_lookX != 0)
                return 0;
            else
                return 1;
        }
    }

    private Enums.UnitState _unitState;
    private Enums.UnitState _prevState;
    private UnitController _collisionTarget;
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
    private Image _minimapIcon;

    private bool _tasked;
    private bool _selected;
    private bool _attack;
    private bool _movingBack;
    private float _defaultLook;
    private float _lookX;
    private float _lookY;

    private float[,] _bonusDamageLookup = new float[,]
    {             //Meele   Ranged   Horse
        /*Melee*/  {  1,      1,       1 },
        /*Ranged*/ {  1,      1,     1.5f},
        /*Horse*/  {  2,      2,       2 }
    };

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
        if (CooldownTimer <= 0)
        {
            if (_unitState != Enums.UnitState.PlusAction)
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
        if (CooldownTimer <= 0 && !Moved && movePoints.Count > 0)
        {
            _tasked = true; // Task has been given
            MeleeAttackedCount = 0;

            for (int x = 0; x < movePoints.Count; x++)
            {
                var point = movePoints[x];
                _movePositions.Enqueue(point);
            }

            Log("Moving");
            Moving = true;
            _bC.size = ColliderSizeMoving;
            _animator.SetBool("Moving", true);
        }
        Log("----------------------------------------");
    }

    public void CancelMove()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _animator.SetBool("Moving", false);
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

    public bool CheckAttack(GridBlock target = null, bool ignoreDistance = false)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (Target == null && target == null && _collisionTarget == null)
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

        GridBlock tempTarget = null;
        if (Target != null)
            tempTarget = Target;
        else
            tempTarget = _collisionTarget.CurrentGridBlock;
        

        var dis = Position.GridDistance(tempTarget.Position);
        if (tempTarget != null && ((dis <= MaxAttackDistance && dis >= MinAttackDistance) || ignoreDistance))
        {
            Log("Target found");
            ReadyAttack(tempTarget.Position);
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
        Log("----------------------------------------");
    }

    public void CreateAttackObject()
    {
        if (!Attacked)
        {
            GameObject projObj = Instantiate(Projectile, (Vector2)transform.position, Quaternion.identity);
            Damager damager = projObj.GetComponent<Damager>();
            damager.Player = Player;
            damager.Parent = this;
            damager.Damage = Mathf.Max(1, Mathf.FloorToInt(Damage * (_damagable.Health / Damageable.MAXHEALTH)));

            Projectile tmpProjectile = projObj.GetComponent<Projectile>();
            var tmpDir = _attackPos - transform.position.V2();
            tmpDir.Normalize();
            tmpProjectile.Launch(tmpDir, AttackSpeed, MaxAttackDistance);
            Attacked = true;
        }
    }

    public void ExitAttackState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (Type == Enums.UnitType.Horse && _pastPositions.Count < MoveDistance && !Blocked)
        {
            Moved = false;
            _unitState = Enums.UnitState.PlusAction;
            PlusAction = PLUSACTIONTIME;
            PlusActionText.alpha = 1;
            PlusActionText.gameObject.SetActive(true);
            _originalPoint = CurrentGridBlock;
            ResetLook();

            _attack = false;
            Target = null;
        }
        else if(Attacked)
        {
            if (Blocked && (Target?.CurrentUnit != null || _collisionTarget != null))
                CollisionClear();

            _unitState = Enums.UnitState.Idle;
            _attack = false;
            Target = null;
        }

        if (!_tasked)
            Attacked = false; // This means the unit attacked without being tasked to which shouldn't hinder it from attacking again.
        Log("----------------------------------------");
    }

    public void EnterHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (_selected)
            OnUnitInterupt?.Invoke();
        _prevState = _unitState;
        _unitState = Enums.UnitState.Hurt;
        Log("----------------------------------------");
    }

    public void ExitHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _unitState = _prevState;

        if ((Blocked || Moving) && _unitState != Enums.UnitState.Attacking)
            CollisionClear();

        if (!TookAction && (!_tasked || _unitState == Enums.UnitState.Attacking) && _collisionTarget != null)
        {
            CheckAttack(_collisionTarget.CurrentGridBlock, true);
        }
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
        Log("----------------------------------------");
    }

    public void IncreaseMeeleAttackCount()
    {
        MeleeAttackedCount++;
    }

    private void Reset()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Select(false);
        Hover(false);
        _collisionTarget = null;
        _nextPoint = null;
        _pastPositions.Clear();
        _tasked = false;
        Moved = false;
        Moving = false;
        Attacked = false;
        _movingBack = false;
        Target = null;
        PlusActionText?.gameObject.SetActive(false);
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

        try
        {
            var uiParent = GameObject.FindGameObjectWithTag("UI");
            var minimapPanel = uiParent.FindObject("UnitIcons");
            _minimapIcon = Instantiate(MinimapIconImage);
            _minimapIcon.color = Player == Enums.Player.Player1 ? Color.green : Color.red;
            _minimapIcon.rectTransform.SetParent(minimapPanel.transform);
            _minimapIcon.rectTransform.anchoredPosition = new Vector2(MapTileImage.rectTransform.rect.width * (transform.position.x - .5f), MapTileImage.rectTransform.rect.height * (transform.position.y - .5f));
            _minimapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Idle : Colors.Enemy_Idle;
        }
        catch { Debug.Log("failed unit minimap"); }

        gameObject.name = $"P{((int)Player) + 1}_" + gameObject.name;

        _bC.size = ColliderSizeIdle;
        ResetLook();

        if (OffCooldownObject != null)
            Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
    }

    void Update()
    {
        if (PlusAction > 0)
        {
            PlusAction -= Time.deltaTime;
            PlusActionText.alpha = PlusAction / PLUSACTIONTIME;
        }
        else if (PlusAction <= 0 && _unitState == Enums.UnitState.PlusAction && !_selected)
            _unitState = Enums.UnitState.Idle;

        if ((_nextPoint != null || _movePositions.Count > 0) && _unitState != Enums.UnitState.Attacking && _unitState != Enums.UnitState.Hurt)
        {
            if (_nextPoint == null && !_movePositions.IsEmpty())
            {
                _minimapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Moving : Colors.Enemy_Moving;
                _nextPoint = _movePositions.Dequeue();
                LookAt(_nextPoint.Position);
            }

            Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Position, Speed * Time.deltaTime);
            _minimapIcon.rectTransform.anchoredPosition = new Vector2(MapTileImage.rectTransform.rect.width * (transform.position.x - .5f), MapTileImage.rectTransform.rect.height * (transform.position.y - .5f));

            transform.position = moveVector;
            if (transform.position.V2() == _nextPoint.Position)
            {
                CurrentGridBlock = _nextPoint;
                Log("Arrived at point");
                _pastPositions.FirstOrDefault()?.DeletePath(this);
                _pastPositions.Push(_nextPoint);
                if (!_movePositions.IsEmpty())
                {
                    Log("Get next point");
                    _nextPoint = _movePositions.Dequeue();
                    Log("Check if last move is empty");
                    if (_nextPoint.CurrentUnit != null && _nextPoint.CurrentUnit.Player == Player && _movePositions.IsEmpty())
                    {
                        if(_nextPoint.CurrentUnit.Player == Player && !_movePositions.IsEmpty())
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

                    _unitState = _selected ? Enums.UnitState.Selected : Enums.UnitState.Idle;
                }
            }
        }

        if(!_attack && Target != null && _movePositions.IsEmpty() && _nextPoint == null)
        {
            Log($"checks for attack");
            CheckAttack();
        }
         
        if (!_selected && !Moving && _animator.GetBool("Moving"))
            _animator.SetBool("Moving", false);

        if (_attack && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Launch") && !Attacked)
        {
            Log($"attacks");
            Attack();
        }

        if (CooldownTimer > 0)
        {
            CooldownTimer -= Time.deltaTime;
            if (CooldownTimer <= 0)
            {
                Log($"Come of cooldown");
                if(Type == Enums.UnitType.Melee)
                    CooldownReduction?.gameObject.SetActive(false);

                _animator.SetBool("Cooldown", OnCooldown = false);
                _minimapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Idle : Colors.Enemy_Idle;

                if (OffCooldownObject != null)
                {
                    Log("Create cooldown object");
                    Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
                }

                if (Player != Enums.Player.Player1) // This is for non player units to make sure all units are looped through
                    UnitManager?.AddUnit(this);
            }
        }
        else if (_nextPoint == null && _unitState == Enums.UnitState.Idle && _tasked && !Moving && (Moved || Attacked))
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
        else if(!OnCooldown && uC != null && uC.IsEnemy(Player) && Type != Enums.UnitType.Range)// && !uC.AlliedWith.Contains(Player))
        {
            Log("Collided with enemy unit");
            Blocked = true;
            var colDir = uC.Position - Position;
            var roundX = Utility.RoundAwayFromZero(colDir.x);
            var roundY = Utility.RoundAwayFromZero(colDir.y);

            if (roundX == _lookX && roundY == _lookY) // If the unit is facing the unit that it collided with.
            {
                //Target = uC.CurrentGridBlock;
                _collisionTarget = uC;
                ReadyAttack(collision.gameObject.transform.position);
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

    private void CollisionClear()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (!_movingBack)
        {
            _unitState = Enums.UnitState.Idle;
            _movingBack = true;
            Target = null;
            _movePositions.Clear();
            DamageResults(false);
            Blocked = false;
        }
        Log("----------------------------------------");
    }

    private void Attack()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _unitState = Enums.UnitState.Attacking;
        _animator.SetTrigger("Launch");
        Log("----------------------------------------");
    }

    private void FindGoodPreviousSpot()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _nextPoint = null;
        foreach (var pos in _pastPositions)
        {
            _movePositions.Enqueue(pos);
            if (pos.CurrentUnit == null || pos.CurrentUnit == this)
                break;
        }
        Log("----------------------------------------");
    }

    private void GoOnCooldown()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Reset();
        _minimapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Cooldown : Colors.Enemy_Cooldown;
        //_pM?.PlayerUnitMoveDown(this);
        _originalPoint = CurrentGridBlock;
        var crc = CheckReducedCooldown();
        CooldownTimer = Cooldown * (!Attacked ? 1 : 1.4f) * crc;
        _animator.SetBool("Cooldown", OnCooldown = true);
        Log("----------------------------------------");
    }

    private float CheckReducedCooldown()
    {
        if (this.Type == Enums.UnitType.Melee)
        {
            var list = CurrentGridBlock.Neighbors.Where(n => n.CurrentUnit != null && n.CurrentUnit != this && !n.IsCurrentUnitEnemy(Player)).ToList();
            if (list.Count > 0)
            {
                Debug.Log($"Units found to decrease cooldown. {String.Join(", ", list.Select(l => l.gameObject.name))}");
                CooldownReduction.text = list.Count + "";
                CooldownReduction.gameObject.SetActive(true);
            }
            return 1 - (.1f * list.Count);
        }
        else
            return 1;
    }

    private void ResetLook()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _animator.SetFloat("Look X", _lookX = _defaultLook);
        _animator.SetFloat("Look Y", _lookY = 0);
        Log("----------------------------------------");
    }

    private void LookAt(Vector2 lookAt)
    {
         Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        float x = lookAt.x - transform.position.x;
        float y = lookAt.y - transform.position.y;
        _animator.SetFloat("Look X", _lookX = (x == 0 ? 0 : x > 0 ? 1 : -1)); 
        _animator.SetFloat("Look Y", _lookY = (y == 0 ? 0 : y > 0 ? 1 : -1));
        Log("----------------------------------------");
    }

    public void Log(string msg)
    {
        UnitManager.Log($"{gameObject.name} | {msg}");
    }

    public void LogError(string msg)
    {
        throw new NotImplementedException();
    }
}