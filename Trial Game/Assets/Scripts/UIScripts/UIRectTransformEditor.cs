using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRectTransformEditor : UIObjectEditor
{
    private RectTransform _rectT;

    [SerializeField] private Vector3 scaleEdit;

    private Vector3 _orgScale;

    public override void Edit(bool edit = true)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    private void Awake()
    {
        _rectT = GetComponent<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();
        _orgScale = _rectT.localScale;
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

        Vector3 newScale = Vector3.MoveTowards(_rectT.localScale, scaleEdit, Speed * Time.deltaTime);
        _rectT.localScale = newScale;
    }

    protected override void Reset()
    {
        base.Reset();
        _rectT.localScale = _orgScale;
    }
}
