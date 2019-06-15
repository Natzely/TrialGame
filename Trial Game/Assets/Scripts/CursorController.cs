using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Enums.Player Player = Enums.Player.Player1;
    public PolygonCollider2D Boundaries;
    public float MoveTimer = 1;
    public float ActionTimer = .1f;

    UnitController _currUnit;
    SpriteRenderer _sR;
    List<Vector2> _moves;
    Vector2 _startPos;
    Color _playerColor;
    Color _errorColor;
    bool _selecting;
    bool _noGo;
    float _moveTimer;
    float _horzClamp;
    float _vertClamp;
    float _actionTimer;
    string _playerPrefix;

    // Start is called before the first frame update
    void Start()
    {
        _moves = new List<Vector2>();
        _sR = GetComponent<SpriteRenderer>();
        _selecting = false;
        _noGo = false;
        _moveTimer = 0;
        _horzClamp = (int)Boundaries.bounds.extents.x;
        _vertClamp = (int)Boundaries.bounds.extents.y;
        _actionTimer = 0;
        _startPos = transform.position;
        _playerPrefix = gameObject.layer == LayerMask.NameToLayer("Player1") ? "P1" : "P2";
        _errorColor = Colors.Error;

        switch (Player)
        {
            case Enums.Player.Player2:
                _playerColor = _sR.color = Colors.Player2;
                break;
            default:
                _playerColor = _sR.color = Colors.Player1;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int vert = Mathf.RoundToInt(Input.GetAxis(_playerPrefix + "_Vertical"));
        int horz = Mathf.RoundToInt(Input.GetAxis(_playerPrefix + "_Horizontal"));
        bool select = Input.GetButtonUp(_playerPrefix + "_Select") ;
        bool cancel = Input.GetButtonUp(_playerPrefix + "_Cancel");
        bool attack = Input.GetButtonUp(_playerPrefix + "_Attack");

        // Possible next move
        Vector2 tmpPos = transform.position;

        if(cancel)
        {
            if(transform.position.V2() != _startPos)
            {
                transform.position = _startPos;
                _moves.Clear();
                _currUnit.CancelMove();
                _actionTimer = ActionTimer;
            }
            else
            {
                _selecting = false;
                _currUnit.Select(_selecting);
                _actionTimer = ActionTimer;
            }
        }

        if (select && _actionTimer <= 0 && _currUnit != null && _currUnit.Player == Player && !_currUnit.OnCooldown)
        {
            if (transform.position == _currUnit.GetHolder().position)
            {
                if (!_currUnit.Moved)
                {
                    _startPos = transform.position;
                    _moves.Clear();
                }
                _selecting = !_selecting;
                _currUnit.Select(_selecting);
                _actionTimer = ActionTimer;
            }
            else if(_selecting && !_noGo)
            {
                _currUnit.MoveTo(_moves);
                _actionTimer = ActionTimer;
            }   
        }

        if(_selecting && attack && transform.position.V2() != _startPos)
        {
            _currUnit.Attack(transform.position);
            _currUnit.Select(_selecting = false);
            _actionTimer = ActionTimer;
        }

        // Timer for actions (selects and moves)
        if (_actionTimer > 0)
            _actionTimer -= Time.deltaTime;
        else if (_moveTimer <= 0 && (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0))
        {
            tmpPos.x = Mathf.Clamp(tmpPos.x + horz, -_horzClamp, _horzClamp);
            tmpPos.y = Mathf.Clamp(tmpPos.y + vert, -_vertClamp, _vertClamp);

            float distance = Vector2.Distance(tmpPos, _startPos);
            if (!_selecting || (_selecting && distance <= _currUnit.TotalMoves))
            {
                transform.position = tmpPos;
                _moveTimer = MoveTimer;
                if (_selecting && !_noGo)
                {
                    if (_moves.Contains(tmpPos))
                        while ((Vector2)_moves.Pop() != tmpPos) { }

                    LinkAndAddMove(tmpPos);
                }
            }
        }

        // Timer for moves
        if (_moveTimer > 0)
            _moveTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triger Enter");
        GameObject gO = collision.gameObject;
        Debug.Log($"Is water: {gO.layer == LayerMask.NameToLayer("Map")}");
        if (gO.layer == LayerMask.NameToLayer("Map") ||
            (gO.tag == "Unit" && _currUnit != null && gO != _currUnit.gameObject && _selecting))
        {
            if (_selecting)
            {
                _sR.color = _errorColor;
                _noGo = true;
                if (_moves.Count > 0)
                    _moves.Pop();
            }
        }
        else
        {
            if (!_selecting)
            {
                UnitController tmpUnit = gO.GetComponent<UnitController>();
                if (tmpUnit != null && tmpUnit.Player == Player)
                {
                    _currUnit = tmpUnit;
                    _currUnit.Hover(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject gO = collision.gameObject;
        if (gO.layer == LayerMask.NameToLayer("Map") || 
            (gO.tag == "Unit" && _selecting))
        {
            _sR.color = _playerColor;
            _noGo = false;
        }
        else if(_currUnit != null)
        {
            _currUnit.Hover(false);
        }
    }

    private void LinkAndAddMove(Vector2 move)
    {
        _moves.Add(move);
    }
}
