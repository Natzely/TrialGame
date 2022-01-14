using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Image))]
public class UIImageEditor : UIObjectEditor
{
    [SerializeField] [Range(0, 1)] public float AlphaEdit;

    private Image _image;
    private float _orgAlpha;

    public override void Edit(bool edit = true)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    // Start is called before the first frame update
    internal override void Start()
    {
        base.Start();
        _orgAlpha = _image.color.a;
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

        _curDistance = Mathf.Abs(AlphaEdit - _image.color.a);
        float newAlpha = Mathf.MoveTowards(_image.color.a, AlphaEdit, Speed * Time.deltaTime);
        _image.color = SetAlpha(newAlpha);

        if (_image.color.a == AlphaEdit)
        {
            base.Edit(false);
            if (EditEvent != null)
                EditEvent.Invoke();
        }
    }

    internal override void Reset()
    {
        base.Reset();
        _image.color = SetAlpha(_orgAlpha);
    }

    private Color SetAlpha(float alpha)
    {
        return new Color
            (
                _image.color.r,
                _image.color.g,
                _image.color.b,
                alpha
            );
    }
}
