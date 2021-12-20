using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class SideSelectionAH : UIActionHandler, IMoveHandler, ICancelHandler, ISubmitHandler
{
    public RectMask2D AztecMask;
    public RectMask2D SpanishMask;
    public Selectable CancelButton;
    public float MoveAcceleration;
    public float MoveSpeed;
    public float MaxMoveSpeed;
    public float MoveBackSpeed;

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
        var sideSelButton = (TitleScreen_Button)button;

        switch (sideSelButton.Type)
        {
            case Enums.UI_TitleButtonType.Quit:
                OnCancel(null);
                break;
            case Enums.UI_TitleButtonType.Start:
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
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
            if (_holdTimer > .05)
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

    public void OnSubmit(BaseEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnCancel(BaseEventData eventData)
    {
        _selectedMask.Selected = false;
        _sideSelected = false;
        _eventSystem.SetSelectedGameObject(this.gameObject);
        
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
        CancelButton.Select();
    }
}
