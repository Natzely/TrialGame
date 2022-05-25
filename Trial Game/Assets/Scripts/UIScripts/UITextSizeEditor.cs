using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextSizeEditor : UIObjectEditor
{
    public TextMeshProUGUI Text;
    public float FontSizeEdit;

    private float _orgFontSize;
    private float _fontSizeTarget;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        _orgFontSize = Text.fontSize;
        _fontSizeTarget = FontSizeEdit;
        base.Edit(edit);
    }

    protected override void Reset()
    {
        base.Reset();
        Text.fontSize = _orgFontSize;
        _fontSizeTarget = FontSizeEdit;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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
        float newSize = Mathf.MoveTowards(Text.fontSize, _fontSizeTarget, Speed * Time.fixedDeltaTime);
        Text.fontSize = newSize;
        if (Text.fontSize == _fontSizeTarget && Loop)
            _fontSizeTarget = _fontSizeTarget == _orgFontSize ? FontSizeEdit : _orgFontSize;
    }
}
