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

    PlayerManager _pm;
    EnemyManager _em;
    Animator _animator;
    SpriteRenderer _sR;
    Queue _movePositions;
    Vector2? _nextPoint;
    Vector2 _lastPoint;
    Vector2 _originalPoint;
    Vector2 _attackPos;

    bool _hover;
    bool _selected;
    bool _attack;
    float _cooldown;

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

        if (Vector2.Distance(Position, Target.Position) <= AttackDistance)
        {
            ReadyAttack(Target.Position);
            return true;
        }

        return false;
    }

    void Awake()
    {
        Moved = false;
        _hover = false;
        _selected = false;
        _nextPoint = null;
        _movePositions = new Queue();
        _originalPoint = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
        _pm = FindObjectOfType<PlayerManager>();
        if (Player != Enums.Player.Player1)
            _em = FindObjectOfType<EnemyManager>();

        _em?.AddUnit(EnemyController);

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

        _pm.AddPlayerUnit(Player, this);
    }

    void Update()
    {
        if(!_movePositions.IsEmpty() || _nextPoint != null)
        {
            if (_nextPoint == null)
            {
                _nextPoint = (Vector2)_movePositions.Dequeue();
                _lastPoint = transform.position;

                LookAt(_nextPoint.Value);
            }

            if (!CheckForBlock())
            {
                Vector2 moveVector = Vector2.MoveTowards(transform.position, _nextPoint.Value, Speed * Time.deltaTime);

                transform.position = moveVector;
                if (transform.position.V2() == _nextPoint.Value)
                {
                    _nextPoint = null;

                    if (_movePositions.IsEmpty())
                    {
                        Moving = false;
                        Moved = true;
                        if (Player != Enums.Player.Player1)
                            CheckAttack();
                        if (!_selected && !_attack)
                            Selected();
                    }
                }
            }
        }

        if (_movePositions.IsEmpty() && _nextPoint == null && _attack)
        {
            Attack();
            _attack = false;
            Selected();
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

                _em?.AddUnit(EnemyController);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gO = collision.gameObject;
        GridBlock gB = gO.GetComponent<GridBlock>();
        if(gB != null)
        {
            CurrentGridBlock = gB;
        }
    }

    private bool CheckForBlock()
    {
        var dir = _nextPoint - Position;
        RaycastHit2D hit = Physics2D.Raycast(Position, dir.Value, 1f, LayerMask.GetMask("Unit"));
        if (hit.collider != null)
        {
            UnitController uC = hit.transform.gameObject.GetComponent<UnitController>();
            if (uC != null && uC.Player != Player && !AlliedWith.Contains(uC.Player))
            {
                _movePositions.Clear();
                _nextPoint = null;
                ReadyAttack(hit.transform.position);

                return true;
            }
        }
        return false;
    }

    private void Selected()
    {
        _animator.SetBool("Selected", _selected);

        if (!_selected && (Moved || Attacked))
        {
            GoOnCooldown(Attacked);
        }
    }

    private void Attack()
    {
        LookAt(_attackPos);

        Attacked = true;
        Vector2 dir = new Vector2(_animator.GetFloat("Look X"), _animator.GetFloat("Look Y"));
        _animator.SetTrigger("Launch");
        GameObject projObj = Instantiate(Projectile, (Vector2)transform.position + (dir * .5f), Quaternion.identity);
        Damager damager = projObj.GetComponent<Damager>();
        damager.Player = Player;

        Projectile tmpProjectile = projObj.GetComponent<Projectile>();
        var tmpDir = _attackPos - transform.position.V2();
        tmpDir.Normalize();
        tmpProjectile.Launch(tmpDir, AttackSpeed, AttackDistance);

        StartCoroutine(Deselect());
    }

    private void GoOnCooldown(bool attack)
    {
        _originalPoint = transform.position;
        ResetLook();
        _animator.SetBool("Selected", false);
        Hover(false);
        _cooldown = Cooldown * (!attack ? .6f : 1);
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