using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator), typeof(Damageable), typeof(GridBlockCosts))]
[RequireComponent(typeof(AudioSource), typeof(AudioSource), typeof(AudioSource))]
public class UnitController : MonoBehaviour, ILog
{
    public int MAXMOVEAFTERATTACK = 3;
    public float PLUSACTIONTIME = 7.5f;
    public delegate void OnDeath();
    public OnDeath OnUnitInterupt;

    public Enums.Player Player = Enums.Player.Player1;
    public Enums.UnitType Type = Enums.UnitType.Melee;
    public Enums.UnitInfo InfoType;

    public AudioSource AttackAudioSource;
    public AudioSource HurtAudioSource;
    public AudioSource WalkingAudioSource;

    public GameObject OffCooldownObject;
    public GameObject Projectile;
    public GameObject HiddenOverlay;
    public GameObject CooldownIcon;
    public StatusEffect_Handler StatusHandler;
    public Image MinimapIconImage;
    public Image MapTileImage;
    public Sprite UnitGlancePortrait;
    public EnemyController EnemyController;
    public TextMeshProUGUI PlusActionText;
    public TextMeshProUGUI CooldownReduction;
    public List<Enums.GridBlockType> FavorableTerrain;
    public Vector2 ColliderSizeMoving;
    public Vector2 ColliderSizeIdle;
    public Vector2 HideMoveDistance;
    public Vector2 HorAttackVector;
    public Vector2 VerAttackVector;
    public bool OnCooldown = false;
    public bool HiddenByObstacle;
    public bool Hide;
    public float AttackSpeed;
    public float Cooldown;
    public float CooldownTimer;
    public float PlusAction;
    public float Speed;
    public float Damage;
    public float Defense;
    [Tooltip("Used to limit attack back wars. The unit has to wait this long to attack back again.")]
    public float AttackBackCooldown = 3;
    public int MaxAttackDistance;
    public int MinAttackDistance;
    public int MoveDistance;

    //public TextMeshProUGUI DebugText;
    //public float DebugTime;
    //private float _debugTimer;

    private Enums.UnitState _unitState;
    public Enums.UnitState UnitState
    {
        get { return _unitState; }
        set
        {
            //UnityEngine.Debug.Log($"{gameObject.name} state changed to {value}");
            if ((value == Enums.UnitState.Hurt && _unitState == Enums.UnitState.Selected) ||
                value == Enums.UnitState.Selected)
                _sR.sortingOrder = 6;
            else
                _sR.sortingOrder = 0;
            _unitState = value;
        }
    }
    public Enums.UnitStatusEffect StatusEffects
    {
        get { return StatusHandler.Statuses; }
    }

    public UnitManager UnitManager { get; set; }
    public CursorController Cursor {get;set;}
    public UnitGlance UnitGlance { get; set;}

    public MovePoint Target
    {
        get { return _moveTarget; }
        set
        {
            _moveTarget = value;

            if (value != null) _attackTarget = value.CurrentUnit;
            else _attackTarget = null;
        }
    }
    public MovePoint AttackBackTarget { get; set; }

    private GridBlock _currentGridBlock;
    public GridBlock CurrentGridBlock
    {
        get { return _currentGridBlock; }
        set
        {
            _currentGridBlock = value;
            _currentGridMoveCost = _gridblockCosts.GetGridblockMoveCost(_currentGridBlock.Type);
            GridblockSpeedModifier = _gridblockCosts.GetGridblockMultiplyer(_currentGridBlock.Type);
        }
    }

    public BoxCollider2D BoxCollider { get; private set; }
    public SpriteRenderer SpriteRender { get { return _sR; } }

    public Vector2 Position 
    { 
        get { return transform.position; } 
        set { transform.position = value; } 
    }
    public Vector2 LookDirVector { get { return new Vector2(_lookX, _lookY); } }
    public Vector2 AttackedFrom { get; set; }

