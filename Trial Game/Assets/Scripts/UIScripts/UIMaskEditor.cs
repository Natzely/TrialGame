using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectMask2D))]
public class UIMaskEditor : UIObjectEditor
{
    [SerializeField] private Vector4 PaddingEdit;
    [SerializeField] private Vector2Int SoftnessEdit;

    private RectMask2D _mask;
    private Vector4 _orgPadding;
    private Vector4 _orderedPadding;
    private Vector2Int _orgSoftness;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    protected override void Reset()
    {
        _mask.padding = _orgPadding;
        _mask.softness = _orgSoftness;
        base.Reset();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //Edit(true);
        _mask = GetComponent<RectMask2D>();
        // Padding is ordered differently from LRTB to XYZW
        // X = X(L) | Y = W(T) | Z = Y(B) | W = Z(R)
        _orderedPadding = new Vector4(PaddingEdit.x, PaddingEdit.w, PaddingEdit.y, PaddingEdit.z);
        _orgPadding = _mask.padding;
        _orgSoftness = _mask.softness;
        _distance = _curDistance = Vector4.Distance(_orgPadding, PaddingEdit);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(base._edit)
        {
            EditObject();
        }
    }

    protected override void EditObject()
    {
        if (!_mask.padding.Equals(_orderedPadding) || _mask.softness.Equals(SoftnessEdit))
        {
            _curDistance = Vector4.Distance(_mask.padding, _orderedPadding);
            base.EditObject();

            if (!_mask.padding.Equals(_orderedPadding))
            {
                Vector4 newPadding = Vector4.MoveTowards(_mask.padding, _orderedPadding, _speed * Time.deltaTime);
                _mask.padding = newPadding;//new Vector4(newPadding.x, newPadding.z, newPadding.w, newPadding.y);
            }

            if (!_mask.softness.Equals(SoftnessEdit))
            {
                Vector2Int newSoftness = Vector2Int.CeilToInt(Vector2.MoveTowards(_mask.softness, SoftnessEdit, _speed * Time.deltaTime));
                _mask.softness = newSoftness;
            }
        }
    }
}
