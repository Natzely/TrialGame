using UnityEngine;

public class GridSpace : GridBlockItem
{
    public float MoveAnimationSpeed;

    private bool _activate;

    public Vector2 Position
    {
        get { return new Vector2(transform.position.x, transform.position.y); }
    }

    public virtual Enums.Player Player
    {
        get { return _player; }
        set { _player = value; }
    }

    public bool Show
    {
        get { return _sR.enabled; }
        set { _sR.enabled = value; }
    }

    public void Enable(Vector2? moveFrom)
    {
        if (!gameObject.activeSelf)
        {
            if (moveFrom != null)
            {
                var newT = transform.position.Copy();

                newT.x = (transform.position.x + moveFrom.Value.x) / 2;
                newT.y = (transform.position.y + moveFrom.Value.y) / 2;
                transform.position = newT;
                _activate = true;
            }

            gameObject.SetActive(true);
        }
    }

    protected SpriteRenderer _sR;
    protected Enums.Player _player;
    protected PlayerManager _pM;

    Vector2 _gridPos;

    void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
    }

    private void Update()
    {
        if (_activate)
        {
            Vector2 moveVector = Vector2.MoveTowards(transform.position, ParentGridBlock.transform.position, MoveAnimationSpeed * Time.deltaTime);
            transform.position = moveVector;

            if (transform.position == ParentGridBlock.transform.position)
            {
                _activate = false;
            }
        }
    }
}
