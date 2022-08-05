using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UITextSizeEditor))]
public class TitleScreen_MainButton : TitleScreen_Button
{
    [SerializeField] internal UITextSizeEditor SizeEditor;

    public override void OnSelect(BaseEventData eventData)
    {
        Log($"{gameObject.name}: Selected");
        AudioSource.Play(Sound_Select);
        _selected = true;
        //SizeEditor.Edit(true);
        UIHandler.OnItemSelected(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Log($"{gameObject.name}: Deselected");
        //_image.color = Colors.Button_Deselected;
        Deselect();
        //SizeEditor.Edit(false);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Log($"{gameObject.name}: Entered");
        if (UIHandler.CurrentButton != this)
        Button.Select();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Log($"{gameObject.name}: Clicked");
        UIHandler.HandleButtonSubmit(this);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        Log($"{gameObject.name}: Submit");
        UIHandler.HandleButtonSubmit(this);
    }
}
