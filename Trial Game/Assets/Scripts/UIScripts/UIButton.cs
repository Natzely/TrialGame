using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public abstract class UIButton : UIUnityObject, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
{
    public float ClipLength { get { return _audioSource.clip.length; } }

    [SerializeField] internal TextMeshProUGUI ButtonText;
    [SerializeField] internal AudioClip Sound_Select;

    internal const int MINTEXTSIZE = 50;
    internal const int MAXTEXTSIZE = 55;
    internal const float TEXTSIZESPEED = 100f;

    internal UIActionHandler _handler;
    internal AudioSource _audioSource;
    internal Button _button;
    internal Image _image;
    internal bool _selected;
    internal float _savedFontSize;

    public abstract void OnDeselect(BaseEventData eventData);
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnSelect(BaseEventData eventData);
    public abstract void OnSubmit(BaseEventData eventData);

    public void MoveToNextUIObject(Vector3 move)
    {
        DebugLog($"{gameObject.name} move to next UI object");
        _audioSource.Play(Sound_Select);
        if (move.x != 0)
        {
            if (move.x > 0)
                _button.navigation.selectOnLeft?.Select();
            else
                _button.navigation.selectOnRight?.Select();
        }
        else if (move.y != 0)
        {
            if (move.y > 0)
                _button.navigation.selectOnUp?.Select();
            else
                _button.navigation.selectOnDown?.Select();
        }
    }

    public void Select()
    {
        _button.Select();
    }

    public void Deselect()
    {
        ButtonText.fontSize = _savedFontSize = MINTEXTSIZE;
        _selected = false;
    }

    public void RaiseVolume()
    {
        _audioSource.volume = 1;
    }

    public override void Awake()
    {
        base.Awake();
        _button = GetComponent<Button>();
        _audioSource = GetComponent<AudioSource>();
        _image = GetComponent<Image>();
        _handler = FindObjectOfType<UIActionHandler>();
        _savedFontSize = MINTEXTSIZE;
    }

    // Update is called once per frame
    void Update()
    {
        if (_selected && ButtonText != null)
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
}
