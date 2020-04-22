using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpace : MonoBehaviour
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

    protected SpriteRenderer _sR;
    protected Enums.Player _player;
    protected PlayerManager _pM;

    Vector2 _gridPos;

    void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
        _pM = FindObjectOfType<PlayerManager>();
    }
}
