using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor_Move : MonoBehaviour
{
    [SerializeField] private CursorController Controller;
    [SerializeField] private PolygonCollider2D Boundaries;
    [SerializeField] private AudioClip MoveSound;
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AttackResults AttackResults;
    [SerializeField] private float ActionTime;
    [SerializeField] private float HoldTime;
    [SerializeField] private float HoldActionTime;

    private Vector2 Position { 
        get { return transform.position.V2(); }
        set { transform.position = value; }
    }

    private LevelManager LevelManager { get { return (LevelManager)SceneManager.Instance; } }

    private Vector2 _minClamp;
    private Vector2 _maxClamp;
    private Vector2Int _moveDir;
    private float _currentTimer;
    private float _actionTimer;
    private float _holdTimer;
    private bool _timeStopped;

    // Start is called before the first frame update
    void Start()
    {
        _minClamp = new Vector2(Boundaries.points[1].x + .5f, Boundaries.points[1].y + .5f);
        _maxClamp = new Vector2(Boundaries.points[3].x - .5f, Boundaries.points[3].y - .5f);
        _currentTimer = ActionTime;
    }

    private void Update()
    {
        // If the movement key is being press
        if (_moveDir != Vector2.zero)
        {
            // Increase hold timer
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= HoldTime) // If the key has been pressed for long enough
            {
                Debug.Log("Held");
                _currentTimer = HoldActionTime; // Switch to the holdtimer to move the cursor faster
            }

            //and the actionTimer reached zero, move the cursor;
            if (_actionTimer <= 0)
            {
                var tmpPos = Position + _moveDir;
                tmpPos = tmpPos.Clamp(_minClamp, _maxClamp);
                if (tmpPos != Position)
                {
                    Position = tmpPos;
                    if (LevelManager.GameState == Enums.GameState.TimeStop)
                    {
                        var neighbor = Controller.CurrentGridBlock.Neighbors[_moveDir];
                        Controller.CurrentGridBlock = neighbor;
                    }
                    Controller.UpdateMinimapIcon();
                    AudioSource.Play(MoveSound);
                }

                _actionTimer = _currentTimer;
            }
        }

        _actionTimer -= Time.deltaTime;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)// && Controller.CursorState != Enums.CursorState.CursorMenu)
        {
            AttackResults.Show(false);
            var tmpPos = context.ReadValue<Vector2>();
            _moveDir = Vector2Int.RoundToInt(tmpPos);
            if(Controller.CursorState == Enums.CursorState.CursorMenu)
            {
                Controller.CursorMenu.SelectNextAvailablePanel(_moveDir.y);
                _moveDir = Vector2Int.zero;
            }
        }
        else if (context.canceled)
        {
            _moveDir = Vector2Int.zero;
            _currentTimer = ActionTime;
            _holdTimer = 0;
        }
    }
}
