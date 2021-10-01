using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Path_Active: GridBlockItem
{
    public Enums.PathDirection PathDirection
    {
        get; private set;
    }

    public Enums.PathDirection NextDirection
    {
        get; private set;
    }

    public bool Show { 
        get { return _sR.enabled; } 
        set { _sR.enabled = _animator.enabled = value; } 
    }

    public void UpdatePathState(Vector2 cDir, Vector2? nDir)
    {
        ResetRotation();
        PathDirection = cDir.ToPathDirection();
        NextDirection = (nDir ??  new Vector2(0,0)).ToPathDirection();

        Show = true;

        _animator.SetBool("Start", false);
        _animator.SetBool("Head", false);
        _animator.SetBool("Straight", false);
        _animator.SetBool("Curve", false);

        if (PathDirection == Enums.PathDirection.Start)
        {
            _animator.SetBool("Start", true);
            switch (NextDirection)
            {
                case Enums.PathDirection.Up:
                    transform.Rotate(0, 0, 90);
                    break;
                case Enums.PathDirection.Down:
                    transform.Rotate(0, 0, -90);
                    break;
                case Enums.PathDirection.Left:
                    transform.Rotate(0, 0, 180);
                    break;
                default:
                    break;
            }
        }
        else if (nDir == null)
        {
            _animator.SetBool("Head", true);
            switch (PathDirection)
            {
                case Enums.PathDirection.Up:
                    transform.Rotate(0, 0, 90);
                    break;
                case Enums.PathDirection.Down:
                    transform.Rotate(0, 0, -90);
                    break;
                case Enums.PathDirection.Left:
                    transform.Rotate(0, 0, 180);
                    break;
            }
        }
        else if (PathDirection == NextDirection)
        {
            _animator.SetBool("Straight", true);
            if (PathDirection == Enums.PathDirection.Up || NextDirection == Enums.PathDirection.Down)
                transform.Rotate(0, 0, 90);
        }
        else
        {
            _animator.SetBool("Curve", true);
            if ((PathDirection == Enums.PathDirection.Right && NextDirection == Enums.PathDirection.Up) ||
               (PathDirection == Enums.PathDirection.Down && NextDirection == Enums.PathDirection.Left))
            {
                _sR.flipY = true;
            }
            else if ((PathDirection == Enums.PathDirection.Up && NextDirection == Enums.PathDirection.Right) ||
                    (PathDirection == Enums.PathDirection.Left && NextDirection == Enums.PathDirection.Down))
            {
                _sR.flipX = true;
            }
            else if ((PathDirection == Enums.PathDirection.Left && NextDirection == Enums.PathDirection.Up) ||
                    (PathDirection == Enums.PathDirection.Down && NextDirection == Enums.PathDirection.Right))
            {
                _sR.flipX = true;
                _sR.flipY = true;
            }
        }
    }

    public void ResetRotation()
    {
        _sR.flipX = false;
        _sR.flipY = false;
        transform.rotation = Quaternion.identity;
    }

    public void Reset()
    {
        _sR.enabled = false;
        _animator.enabled = false;
        ResetRotation();
        PathDirection = Enums.PathDirection.Start;
        if (_animator != null)
        {
            _animator.SetBool("Head", false);
            _animator.SetBool("Straight", false);
            _animator.SetBool("Curve", false);
            _animator.SetBool("Start", false);
        }
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }

    private Animator _animator;
    private SpriteRenderer _sR;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sR = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
    }
}
