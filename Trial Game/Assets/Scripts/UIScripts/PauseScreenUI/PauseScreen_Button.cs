using UnityEngine;
using UnityEngine.EventSystems;

public class PauseScreen_Button : UIButton
{
    public Enums.UI_PauseButtonType Type;

    public override void OnSelect(BaseEventData eventData)
    {
        _selected = true;
        Image.color = Colors.Button_Selected;
        AudioSource.Play(Sound_Select);
        UIHandler.OnItemSelected(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Image.color = Colors.Button_Deselected;
        Deselect();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (UIHandler.CurrentButton != this.gameObject)
            AudioSource.Play(Sound_Select);
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
