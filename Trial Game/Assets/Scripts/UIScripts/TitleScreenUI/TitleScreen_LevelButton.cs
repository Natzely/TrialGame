using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleScreen_LevelButton : TitleScreen_Button
{
    public string LevelName;
    
    [SerializeField] [Range(0, 1)] private float SelectedOpacity;


    public override void OnSelect(BaseEventData eventData)
    {
        AudioSource.Play(Sound_Select);
        Image.color = new Color(1, 1, 1, SelectedOpacity);
        _selected = true;
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        Image.color = new Color(1, 1, 1, 0);
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
