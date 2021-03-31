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
    public TextMeshProUGUI ButtonText;

    [SerializeField] private AudioClip SoundMove;

    private AudioSource _aS;
    private PauseScreen _pauseScreen;
    private Button _button;
    private Image _image;
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
        _aS.Play(SoundMove);
        if (move.x != 0)
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
    }

    public override void Awake()
    {
        base.Awake();
        _pauseScreen = GetComponentInParent<PauseScreen>();
        _button = GetComponent<Button>();
        _aS = GetComponent<AudioSource>();
        _image = GetComponent<Image>();
        _savedFontSize = MINTEXTSIZE;
    }

    // Start is called before the first frame update
    void Start()
    {

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
        if (_pauseScreen.CurrentButton != this.gameObject)
            _aS.Play(SoundMove);
        _button.Select();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DebugLog($"{gameObject.name} OnPointerClick");
        _pauseScreen.OnMainButtonClick(this);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        DebugLog($"{gameObject.name} OnSubmit");
        _pauseScreen.OnMainButtonClick(this);
    }

    public void OnSelect(BaseEventData eventData)
    {
        DebugLog($"{gameObject.name} OnSelect");
        Selected();
        _image.color = Colors.Button_Selected;
        _pauseScreen.OnItemSelected(this);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DebugLog($"{gameObject.name} OnDeselect");
        _image.color = Colors.Button_Deselected;
        Deselect();
    }

    public void OnMove(AxisEventData eventData)
    {
        DebugLog($"{gameObject.name} OnMove to {eventData.selectedObject.name}");
    }
}