    public bool Attacked { get; private set; }
    public bool Moved { get; private set; }
    public bool Moving { get; private set; }
    public bool Blocked { get; private set; }
    public bool Available { get { return UnitState == Enums.UnitState.Idle; } }
    public bool TookAction { get { return OnCooldown || Moving || Moved || Attacked; } }
    public bool IsDestroyed { get; private set; }
    public bool AtDestination
    {
        get
        {
            return (!OnCooldown && !Moving) ||
                   (Moving && _movePositions.IsEmpty()) ||
                   OnCooldown;
        }
    }

    public bool IsHidden { get; private set; }
    public bool Overlay { get { return Player == Enums.Player.Player1; } }
    public double DistanceFromCursor { get; private set; }
    public float CurrentHealth { get { return _damagable.Health; } }
    public float GridblockSpeedModifier { get; set; }
    public int CurrentDamage
    {
        get
        {
            return Mathf.Max(1,
                Mathf.FloorToInt(Damage * Mathf.Clamp(_damagable.Health / Damageable.MAXHEALTH, 0, 1)));
        }
    }
    public int AdjustedMoveDistance { get { return UnitState == Enums.UnitState.PlusAction ? Mathf.Clamp(MoveDistance - _prevPositions.Count, 1, 3) : MoveDistance; } }
    public int AdjustedMaxAttackDistance { get { return UnitState == Enums.UnitState.PlusAction ? 0 : MaxAttackDistance; } }
    public int AdjustedMinAttackDistance { get { return UnitState == Enums.UnitState.PlusAction ? 0 : MinAttackDistance; } }
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
    private int _currentGridMoveCost;
    public int CurentGridMoveCost { get { return _currentGridMoveCost; } }
    public int DefaultLook 
    { 
        get { return _defaultLook; } 
        set { _defaultLook = value;
              ResetLook(); } 
    }


    private Enums.UnitState _prevState;
    private PlayerManager _pM;
    private UnitController _collisionTarget;
    private UnitController _attackTarget;
    private GridBlockCosts _gridblockCosts;
    private Animator _animator;
    private SpriteRenderer _sR;
    private Queue<MovePoint> _movePositions;
    private Stack<MovePoint> _prevPositions;
    private Stack<MovePoint> _backupPos;
    private MovePoint _nextPoint;
    private MovePoint _moveTarget;
    private Damageable _damagable;
    private Vector2 _attackPos;
    private Image _miniMapIcon;

    private bool _tasked;
    private bool _selected;
    private bool _attack;
    private bool _movingBack;
    private bool _attackWhenInRange;
    private float _lookX;
    private float _lookY;
    private float _attackBackTimer;
    private int _defaultLook;
    private int _triedFindingPlace;

    private Stopwatch _sW;

