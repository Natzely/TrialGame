using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class UIImageEditor : UIObjectEditor
{
    [SerializeField] private Color ColorEdit = Color.white;
    [SerializeField] private float EffectTime;

    private Image _image;
    private Color _startColor;
    private float _timer;
    private bool _loop;

    public override void Edit(bool edit = true)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _startColor = _image.color;
        _timer = 0;
        _loop = Loop;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(_edit)
        {
            EditObject();
        }
    }

    protected override void EditObject()
    {
        base.EditObject();

        float lerpPer = _timer / EffectTime;
        Color newColor = Color.Lerp(_startColor, ColorEdit, lerpPer);
        _image.color = newColor;

        _timer += Time.deltaTime;

        if (_image.color == ColorEdit && _loop)
        {
            Color tmp = ColorEdit;
            ColorEdit = _startColor;
            _startColor = tmp;
            _timer = 0;
            _loop = false;
        }
        else if (_image.color == ColorEdit && !_loop)
        {
            base.Edit(false);
            if (EditEvent != null)
                EditEvent.Invoke();
        }
        
    }

    protected override void Reset()
    {
        base.Reset();
        _image.color = _startColor;
    }
}
