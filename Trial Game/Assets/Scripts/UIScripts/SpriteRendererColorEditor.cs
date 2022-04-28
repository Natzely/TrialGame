using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRendererColorEditor : UIObjectEditor
{

    [SerializeField] private SpriteRenderer SpriteRender;
    [SerializeField] private Color ColorEdit;

    private Color _orgColor;
    private Color _targetColor;
    private float _colorSpeed;

    public override void Edit(bool edit = true)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    private void Awake()
    {
        _orgColor = SpriteRender.color;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _targetColor = ColorEdit;
    }

    protected override void Update()
    {
        base.Update();
        if (_edit)
            EditObject();
    }

    protected override void EditObject()
    {
        base.EditObject();
        var newColor = Vector4.Lerp(SpriteRender.color, _targetColor, _colorSpeed +=(Speed * Time.deltaTime));
        SpriteRender.color = newColor;

        if(Loop && SpriteRender.color == _targetColor)
        {
            if (_targetColor == ColorEdit)
                _targetColor = _orgColor;
            else
                _targetColor = ColorEdit;
            _colorSpeed = 0;
        }
    }

    protected override void Reset()
    {
        base.Reset();
        SpriteRender.color = _orgColor;
        _targetColor = ColorEdit;
        _colorSpeed = 0;
    }
}
