using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space : MonoBehaviour
{
    public Vector2 Position
    {
        get { return new Vector2(transform.position.x - .5f, transform.position.y - .5f); }
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

    public bool Enabled
    {
        get { return gameObject.activeSelf; }
    }

    public Vector2 GridPosition
    {
        get
        {
            return _gridPos;
        }
        set
        {
            _gridPos = value;
            _pM.UpdatePathMatrix(Player, value, this);
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

    virtual protected void Update()
    {
        if (_pM.GetDeleteMoveSpace(Player))
            gameObject.SetActive(false);

    }
}
