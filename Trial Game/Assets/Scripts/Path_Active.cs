using UnityEngine;

public class Path_Active: GridBlockItem
{
    public Enums.PathDirection PathDirection
    {
        get; private set;
    }

    public bool Show { 
        get { return _sR.enabled; } 
        set { _sR.enabled = value; } 
    }

    public void SavePath()
    {
        _animator.SetBool("Save", true);
    }

    public void UpdatePathState(Vector2 cDir, Vector2? nDir)
    {
        ResetRotation();
        PathDirection = cDir.ToPathDirection();
        Enums.PathDirection nextDir = (nDir ??  new Vector2(0,0)).ToPathDirection();

        if (PathDirection == Enums.PathDirection.Start)
        {
            _animator.SetBool("Start", true);
            switch (nextDir)
            {
                case Enums.PathDirection.Down:
                    transform.Rotate(0, 0, -90);
                    break;
                case Enums.PathDirection.Up:
                    transform.Rotate(0, 0, 90);
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
            _animator.SetBool("Straight", false);
            _animator.SetBool("Curve", false);
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
        else if (PathDirection == nextDir)
        {
            _animator.SetBool("Straight", true);
            _animator.SetBool("Head", false);
            _animator.SetBool("Curve", false);
            if (PathDirection == Enums.PathDirection.Up || nextDir == Enums.PathDirection.Down)
                transform.Rotate(0, 0, 90);
        }
        else
        {
            _animator.SetBool("Curve", true);
            _animator.SetBool("Head", false);
            _animator.SetBool("Straight", false);
            if ((PathDirection == Enums.PathDirection.Right && nextDir == Enums.PathDirection.Up) ||
               (PathDirection == Enums.PathDirection.Down && nextDir == Enums.PathDirection.Left))
            {
                _sR.flipY = true;
            }
            else if ((PathDirection == Enums.PathDirection.Up && nextDir == Enums.PathDirection.Right) ||
                    (PathDirection == Enums.PathDirection.Left && nextDir == Enums.PathDirection.Down))
            {
                _sR.flipX = true;
            }
            else if ((PathDirection == Enums.PathDirection.Left && nextDir == Enums.PathDirection.Up) ||
                    (PathDirection == Enums.PathDirection.Down && nextDir == Enums.PathDirection.Right))
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
        ResetRotation();
        PathDirection = Enums.PathDirection.Start;
        _animator?.SetBool("Head", false);
        _animator?.SetBool("Straight", false);
        _animator?.SetBool("Curve", false);
        _animator?.SetBool("Start", false);
        
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
        Reset();
    }
}
