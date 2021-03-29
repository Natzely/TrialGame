using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PauseScreen_MainButton : UIUnityObject, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerClickHandler, IMoveHandler, ISubmitHandler
{
    const int MINTEXTSIZE = 50;
    const int MAXTEXTSIZE = 55;
    const float TEXTSIZESPEED = 100f;

    public Enums.UI_PauseButtonType ButtonType;
    public PauseScreen PauseScreen;
    public TextMeshProUGUI ButtonText;

    private Button _button;
    private bool _selected;
    private float _savedFontSize;

    public void Selected()
    {
        _selected = true;
    }

    public void Deselect()
    {
        ButtonText.fontSize = _savedFontSize = MINTEXTSIZE;
        _selected = false;
    }

    public void MoveToNextUIObject(Vector3 move)
    {
        DebugLog($"{gameObject.name} move to next UI object");
        if(move.x != 0)
        {
            if (move.x > 0)
                _button.navigation.selectOnLeft?.Select();
            else
                _button.navigation.selectOnRight?.Select();
        }
        else if(move.y != 0)
        {
            if (move.y > 0)
                _button.navigation.selectOnUp?.Select();
            else
                _button.navigation.selectOnDown?.Select();
        }
    }

    public void Submit()
    {
        DebugLog($"{gameObject.name} Submit");
        PauseScreen.OnMainButtonClick(this);
    }

    public override void Awake()
    {
        base.Awake();
        _button = GetComponent<Button>();
        _savedFontSize = MINTEXTSIZE;
    }

    // Start is called before the first frame update
    void Start()
    {
        PauseScreen.AddButtonToList(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(_selected)
        {
            if (ButtonText.fontSize < MAXTEXTSIZE)
            {
                _savedFontSize = _savedFontSize + TEXTSIZESPEED * Time.fixedDeltaTime;
                ButtonText.fontSize = Mathf.Clamp(_savedFontSize, MINTEXTSIZE, MAXTEXTSIZE);
            }
            else
                _selected = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        DebugLog($"{gameObject.name} Pointer Enter");
        _button.Select();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DebugLog($"{gameObject.name} OnPointerClick");
        PauseScreen.OnMainButtonClick(this);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        DebugLog($"{gameObject.name} OnSubmit");
        PauseScreen.OnMainButtonClick(this);
    }

    public void OnSelect(BaseEventData eventData)
    {
        DebugLog($"{gameObject.name} OnSelect");
        Selected();
        PauseScreen.OnItemSelected(this);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DebugLog($"{gameObject.name} OnDeselect");
        Deselect();
    }

    public void OnMove(AxisEventData eventData)
    {
        DebugLog($"{gameObject.name} OnMove to {eventData.selectedObject.name}");
        //selectable.Select();
    }
}
