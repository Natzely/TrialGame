using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Cursor_Move : MonoBehaviour
{
    [SerializeField] private CursorController Controller;
    [SerializeField] private PolygonCollider2D Boundaries;
    [SerializeField] private AudioClip MoveSound;
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private float ActionTime;
    [SerializeField] private float HoldTime;
    [SerializeField] private float HoldActionTime;

    private Vector2 Position { 
        get { return transform.position.V2(); }
        set { transform.position = value; }
    }

    private Vector2 _minClamp;
    private Vector2 _maxClamp;
    private Vector2Int _moveDir;
    private float _currentTimer;
    private float _actionTimer;
    private float _holdTimer;

    // Start is called before the first frame update
    void Start()
    {
        _minClamp = new Vector2(Boundaries.points[1].x + .5f, Boundaries.points[1].y + .5f);
        _maxClamp = new Vector2(Boundaries.points[3].x - .5f, Boundaries.points[3].y - .5f);
        _currentTimer = ActionTime;
    }

    private void Update()
    {
        if (_moveDir != Vector2.zero && _actionTimer <= 0)
        {
            var tmpPos = Position + _moveDir;
            tmpPos = tmpPos.Clamp(_minClamp, _maxClamp);
            if (tmpPos != Position)
            {
                Position = tmpPos;
                Controller.UpdateMinimapIcon();
                AudioSource.Play(MoveSound);
            }

            _holdTimer += Time.deltaTime;
            if (_holdTimer >= HoldTime)
                _currentTimer = HoldActionTime;

            _actionTimer = _currentTimer;
        }

        _actionTimer -= Time.deltaTime;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed && Controller.CursorState != Enums.CursorState.CursorMenu)
        {
            var tmpPos = context.ReadValue<Vector2>();
            _moveDir = Vector2Int.RoundToInt(tmpPos);
        }
        else if (context.canceled)
        {
            _moveDir = Vector2Int.zero;
            _currentTimer = ActionTime;
            _holdTimer = 0;
        }
    }
}
