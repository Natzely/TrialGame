using UnityEngine;
using UnityEngine.EventSystems;

public class PauseScreen_Button : UIButton
{
    public Enums.UI_PauseButtonType Type;

    public override void OnSelect(BaseEventData eventData)
    {
        _selected = true;
        _image.color = Colors.Button_Selected;
        _handler.OnItemSelected(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        _image.color = Colors.Button_Deselected;
        Deselect();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (_handler.CurrentButton != this.gameObject)
            _audioSource.Play(Sound_Select);
        _button.Select();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        _handler.HandleButtonSubmit(this);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        _handler.HandleButtonSubmit(this);
    }
}
