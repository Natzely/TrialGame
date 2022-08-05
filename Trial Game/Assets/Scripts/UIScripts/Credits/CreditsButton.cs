using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreditsButton : UIButton
{
    public override void OnDeselect(BaseEventData eventData)
    {
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
    }

    public override void OnSelect(BaseEventData eventData)
    {
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        UIHandler.HandleButtonSubmit(this);
    }
}
