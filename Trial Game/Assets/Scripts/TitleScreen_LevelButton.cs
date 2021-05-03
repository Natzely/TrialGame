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
        _audioSource.Play(Sound_Select);
        _image.color = new Color(1, 1, 1, SelectedOpacity);
        _selected = true;
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        _image.color = new Color(1, 1, 1, 0);
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
