using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIMoveToAndFontSize : UIMoveTo
{
    [SerializeField] internal float ToFontSize;
    [SerializeField] internal TextMeshProUGUI Text;

    private float _orgFontSize;
    private float _fontDif;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    internal override void Reset()
    {
        Text.fontSize = _orgFontSize;
    }

    internal override void Start()
    {
        base.Start();
        _orgFontSize = Text.fontSize;
        _fontDif = ToFontSize - _orgFontSize;
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
        float newAdd = _fontDif * DistancePer;
        Text.fontSize = _orgFontSize + newAdd;
    }
}
