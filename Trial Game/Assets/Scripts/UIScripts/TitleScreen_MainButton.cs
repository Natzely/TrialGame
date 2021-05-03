using UnityEngine;
using UnityEngine.EventSystems;

public class TitleScreen_MainButton : TitleScreen_Button
{
    public override void OnSelect(BaseEventData eventData)
    {
        _audioSource.Play(Sound_Select);
        _selected = true;
        _handler.OnItemSelected(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        //_image.color = Colors.Button_Deselected;
        Deselect();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (_handler.CurrentButton != this.gameObject)
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
