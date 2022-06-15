using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UITextSizeEditor))]
public class ConfirmPanelButton : UIButton, ICancelHandler
{
    public Enums.UI_ConfirmButtonType Type;
    [SerializeField] private UITextSizeEditor SizeEditor;

    public override void OnSelect(BaseEventData eventData)
    {
        Log($"{gameObject.name}: Selected");
        AudioSource.Play(Sound_Select);
        SizeEditor.Edit(true);
        _selected = true;
        UIHandler.OnItemSelected(this);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Log($"{gameObject.name}: Deselected");
        SizeEditor.Edit(false);
        //_image.color = Colors.Button_Deselected;
        Deselect();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Log($"{gameObject.name}: Entered");
        if (UIHandler.CurrentButton != this.gameObject)
            Button.Select();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        Log($"{gameObject.name}: Clicked");
        Press();
        UIHandler.HandleButtonSubmit(this);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        Log($"{gameObject.name}: Submit");
        Press();
        UIHandler.HandleButtonSubmit(this);
    }

    public void OnCancel(BaseEventData eventData)
    {
        AudioSource.Play(Sound_Cancel);
        ((SideSelectionAH)UIHandler).OnCancel(eventData);
    }
}
