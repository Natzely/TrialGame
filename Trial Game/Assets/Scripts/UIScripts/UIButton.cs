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

    [SerializeField] internal TextMeshProUGUI ButtonText;
    [SerializeField] internal AudioClip Sound_Select;

    internal const int MINTEXTSIZE = 50;
    internal const int MAXTEXTSIZE = 55;
    internal const float TEXTSIZESPEED = 100f;

    internal UIActionHandler UIHandler { private set; get; }
    internal AudioSource AudioSource { private set; get; }
    internal Button Button { private set; get; }
    internal Image Image { private set; get; }

    internal bool _selected;
    internal float _savedFontSize;

    public abstract void OnDeselect(BaseEventData eventData);
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void OnPointerEnter(PointerEventData eventData);
    public abstract void OnSelect(BaseEventData eventData);
    public abstract void OnSubmit(BaseEventData eventData);

    private bool _silent;

    public void MoveToNextUIObject(Vector3 move)
    {
        DebugLog($"{gameObject.name} move to next UI object");
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



    public void Deselect()
    {
        ButtonText.fontSize = _savedFontSize = MINTEXTSIZE;
        _selected = false;
    }

    public void RaiseVolume()
    {
        AudioSource.volume = 1;
    }

    public override void Awake()
    {
        base.Awake();
        Button = GetComponent<Button>();
        AudioSource = GetComponent<AudioSource>();
        Image = GetComponent<Image>();
        UIHandler = FindObjectOfType<UIActionHandler>();
        _savedFontSize = MINTEXTSIZE;
    }

    // Update is called once per frame
    void Update()
    {
        if (_selected && ButtonText != null)
        {
            if (ButtonText.fontSize < MAXTEXTSIZE)
            {
                _savedFontSize += TEXTSIZESPEED * Time.unscaledDeltaTime;
                ButtonText.fontSize = Mathf.Clamp(_savedFontSize, MINTEXTSIZE, MAXTEXTSIZE);
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
