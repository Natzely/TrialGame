using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CursorPanel : MonoBehaviour
{
    public Enums.CursorMenuState ActiveState;
    public AudioClip Sound_Selected;
    public bool Select
    {
        get { return _select; }
        set { SelectPanel(_select = value); }
    }

    [SerializeField] private CursorMenuManager Manager;
    [SerializeField] private GameObject Background;
    [SerializeField] private GameObject Text;
    [SerializeField] private float AnimationSpeed;
    [SerializeField] private float MoveDistance;

    
    
    public bool Showing { get { return _showing; } }

    private RectTransform _rectTrans;
    private RectTransform _backgroundTrans;
    private AudioSource _audioSource;
    private Vector2 _baseSize;
    private Vector2 _hideSize;
    private Vector2 _goalSize;
    private Vector2 _basePos;
    private Vector2 _selPos;
    private Vector2 _goalPos;
    private bool _showing;
    private bool _select;

    private void Awake()
    {
        _rectTrans = GetComponent<RectTransform>();
        _backgroundTrans = Background.GetComponent<RectTransform>();
        _audioSource = GetComponent<AudioSource>();
        _baseSize = _rectTrans.sizeDelta;
        _hideSize = new Vector2(_baseSize.x, -.02f);
        _basePos = _rectTrans.anchoredPosition;
        _selPos = new Vector2(_basePos.x + MoveDistance, _basePos.y);
        _goalPos = _basePos;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rectTrans.sizeDelta = _hideSize;
        Show(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager.State.HasFlag(ActiveState) && !_showing)
            Show(true);
        else if (!Manager.State.HasFlag(ActiveState) && _showing)
            Show(false);

        if (_rectTrans.sizeDelta != _goalSize)
        {
            var newSize = Vector2.MoveTowards(_rectTrans.sizeDelta, _goalSize, AnimationSpeed * Time.unscaledDeltaTime);
            //_rectTrans.sizeDelta = _backgroundTrans.sizeDelta = newSize;
            _rectTrans.sizeDelta = newSize;
        }

        if (_backgroundTrans.anchoredPosition != _goalPos)
        {
            var newPos = Vector2.MoveTowards(_backgroundTrans.anchoredPosition, _goalPos, AnimationSpeed / 2 * Time.unscaledDeltaTime);
            _backgroundTrans.anchoredPosition = newPos;
        }
    }

    private void Show(bool show)
    {
        Text.SetActive(show);
        _goalSize = show ? _baseSize : _hideSize;
        _showing = show;
        if (!_showing)
            _goalPos = _basePos;
    }

    private void SelectPanel(bool select)
    {
        _goalPos = select ? _selPos : _basePos;
        if (select)
            _audioSource.Play(Sound_Selected);
    }
}
