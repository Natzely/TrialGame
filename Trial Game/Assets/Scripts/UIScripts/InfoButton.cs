using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfoButton : UIButton
{
    public Sprite TutorialImage;
    public List<string> LanguageTexts;

    public override void OnDeselect(BaseEventData eventData)
    {
        Deselect();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
    }

    public override void OnSelect(BaseEventData eventData)
    {
        Select();
        UIHandler.OnItemSelected(this);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
    }
}
