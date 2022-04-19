using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine;
using System.Reflection;

public class SideSelectionAH : UIActionHandler, IMoveHandler, ICancelHandler, ILog
{
    [SerializeField] private LevelManager LevelManager;
    [SerializeField] private InputSystemUIInputModule InputSystem;
    [SerializeField] private RectMask2D AztecMask;
    [SerializeField] private RectMask2D SpanishMask;
    [SerializeField] private UIButton CancelButton;
    [SerializeField] private float MoveAcceleration;
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float MaxMoveSpeed;
    [SerializeField] private float MoveBackSpeed;

    public Enums.PlayerSides ConfirmedSide { get { return _selectedMask.PlayerSides; } }

    private UICanvasGroupEditor _cgEditor;
    private RectTransform _aztecTrans;
    private RectTransform _spanishTrans;
    private SideSelectionMask _selectedMask;
    private bool _sideSelected;
    private float _sideMove;
    private float _moveSpeed;
    private float _holdTimer;
    private float _startWidth;
    private float _maxWidth;
    private float _maxPadding;
    private float _maxSoftness;

    public override void HandleButtonSubmit(UIButton button)
    {
        Log($"{gameObject.name}: HandleButtonSubmit");
        var sideSelButton = (TitleScreen_Button)button;

        switch (sideSelButton.Type)
        {
            case Enums.UI_TitleButtonType.Quit:
                Log("\tQuit");
                OnCancel(null);
                break;
            case Enums.UI_TitleButtonType.Start:
                Log("\tStart");
                _selectedMask.Confirmed = true;
                _cgEditor.Edit(true);
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _cgEditor = GetComponent<UICanvasGroupEditor>();
        _moveSpeed = MoveSpeed;
        _aztecTrans = AztecMask.rectTransform;
        _spanishTrans = SpanishMask.rectTransform;
        _startWidth = _aztecTrans.rect.width;
        _maxWidth = _startWidth * 2;
        _maxPadding = AztecMask.padding.y * 2;
        _maxSoftness = AztecMask.softness.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(_sideMove != 0)
        {
            MoveSpeed = Mathf.Clamp(MoveSpeed * MoveAcceleration, 0, MaxMoveSpeed);
            EditMask(AztecMask,   _sideMove < 0 ? _maxWidth : 0, MoveSpeed);
            EditMask(SpanishMask, _sideMove < 0 ? 0 : _maxWidth, MoveSpeed);
            _holdTimer += Time.deltaTime;

            if(_spanishTrans.sizeDelta.x == 0 || _aztecTrans.sizeDelta.x == 0)
            {
                _sideSelected = true;
                HighlightMask(_spanishTrans.sizeDelta.x == 0 ? AztecMask : SpanishMask);
            }
            if (_holdTimer > .1)
            {
                _sideMove = 0;
                MoveSpeed = _moveSpeed;
            }
        }
        else if(!_sideSelected && _aztecTrans.rect.width != _startWidth)
        {
            EditMask(AztecMask, _startWidth, MoveBackSpeed);
            EditMask(SpanishMask, _startWidth, MoveBackSpeed);
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        _sideMove = eventData.moveVector.x;
        _holdTimer = 0;
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (_selectedMask)
        {
            _selectedMask.Selected = false;
            _sideSelected = false;
            InputSystem.moveRepeatDelay = 0;
            InputSystem.moveRepeatRate = 0;
            _eventSystem.SetSelectedGameObject(this.gameObject);
            _selectedMask = null;
        }
        
    }

    private void EditMask(RectMask2D mask, float moveTowards, float moveSpeed)
    {
        float newX = Mathf.MoveTowards(mask.rectTransform.rect.width, moveTowards, moveSpeed * Time.deltaTime);
        mask.rectTransform.sizeDelta = new Vector2(newX, mask.rectTransform.rect.height);
        float newPadding = _maxPadding * (1 - newX / _maxWidth); // This should be the reverse, the bigger the flag, the smaller the padding and softness
        float newSoftness = _maxSoftness * (1 - newX / _maxWidth);
        mask.padding = new Vector4(0, newPadding, 0, newPadding);
        mask.softness = new Vector2Int(0, (int)newSoftness);
    }

    private void HighlightMask(RectMask2D mask)
    {
        _selectedMask = mask.GetComponent<SideSelectionMask>();
        _selectedMask.Selected = true;
        InputSystem.moveRepeatDelay = 2;
        InputSystem.moveRepeatRate = .3f;
        CancelButton.Select(true);
    }

    public void Log(string msg)
    {
        DebugLogger.Instance.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new System.NotImplementedException();
    }
}
