using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody), typeof(SpriteRenderer), typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator), typeof(Damageable), typeof(GridBlockCosts))]
[RequireComponent(typeof(AudioSource), typeof(AudioSource), typeof(AudioSource))]
[RequireComponent(typeof(Unit_Movement))]
public class Unit_Controller : MonoBehaviour, ILog
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
    public Vector2 HideMoveDistance;
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
    public int MaxAttackDistance;
    public int MinAttackDistance;
    public int MoveDistance;

    public Enums.UnitState UnitState { get; set; }
    public UnitManager UnitManager { get; set; }
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
    private GridBlock _currentGridBlock;
    public GridBlock CurrentGridBlock
    {
        get { return _currentGridBlock; }
        private set
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
    public bool Attacked { get; private set; }
    public bool Moved { get; private set; }
    public bool Moving { get; private set; }
    public bool Blocked { get; private set; }
    public bool Available { get { return !OnCooldown && !Moving; } }
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
        set
        {
            _defaultLook = value;
            ResetLook();
        }
    }


    private Enums.UnitState _prevState;
    private PlayerManager _pM;
    private UnitController _collisionTarget;
    private UnitController _attackTarget;
    private CursorController _cC;
    private GridBlockCosts _gridblockCosts;
    private Animator _animator;
    private SpriteRenderer _sR;
    private Queue<MovePoint> _movePositions;
    private Stack<MovePoint> _prevPositions;
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
    private int _defaultLook;
    public float GridblockSpeedModifier { get; set; }

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
            if (UnitState != Enums.UnitState.PlusAction)
                UnitState = select ? Enums.UnitState.Selected : Enums.UnitState.Idle;
            _animator.SetBool("Selected", _selected = select);
        }
        Log("----------------------------------------");
    }

    public void ReadyAttack(Vector2 pos)
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        LookAt(pos);
        _attack = true;
        _attackPos = pos;
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
            Target = target.ToMovePoint();
            _tasked = true;
        }

        Vector2 tempTarget = new Vector2(999, 999);
        if (_attackTarget != null)
            tempTarget = _attackTarget.Position;
        else if (_collisionTarget != null && _collisionTarget.CurrentGridBlock != null)
            tempTarget = _collisionTarget.CurrentGridBlock.CurrentUnit.Position;
        else if (Target != null)
            tempTarget = Target.Position;

        var dis = Position.GridDistance(tempTarget);
        if (tempTarget != null && ((dis <= MaxAttackDistance && dis >= MinAttackDistance) || ignoreDistance))
        {
            Log("Target found");
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

        _prevState = UnitState;
        UnitState = Enums.UnitState.Attacking;
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
            tmpProjectile.Launch(_attackPos);
            Attacked = true;
        }
    }

    public void ExitAttackState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (Type == Enums.UnitType.Horse && _prevPositions.Count < MoveDistance && !Blocked && _tasked)
        {
            Moved = false;
            UnitState = Enums.UnitState.PlusAction;
            PlusAction = PLUSACTIONTIME;
            PlusActionText.alpha = 1;
            PlusActionText.gameObject.SetActive(true);

            _attackWhenInRange = false;
            _attack = false;
            Target = null;
        }
        else if (Attacked)
        {
            if (Blocked && (Target?.CurrentUnit != null) || _collisionTarget != null)
                CollisionClear();

            UnitState = _prevState == Enums.UnitState.Attacking ? Enums.UnitState.Idle : _prevState; // DO NOT reassign Attacking as a state
            _attackWhenInRange = false;
            _attack = false;
            Target = null;
        }

        ResetLook();
        if (!_tasked)
            Attacked = false; // This means the unit attacked without being tasked to which shouldn't hinder it from attacking again.
        Log("----------------------------------------");
    }

    public void EnterHurtState()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (_selected)
            OnUnitInterupt?.Invoke();

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
        {
            CheckAttack(_collisionTarget.CurrentGridBlock, true);
        }

        UnitState = _prevState == Enums.UnitState.Hurt ? Enums.UnitState.Idle : _prevState; // DO NOT reassign Attacking as a state;
        Log("----------------------------------------");
    }

    public bool IsEnemy(Enums.Player player)
    {
        return Player != player;// && !AlliedWith.Contains(player);
    }

    public void IncreaseMeeleAttackCount()
    {
        MeleeAttackedCount++;
    }

    public int CheckGridMoveCost(Enums.GridBlockType gridType)
    {
        return _gridblockCosts.GetGridblockMoveCost(gridType);
    }

    public void UpdateMinimapIcon()
    {
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
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Player == Enums.Player.Player1)
            _cC = FindObjectOfType<CursorController>();

        if (!OnCooldown && UnitManager)
            UnitManager.AddUnit(this, true);

        _pM = FindObjectOfType<PlayerManager>();
        StartCoroutine(CreateMinimapIcon());

        gameObject.name = $"P{((int)Player) + 1}_" + gameObject.name;

        BoxCollider.size = ColliderSizeIdle;

        if (OffCooldownObject != null)
            Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
    }

    void Update()
    {
        if (CooldownTimer <= 0)
        {
            if (PlusAction > 0)
            {
                PlusAction -= Time.deltaTime;
                PlusActionText.alpha = PlusAction / PLUSACTIONTIME;
            }
            else if (PlusAction <= 0 && UnitState == Enums.UnitState.PlusAction && !_selected)
                UnitState = Enums.UnitState.Idle;

            if (!_attack && Target != null && _movePositions.IsEmpty() && _nextPoint == null)
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

            if (_nextPoint == null && UnitState == Enums.UnitState.Idle && _tasked && !Moving && !_attack && (Moved || Attacked))
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
            CooldownTimer -= Time.deltaTime;
            if (CooldownTimer <= 0)
            {
                Log($"Come of cooldown");
                if (Type == Enums.UnitType.Melee && CooldownReduction)
                    CooldownReduction.gameObject.SetActive(false);

                if (UnitManager && _cC && UnitManager.AvailableUnits <= 0)
                    _cC.SetPosition(CurrentGridBlock.Position);

                _animator.SetBool("Cooldown", OnCooldown = false);
                _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Idle : Colors.Enemy_Idle;

                if (OffCooldownObject != null)
                {
                    Log("Create cooldown object");
                    Instantiate(OffCooldownObject, transform.position, Quaternion.identity);
                }

                if (Player != Enums.Player.Player1 && UnitManager) // This is for non player units to make sure all units are looped through
                    UnitManager.AddUnit(this);

                UnitState = Enums.UnitState.Idle;
            }
        }

        if (Player != Enums.Player.Player1 && _nextPoint == null && UnitState == Enums.UnitState.Idle && !OnCooldown && !Moving && !Moved && !Attacked)
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
            var colDir = uC.Position - Position;
            var roundX = Utility.RoundAwayFromZero(colDir.x);
            var roundY = Utility.RoundAwayFromZero(colDir.y);

            if (roundX == _lookX && roundY == _lookY && Type != Enums.UnitType.Range) // If the unit is facing the unit that it collided with and the unit is not ranged.
            {
                //Target = uC.CurrentGridBlock;
                _collisionTarget = uC;
                DeleteSavedPath();
                ReadyAttack(collision.gameObject.transform.position);
            }
        }
        //Log("----------------------------------------");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        if (_cC != null && collision.gameObject == _cC.gameObject && !_selected && !Moving && !Moved && !Attacked)
        {
            Hover(false);
        }
        //Log("----------------------------------------");
    }

    private void OnDestroy()
    {
        //Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        UnitManager.RemoveUnit(this);
        DeleteSavedPath();
        if (CurrentGridBlock) CurrentGridBlock.ResetCurrentUnit(this);
        Destroy(_miniMapIcon);
        IsDestroyed = true;
        //Log("----------------------------------------");
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
        Hover(false);
        _collisionTarget = null;
        _nextPoint = null;
        _prevPositions.Clear();
        _tasked = false;
        Moved = false;
        Moving = false;
        Attacked = false;
        _movingBack = false;
        Target = null;
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
        if (possiblePoint == null || possiblePoint.Position.GridDistance(Position) > 1) // Make sure the next point is one point away, this should trigger VERY rarely
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
        else
            _movePositions.Dequeue(); // point is fine go ahead and remove it from queue

        if (currentPoint != null && _movePositions.IsEmpty() && possiblePoint.CurrentUnit != null && !possiblePoint.CurrentUnit.IsEnemy(Player)) // if this is the last point in the queue make sure it's not empty
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
        Moving = true;
        _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Moving : Colors.Enemy_Moving;
        UnitState = Enums.UnitState.Moving;
        BoxCollider.size = ColliderSizeMoving;
        _animator.SetBool("Moving", true);
        IsHidden = false;
        LookAt(possiblePoint.Position);
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
        Log("----------------------------------------");
    }

    private void FindGoodPreviousSpot()
    {
        Log($"---------- {MethodBase.GetCurrentMethod().Name} ----------");
        _movePositions.Clear();

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
        UnitState = Enums.UnitState.Cooldown;
        if (_miniMapIcon)
            _miniMapIcon.color = Player == Enums.Player.Player1 ? Colors.Player_Cooldown : Colors.Enemy_Cooldown;
        if (Position == CurrentGridBlock.ToMovePoint(true).Position)
            IsHidden = true;

        //_pM?.PlayerUnitMoveDown(this);
        var crc = CheckReducedCooldown();
        CooldownTimer = Cooldown * (!Attacked ? 1 : 1.4f) * crc;
        _animator.SetBool("Cooldown", OnCooldown = true);
        //Log("----------------------------------------");
    }

    private float CheckReducedCooldown()
    {
        if (this.Type == Enums.UnitType.Melee)
        {
            int friendliesAround = CurrentGridBlock.Neighbors.GetAlliedUnits(Player);
            if (friendliesAround > 0)
            {
                //Debug.Log($"Units found to decrease cooldown. {String.Join(", ", list.Select(l => l.gameObject.name))}");
                CooldownReduction.text = friendliesAround + "";
                CooldownReduction.gameObject.SetActive(true);
            }
            return 1 - (.1f * friendliesAround);
        }
        else
            return 1;
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
        float x = Mathf.Round(lookAt.x - transform.position.x);
        float y = Mathf.Round(lookAt.y - transform.position.y);
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