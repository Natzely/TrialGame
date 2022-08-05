using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public abstract class UIButton : UIObject, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
{
    public float ClipLength { get { return AudioSource.clip.length; } }
    public bool SilentSelect { get; set; }

    [SerializeField] protected TextMeshProUGUI ButtonText;
    [SerializeField] protected bool OverrideConstTextSizes;
    [SerializeField] protected AudioClip Sound_Select;
    [SerializeField] protected AudioClip Sound_Press;
    [SerializeField] protected AudioClip Sound_Cancel;

    protected const int MINTEXTSIZE = 50;
    protected const int MAXTEXTSIZE = 55;
    protected const float TEXTSIZESPEED = 100f;

    protected UIActionHandler UIHandler { private set; get; }
    protected AudioSource AudioSource { private set; get; }
    protected Button Button { private set; get; }
    protected Image Image { private set; get; }

    protected bool _selected;
    protected float _savedFontSize;

    public abstract void OnDeselect(BaseEventData eventData);
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnSelect(BaseEventData eventData);
    public abstract void OnSubmit(BaseEventData eventData);

    private bool _silent;
    private float _maxFontSize;
    private float _minFontSize;

    public void MoveToNextUIObject(Vector3 move)
    {
        Log($"{gameObject.name} move to next UI object");
        AudioSource.Play(Sound_Select);
        if (move.x != 0)
        {
            if (move.x > 0 && Button.navigation.selectOnLeft)
                Button.navigation.selectOnLeft.Select();
            else if(Button.navigation.selectOnRight)
                Button.navigation.selectOnRight.Select();
        }
        else if (move.y != 0)
        {
            if (move.y > 0 && Button.navigation.selectOnUp)
                Button.navigation.selectOnUp.Select();
            else if(Button.navigation.selectOnDown)
                Button.navigation.selectOnDown.Select();
        }
    }

    public void Select(bool silent = false)
    {
        _silent = silent;
        if (silent)
            AudioSource.volume = 0;
        Button.Select();
    }

    internal void Press(bool silent = false)
    {
        if (!silent && Sound_Press)
            AudioSource.Play(Sound_Press);
    }

    public void Deselect()
    {
        if (ButtonText)
            ButtonText.fontSize = _savedFontSize = _minFontSize;
        _selected = false;
    }

    public void RaiseVolume()
    {
        AudioSource.volume = 1;
    }

    protected virtual void Awake()
    {
        Button = GetComponent<Button>();
        AudioSource = GetComponent<AudioSource>();
        Image = GetComponent<Image>();
        UIHandler = GetComponentInParent<UIActionHandler>();
        _maxFontSize = _savedFontSize = OverrideConstTextSizes ? ButtonText.fontSize + 5 : MAXTEXTSIZE;
        _minFontSize = OverrideConstTextSizes ? ButtonText.fontSize : MINTEXTSIZE;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_selected && ButtonText != null)
        {
            if (ButtonText.fontSize < _maxFontSize)
            {
                _savedFontSize += TEXTSIZESPEED * Time.unscaledDeltaTime;
                ButtonText.fontSize = Mathf.Clamp(_savedFontSize, _minFontSize, _maxFontSize);
            }
            else
                _selected = false;
        }
        
        if(_silent && !AudioSource.isPlaying)
        {
            _silent = false;
            AudioSource.volume = 1;
        }
    }
}
