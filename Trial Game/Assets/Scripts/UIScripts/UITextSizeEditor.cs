using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextSizeEditor : UIObjectEditor
{
    [SerializeField] private TextMeshProUGUI Text;
    public float FontSizeEdit;

    private float _orgFontSize;
    private float _fontSizeTarget;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    internal override void Reset()
    {
        base.Reset();
        Text.fontSize = _orgFontSize;
        _fontSizeTarget = FontSizeEdit;
    }

    // Start is called before the first frame update
    internal override void Start()
    {
        base.Start();
        _orgFontSize = Text.fontSize;
        _fontSizeTarget = FontSizeEdit;
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();
        if(_edit)
        {
            EditObject();
        }
    }

    internal override void EditObject()
    {
        base.EditObject();
        float newSize = Mathf.MoveTowards(Text.fontSize, _fontSizeTarget, Speed * Time.deltaTime);
        Text.fontSize = newSize;
        if (Text.fontSize == _fontSizeTarget && Loop)
            _fontSizeTarget = _fontSizeTarget == _orgFontSize ? FontSizeEdit : _orgFontSize;
    }
}
