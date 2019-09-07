using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space : MonoBehaviour
{
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

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public bool SpaceActive
    {
        get { return gameObject.activeSelf; }
    }

    public GridBlock ParentGridBlock
    {
        get; set;
    }

    //public Vector2 GridPosition
    //{
    //    get
    //    {
    //        return _gridPos;
    //    }
    //    set
    //    {
    //        _gridPos = value;
    //        _pM.UpdatePathMatrix(Player, value, this);
    //    }
    //}

    protected SpriteRenderer _sR;
    protected Enums.Player _player;
    protected PlayerManager _pM;

    Vector2 _gridPos;

    void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
    }

    virtual protected void Update()
    {
        var grid = _pM.GetPathMatrix(Player);
        bool inGrid = false;

        if (grid != null)
        {
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                if (inGrid)
                    break;
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    if (grid[x, y] == ParentGridBlock)
                    {
                        inGrid = true;
                        break;
                    }
                }
            }
        }

        if(!inGrid)
            gameObject.SetActive(false);

        if (_pM.GetDeleteMoveSpace(Player))
            gameObject.SetActive(false);
    }
}
