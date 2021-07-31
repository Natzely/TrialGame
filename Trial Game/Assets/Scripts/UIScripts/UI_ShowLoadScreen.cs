using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ShowLoadScreen : UIObject
{
    [SerializeField] private float ExpandSpeed;
    [SerializeField] private UI_PanelFade Panel;

    public bool Show
    {
        get { return _show; }
        set
        {
            _show = value;
            if (!_show)
                _rectTransform.sizeDelta = new Vector2(_orgSize.x, 0);
        }
    }

    private RectTransform _rectTransform;
    private Vector2 _orgSize;
    private bool _show;

    public override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _orgSize = _rectTransform.sizeDelta;
        Show = false;
    }

    void Update()
    {
        if(_show)
        {
            Vector2 newSize = Vector2.MoveTowards(_rectTransform.sizeDelta, _orgSize, Time.deltaTime * ExpandSpeed);
            _rectTransform.sizeDelta = newSize;
            if (newSize == _orgSize)
            {
                _show = false;
                Panel.StartFade();
            }
        }
    }
}