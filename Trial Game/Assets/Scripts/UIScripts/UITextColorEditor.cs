using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextColorEditor : UIObjectEditor
{
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private Color FontColorEdit;

    private Color _orgFontColor;
    private Color _fontColorTarget;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    public void UpdateColorEdit(Color newColor)
    {
        if (_fontColorTarget == FontColorEdit)
            _fontColorTarget = newColor;
        FontColorEdit = newColor;
    }

    protected override void Reset()
    {
        base.Reset();
        Text.color = _orgFontColor;
        _fontColorTarget = FontColorEdit;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        _orgFontColor = Text.color;
        _fontColorTarget = FontColorEdit;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (_edit)
        {
            EditObject();
        }
    }

    protected override void EditObject()
    {
        base.EditObject();
        
        Color newColor = Color.Lerp(Text.color, _fontColorTarget, Speed * Time.deltaTime);
        Text.color = newColor;
        if (Text.color == _fontColorTarget && Loop)
            _fontColorTarget = _fontColorTarget == _orgFontColor ? FontColorEdit : _orgFontColor;
    }
}