    public void Select(bool select)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (CooldownTimer <= 0)
        {
            if (UnitState != Enums.UnitState.PlusAction)
                UnitState = select ? Enums.UnitState.Selected : UnitState; // Enums.UnitState.Idle;
            _animator.SetBool("Selected", _selected = select);
        }
        Log("----------------------------------------");
    }

    public void ReadyAttack(Vector2 pos)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        LookAt(pos);
        _attackPos = pos;
        _nextPoint = null;
        _attack = true;
        _movePositions.Clear();
        Log("----------------------------------------");
    }

    public void MoveTo(List<MovePoint> movePoints, bool attackWhenInRange = false)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Log($"Number of moves: {movePoints.Count}");
        if (CooldownTimer <= 0 && !Moved && movePoints.Count > 0)
        {
            _attackWhenInRange = attackWhenInRange;
            _tasked = true; // Task has been given
            MeleeAttackedCount = 0;

            for (int x = 0; x < movePoints.Count; x++)
            {
                var point = movePoints[x];
                _movePositions.Enqueue(point);
            }

            Log("Moving");
            CurrentGridBlock.ResetCurrentUnit(this);
            GetNextPoint();
        }
        Log("----------------------------------------");
    }

    public bool CheckAttack(GridBlock target = null, bool justChecking = false, GridBlock attackFrom = null)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if(attackFrom == null)
                attackFrom = CurrentGridBlock;

        if (Target == null && target == null && _collisionTarget == null)
        {
            Log("No target found");
            Log("----------------------------------------");
            return false;
        }

        if (OnCooldown && InfoType != Enums.UnitInfo.Soldier)
            return false;

        if (target != null && !justChecking)
        {
            Target = target.ToMovePoint();
            _tasked = true;
        }
        else if(Target != null && !justChecking)
            target = Target.GridBlock;

        if (gameObject.CompareTag("Unit_Arq"))
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(attackFrom.Position, target.Position - attackFrom.Position, attackFrom.Position.GridDistance(target.Position));
            if (hits.Any(h => h.collider.gameObject.tag.Equals("Wall")))
                return false;
        }

        Vector2 tempTarget = new Vector2(999, 999);
        if (_attackTarget != null)
            tempTarget = _attackTarget.Position;
        else if (_collisionTarget != null && _collisionTarget.CurrentGridBlock != null)
            tempTarget = _collisionTarget.Position;
        else if (target != null)
            tempTarget = target.Position;

        var dis = attackFrom.Position.GridDistance(tempTarget);
        if (tempTarget != null && ((dis <= MaxAttackDistance && dis > MinAttackDistance)))// || ignoreDistance))
        {
            Log("Target found");
            if (!justChecking)
                ReadyAttack(tempTarget);
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

        if (Target != null)
            LookAt(Target.Position);
        Log("----------------------------------------");
    }

    public void CreateAttackObject()
    {
        if (!Attacked)
        {
            Vector2 instPos = transform.position.V2();
            if(MaxAttackDistance > 1)
            {
                if (_lookY != 0)
                    instPos += VerAttackVector;
                else
                    instPos += new Vector2(HorAttackVector.x * _lookX, HorAttackVector.y);
            }
            else
                instPos += new Vector2(_lookX * .75f, _lookY * .75f);

            GameObject projObj = Instantiate(Projectile, instPos , Quaternion.identity);
            Damager damager = projObj.GetComponent<Damager>();
            damager.Player = Player;
            damager.Unit = this;
            damager.Damage = CurrentDamage;
            damager.StatusHandler = StatusHandler;

            Projectile tmpProjectile = projObj.GetComponent<Projectile>();
            var tmpDir = _attackPos - transform.position.V2();
            tmpDir.Normalize();
            tmpProjectile.Launch(_attackPos);
            Attacked = true;
        }
    }

    public void ExitAttackState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        //if (Type == Enums.UnitType.Horse && _prevPositions.Count < MoveDistance && !Blocked && _tasked)
        //{
        //    Moved = false;
        //    UnitState = Enums.UnitState.PlusAction;
        //    PlusAction = PLUSACTIONTIME;
        //    PlusActionText.alpha = 1;
        //    PlusActionText.gameObject.SetActive(true);

        //    _attackWhenInRange = false;
        //    _attack = false;
        //    Target = null;
        //}
        //else 
        if (Attacked)
        {
            if (Blocked && (Target?.CurrentUnit != null) || _collisionTarget != null)
                CollisionClear();

            // DO NOT reassign Attacking as a state, just a fail safe
            UnitState = _prevState == Enums.UnitState.Attacking ? Enums.UnitState.Idle : _prevState; 
            _attackWhenInRange = false;
            _attack = false;
            Target = null;
            _attackBackTimer = AttackBackCooldown;
            AttackBackTarget = null;
        }
        
        ResetLook();
        if (!_tasked)
            Attacked = false; // This means the unit attacked without being tasked to which shouldn't hinder it from attacking again.
        Log("----------------------------------------");
    }

    public void EnterBlockState()
    {
        LookAt(AttackedFrom);

        _prevState = UnitState;
        UnitState = Enums.UnitState.Blocking;
    }

    public void ExitBlockState()
    {
        if (AttackBackTarget != null)
        {
            Target = AttackBackTarget;
            AttackBackTarget = null;
            _tasked = false;
        }

        UnitState = _prevState; // DO NOT reassign Attacking as a state;
        ResetLook();
    }

    public void EnterHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        
        LookAt(AttackedFrom);

        _prevState = UnitState;
        UnitState = Enums.UnitState.Hurt;

        Log("----------------------------------------");
    }

    public void ExitHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if ((Blocked || Moving) && !Attacked)
            CollisionClear();

        if (!TookAction && (!_tasked || UnitState == Enums.UnitState.Attacking) && _collisionTarget != null)
            CheckAttack(_collisionTarget.CurrentGridBlock);
        else if(!_movingBack)
            ResetLook();

        if (AttackBackTarget != null && _attackBackTimer <= 0)
        {
            if (CheckAttack(AttackBackTarget.GridBlock) && OnCooldown)
                Attack();
            AttackBackTarget = null;
            _tasked = false;
        }

        UnitState = _prevState == Enums.UnitState.Attacking ? Enums.UnitState.Idle : _prevState; // DO NOT reassign Attacking as a state;

        if (UnitState == Enums.UnitState.Selected)
        {
            OnUnitInterupt?.Invoke();
            Reset();
        }
        Log("----------------------------------------");
    }

    public bool IsEnemy(Enums.Player player)
    {
        return Player != player;// && !AlliedWith.Contains(player);
    }

    public int CheckGridMoveCost(Enums.GridBlockType gridType)
    {
        return _gridblockCosts.GetGridblockMoveCost(gridType);
    }

    public void UpdateMinimapIcon()
    {
        if (_miniMapIcon)
            _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
    }

    void Awake()
    {
        BoxCollider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _damagable = GetComponent<Damageable>();
        _gridblockCosts = GetComponent<GridBlockCosts>();

        UnitState = Enums.UnitState.Idle;
        _movePositions = new Queue<MovePoint>();
        _prevPositions = new Stack<MovePoint>();
        _nextPoint = null;
        GridblockSpeedModifier = 1;

        _sW = new Stopwatch();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Player == Enums.Player.Player1)
            Cursor = FindObjectOfType<CursorController>();

        _animator.keepAnimatorStateOnDisable = false;

        _pM = FindObjectOfType<PlayerManager>();
        if(_pM.Minimap_UnitIcons)
            StartCoroutine(CreateMinimapIcon());

        BoxCollider.size = ColliderSizeIdle;

        if (OffCooldownObject != null)
            Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
    }

    void Update()
    {
        if (CooldownTimer <= 0 && LevelManager.Instance?.GameState != Enums.GameState.TimeStop)
        {
            CheckState();

            if (PlusAction > 0 && PlusActionText)
            {
                PlusAction -= Time.deltaTime;
                PlusActionText.alpha = PlusAction / PLUSACTIONTIME;
            }
            else if (PlusAction <= 0 && UnitState == Enums.UnitState.PlusAction && !_selected)
                UnitState = Enums.UnitState.Idle;
            if(!_attack && UnitState == Enums.UnitState.Moving && _nextPoint == null && _movePositions.IsEmpty() && !_prevPositions.IsEmpty())
            {
                FindGoodPreviousSpot();
            }

            if ((_nextPoint != null || _movePositions.Count > 0) && UnitState != Enums.UnitState.Attacking && UnitState != Enums.UnitState.Hurt)
            {
                if (_nextPoint == null)
                    GetNextPoint();

                Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Position, Speed * GridblockSpeedModifier * Time.deltaTime);
                _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);

                LookAt(moveVector);
                transform.position = moveVector;
                if (transform.position.V2() == _nextPoint.Position)
                {
                    Log("Arrived at point");
                    var fod = _prevPositions.FirstOrDefault();
                    fod?.Path_Delete(this);
                    _prevPositions.Push(_nextPoint);

                    if(_attackWhenInRange && CheckAttack())
                    {
                        _attack = true;
                    }
                    else if (!_movePositions.IsEmpty())
                    {
                        Log("Get next point");
                        GetNextPoint();
                    }
                    else
                    {
                        if (CurrentGridBlock != _nextPoint.GridBlock) // Arrived at the last point, make sure it's the currentgridblock
                            CurrentGridBlock = _nextPoint.GridBlock;
                        Log("Last move");
                        _nextPoint = null;
                    }

                    if (Moving && _movePositions.IsEmpty() && _nextPoint == null)
                    {
                        if (CurrentGridBlock.CurrentUnit == null)
                        {
                            Log($"Ends move");
                            Moving = false;
                            Moved = true;

                            CurrentGridBlock.SetCurrentUnit(this);
                            UnitState = Enums.UnitState.Idle;
                            //_sW.Stop();
                            //double totalSecs = (_sW.Elapsed.TotalMilliseconds / 1000);
                            //double speed = (_prevPositions.Count - 1) / totalSecs;
                            //UnityEngine.Debug.Log($"{gameObject.name}: Moving {_prevPositions.Count - 1} blocks took {totalSecs.ToString("F1")} for a speed of {speed.ToString("F1")}");
                            //if(DebugText)
                            //DebugText.text += $"{gameObject.name}: Moving {_prevPositions.Count - 1} blocks took {totalSecs.ToString("F1")} for a speed of {speed.ToString("F1")}\n";
                        }
                        else if(_prevPositions.Count > 1)
                        {
                            FindGoodPreviousSpot();
                        }
                    }
                }
            }

            if (!_attack && Target != null && _movePositions.IsEmpty() && _nextPoint == null)
            {
                Log($"checks for attack");
                  CheckAttack();
            }

            if (!_selected && !Moving && _animator.GetBool("Moving"))
                _animator.SetBool("Moving", false);

            if (_attack && !Attacked && (UnitState == Enums.UnitState.Idle || UnitState == Enums.UnitState.Moving))//!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                Log($"attacks");
                Attack();
            }

            if (_nextPoint == null && _movePositions.IsEmpty() && UnitState == Enums.UnitState.Idle && _tasked && !Moving && !_attack && (Moved || Attacked))
            {
                Log($"goes on cooldown");
                if (CurrentGridBlock.CurrentUnit && CurrentGridBlock.CurrentUnit != this)
                {
                    FindGoodPreviousSpot();
                }
                else
                {
                    GoOnCooldown();
                }
            }
        }

        if (CooldownTimer > 0)
        {
            CooldownTimer -= Time.deltaTime * UnitManager.CDReductionMult;
            if (CooldownTimer <= 0)
            {
                Log($"Come of cooldown");

                if (Type == Enums.UnitType.Melee && CooldownReduction)
                    CooldownReduction.gameObject.SetActive(false);

                if (UnitManager && Cursor && Cursor.CursorState == Enums.CursorState.Default && UnitManager.AvailableUnits <= 0)
                    Cursor.SetPosition(CurrentGridBlock.Position);

                _animator.SetBool("Cooldown", OnCooldown = false);
                _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Idle : Colors.Enemy_Idle;

                if (OffCooldownObject != null)
                {
                    Log("Create cooldown object");
                    Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
                }

                if (Player != Enums.Player.Player1 && UnitManager) // This is for non player units to make sure all units are looped through
                    ((EnemyManager)UnitManager).AddBackToQueue(this);

                CooldownIcon.SetActive(false);
                UnitState = Enums.UnitState.Idle;
            }
        }

        if (Player != Enums.Player.Player1 && _nextPoint == null && 
            (UnitState == Enums.UnitState.Idle || UnitState == Enums.UnitState.Cooldown) && 
            !OnCooldown && !Moving && !Moved && !Attacked)
        {
            UnitState = Enums.UnitState.Idle;
            // This is just a failsafe. In case the unit isn't doing anything and it somehow
            // got removed from it's manager.
            if (!UnitManager.Units.Contains(this))
            {
                Log($"added to unit list");
                UnitManager.AddUnit(this);
            }
            if (!((EnemyManager)UnitManager).UnitQueue.Contains(this)) // This is for non player units to make sure all units are looped through
                ((EnemyManager)UnitManager).AddBackToQueue(this);
        }

        if (_attackBackTimer > 0)
            _attackBackTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        var gO = collision.gameObject;
        GridBlock gB = gO.GetComponent<GridBlock>();
        UnitController uC = gO.GetComponent<UnitController>();

        if (gB != null)
        {
            //Log("Gridblock entered");
            if (gB.CurrentUnit != null && _movePositions.IsEmpty() && !gB.CurrentUnit.IsEnemy(Player))
            {
                //Log("Destination block is occupied, find new block");
                FindGoodPreviousSpot();
            }

            if (CurrentGridBlock == null) // This only triggers at the beggining of the scene
                gB.SetCurrentUnit(this);

            CurrentGridBlock = gB;
            GridblockSpeedModifier = _gridblockCosts.GetGridblockMultiplyer(gB.Type);
        }
        else if (!OnCooldown && uC != null && uC.IsEnemy(Player)) // Collided with an enemy
        {
            Log("Collided with enemy unit");
            Blocked = true;
            var colDir = uC.Position - Position;// - uC.Position;
            float roundX = Utility.RoundAwayFromZero(colDir.x);
            float roundY = Utility.RoundAwayFromZero(colDir.y);

            if (roundX == _lookX && roundY == _lookY && Type != Enums.UnitType.Range) // If the unit is facing the unit that it collided with and the unit is not ranged.
            {
                //Target = uC.CurrentGridBlock;
                _collisionTarget = uC;
                DeleteSavedPath();
                ReadyAttack(CurrentGridBlock.Position + new Vector2(roundX, roundY));
            }
        }
        //Log("----------------------------------------");
    }

    private void OnDestroy()
    {
        //Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if(UnitManager)
            UnitManager.RemoveUnit(this);
        DeleteSavedPath();
        if (CurrentGridBlock) 
            CurrentGridBlock.ResetCurrentUnit(this);
        Destroy(_miniMapIcon);
        IsDestroyed = true;
        //Log("----------------------------------------");
    }

    private void CheckState()
    {
        if (UnitState == Enums.UnitState.Selected && Cursor.CurrentUnit != this)
            UnitState = Enums.UnitState.Idle;
    }

    private IEnumerator CreateMinimapIcon()
    {
        yield return new WaitUntil(() => _pM.FullGrid != null);

        _miniMapIcon = Instantiate(MinimapIconImage);
        _miniMapIcon.rectTransform.SetParent(_pM.Minimap_UnitIcons.transform);
        float squareSize = _pM.MinimapSquareSize;
        _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, squareSize);
        _miniMapIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, squareSize);
        _miniMapIcon.rectTransform.anchoredPosition = Utility.UITilePosition(_miniMapIcon.rectTransform, transform);
        _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Idle : Colors.Enemy_Idle;
    }

    private void Reset()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Select(false);
        _collisionTarget = null;
        _nextPoint = null;
        _prevPositions.Clear();
        _backupPos = null;
        _tasked = false;
        Moved = false;
        Moving = false;
        Attacked = false;
        _movingBack = false;
        Target = null;
        _triedFindingPlace = 0;
        if (PlusActionText) PlusActionText.gameObject.SetActive(false);
        BoxCollider.size = ColliderSizeIdle;
        ResetLook();
        Log("----------------------------------------");
    }

    private float AttackTargetDistance()
    {
        if (_attackTarget != null)
            return Position.GridDistance(_attackTarget.Position);
        else
            return Position.GridDistance(Target.Position);
    }

    private void GetNextPoint()
    {
        var currentPoint = _nextPoint;
        var possiblePoint = _movePositions.Peek();

        // Check if it's the start position
        if(currentPoint == null && possiblePoint.Position == Position)
        {
            // If it is, save it and move to the first new position
            // It is still important to save this first position in case we need to go back to it.
            _prevPositions.Push(possiblePoint); 
            _movePositions.Dequeue();
            currentPoint = possiblePoint;
            if (!_movePositions.TryPeek(out possiblePoint))
                possiblePoint = null;
        }

        if (possiblePoint != null && possiblePoint.Position.GridDistance(Position) > 1) // Make sure the next point is one point away, this should trigger VERY rarely
        {
            var orderedNeighbors = CurrentGridBlock.Neighbors.OrderByDistance(possiblePoint.GridBlock);
            foreach (GridBlock gB in orderedNeighbors)
            {
                if (gB.Position.GridDistance(possiblePoint.Position) <= 1)
                {
                    possiblePoint = gB.ToMovePoint();
                    break;
                }
            }
        }
        else if(possiblePoint != null)
            _movePositions.Dequeue(); // point is fine go ahead and remove it from queue

        if (currentPoint != null && possiblePoint != null && _movePositions.IsEmpty() && possiblePoint.CurrentUnit != null && !possiblePoint.CurrentUnit.IsEnemy(Player)) // if this is the last point in the queue make sure it's not empty
        {
            if (currentPoint.CurrentUnit == this) // Last spot is taken and current spot is safe to stay at
            {
                _nextPoint = null;
                return;
            }
            else // current spot isn't safe so find a new spot
            {
                FindGoodPreviousSpot();
                GetNextPoint();
                return;
            }
        }

        _nextPoint = possiblePoint;

        if (_nextPoint != null)
        {
            Moving = true;
            //_miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Moving : Colors.Enemy_Moving;
            //UnityEngine.Debug.Log($"{gameObject.name} is moving");
            UnitState = Enums.UnitState.Moving;
            BoxCollider.size = ColliderSizeMoving;
            _animator.SetBool("Moving", true);
            IsHidden = false;
            LookAt(_nextPoint.Position);
        }
    }

    private void DeleteSavedPath()
    {
        var fod = _prevPositions.FirstOrDefault();
        fod?.Path_Delete(this);
        _nextPoint?.Path_Delete(this);
        while (_movePositions.Count != 0)
        {
            var pos = _movePositions.Dequeue();
            pos?.Path_Delete(this);
        }
    }

    private void CollisionClear()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (!_movingBack)
        {
            UnitState = Enums.UnitState.Idle;
            Target = null;
            _collisionTarget = null;
            _attack = false;
            DeleteSavedPath();
            _attackWhenInRange = false;
            if (!_prevPositions.IsEmpty())
            {
                FindGoodPreviousSpot();
                _movingBack = true;
            }
            Blocked = false;
        }
        Log("----------------------------------------");
    }

    private void Attack()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _animator.SetTrigger("Launch");
        _prevState = UnitState;
        UnitState = Enums.UnitState.Attacking;
        Log("----------------------------------------");
    }

    private void FindGoodPreviousSpot()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _movePositions.Clear();
        _triedFindingPlace++;

        if(_triedFindingPlace > 3)
            _prevPositions = _backupPos;

        if (_backupPos == null)
            _backupPos = new Stack<MovePoint>(_prevPositions);

        foreach (var pos in _prevPositions)
        {
            _movePositions.Enqueue(pos);
            if (pos.CurrentUnit == null || pos.CurrentUnit == this)
                break;
        }

        _prevPositions.Clear();
        if (_nextPoint != null)
        {
            _prevPositions.Push(_nextPoint);
            _nextPoint = null;
        }
        Log("----------------------------------------");
    }

    public void GoOnCooldown()
    {
        //Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        Reset();
        if (_miniMapIcon)
            _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Cooldown : Colors.Enemy_Cooldown;
        if (Position == CurrentGridBlock.ToMovePoint(true).Position)
            IsHidden = true;

        //_pM?.PlayerUnitMoveDown(this);
        var crc = 1;//CheckReducedCooldown();
        CooldownTimer = Cooldown * (!Attacked ? 1 : 1.4f) * crc;
        _animator.SetBool("Cooldown", OnCooldown = true);
        UnitState = Enums.UnitState.Cooldown;
        CooldownIcon.SetActive(true);
        //Log("----------------------------------------");
    }

    private void ResetLook()
    {
        //Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
            _animator.SetFloat("Look X", _lookX = _defaultLook);
            _animator.SetFloat("Look Y", _lookY = 0);
        //Log("----------------------------------------");
    }

    private void LookAt(Vector2 lookAt)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        float x = lookAt.x - transform.position.x;
        float y = lookAt.y - transform.position.y;
        Vector2 normalLookAt = new Vector2(x, y).normalized;
        _animator.SetFloat("Look X", _lookX = normalLookAt.x);// == 0 ? 0 : x > 0 ? 1 : -1));
        _animator.SetFloat("Look Y", _lookY = normalLookAt.y);// == 0 ? 0 : y > 0 ? 1 : -1));
        Log("----------------------------------------");
    }

    public void Log(string msg)
    {
        DebugLogger.Instance?.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new NotImplementedException();
    }
}