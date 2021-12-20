using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveTo : UIObjectEditor
{
    [SerializeField] internal Vector2 MoveTo;

    internal RectTransform _rectT;
    internal Vector2 _orgPos;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    internal override void Reset()
    {
        _rectT.anchoredPosition = _orgPos;
    }

    internal override void Start()
    {
        base.Start();
        //Edit(true);
        _rectT = GetComponent<RectTransform>();
        _orgPos = _rectT.anchoredPosition;
        _speed = Speed;
        _distance = _curDistance = Vector2.Distance(_rectT.anchoredPosition, MoveTo);
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();
        if(base._edit)
        {
            EditObject();
        }
    }

    internal override void EditObject()
    {
        _curDistance = Vector2.Distance(_rectT.anchoredPosition, MoveTo);
        base.EditObject();

        Vector2 newPos = Vector2.MoveTowards(_rectT.anchoredPosition, MoveTo, _speed * Time.deltaTime);
        _rectT.anchoredPosition = newPos;
    }
}
