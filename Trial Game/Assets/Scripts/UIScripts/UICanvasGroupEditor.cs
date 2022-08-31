using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvasGroupEditor : UIObjectEditor
{
    [SerializeField][Range(0,1)] public float AlphaEdit;

    CanvasGroup _cGroup;
    private float _orgAlpha;

    public override void Edit(bool edit = true)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    protected override void Start()
    {
        base.Start();
        _cGroup = GetComponent<CanvasGroup>();
        _orgAlpha = _cGroup.alpha;
        _distance = _curDistance = Mathf.Abs(AlphaEdit - _cGroup.alpha);
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

        _curDistance = Mathf.Abs(AlphaEdit - _cGroup.alpha);
        float newAlpha = Mathf.MoveTowards(_cGroup.alpha, AlphaEdit, Speed * Time.unscaledDeltaTime);
        _cGroup.alpha = newAlpha;

        if (_cGroup.alpha == AlphaEdit)
        {
            base.Edit(false);
            if (EditEvent != null)
                EditEvent.Invoke();
        }
    }

    protected override void Reset()
    {
        _cGroup.alpha = _orgAlpha;
        base.Reset();
    }
}
