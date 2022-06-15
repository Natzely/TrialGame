using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorEditor : UIObjectEditor
{
    [SerializeField] private SpriteRenderer Sprite;
    public Color ColorEdit;
    [SerializeField] float TransitionTime;

    Color _orgColor;
    Color _fromColor;
    Color _targetColor;
    float _timeLeft;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        _orgColor = _fromColor = Sprite.color;
        _targetColor = ColorEdit;
        _timeLeft = TransitionTime;
        base.Edit(edit);
    }

    protected override void Reset()
    {
        base.Reset();
        Sprite.color = _orgColor;
        _targetColor = ColorEdit;
    }

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


        Color newColor = Color.Lerp(Sprite.color, _targetColor, Time.deltaTime / _timeLeft);
        _timeLeft -= Time.deltaTime;
        Sprite.color = newColor;
        if (Sprite.color == _targetColor && Loop)
        {
            _targetColor = _targetColor == _orgColor ? ColorEdit : _orgColor;
            _fromColor = _targetColor == _orgColor ? _orgColor : ColorEdit;
            _timeLeft = TransitionTime;
        }

    }
}
