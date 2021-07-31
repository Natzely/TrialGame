using UnityEngine;
using UnityEngine.EventSystems;

public class TitleScreen_MainButton : TitleScreen_Button
{
    public override void OnSelect(BaseEventData eventData)
    {
        AudioSource.Play(Sound_Select);
        _selected = true;
        UIHandler.OnItemSelected(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        //_image.color = Colors.Button_Deselected;
        Deselect();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (UIHandler.CurrentButton != this.gameObject)
        Button.Select();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        UIHandler.HandleButtonSubmit(this);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        UIHandler.HandleButtonSubmit(this);
    }
}
