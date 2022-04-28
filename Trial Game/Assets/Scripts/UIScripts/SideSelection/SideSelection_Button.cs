using UnityEngine;
using UnityEngine.EventSystems;

public class SideSelection_Button : UIButton
{
    public Enums.UI_SideSelectionButtonType Type;

    public override void OnSelect(BaseEventData eventData)
    {
        AudioSource.Play(Sound_Select);
        _selected = true;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        //_image.color = Colors.Button_Deselected;
        Deselect();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
            Button.Select();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {

    }

    public override void OnSubmit(BaseEventData eventData)
    {

    }
}
