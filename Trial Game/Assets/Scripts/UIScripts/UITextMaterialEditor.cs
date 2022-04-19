using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextMaterialEditor : UIObjectEditor
{
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] [ColorUsage(true, true)] private Color GlowColor;
    [SerializeField] [Range(0, 1)] private float GlowPowerEdit;

    private float _orgGlowPower;
    private float _glowPowerTarget;
    private int GlowPowerID;

    public override void Edit(bool edit)
    {
        if (!edit)
            Reset();
        base.Edit(edit);
    }

    public void UpdateColorEdit(float newGlowPower)
    {
        if (_glowPowerTarget == GlowPowerEdit)
            _glowPowerTarget = newGlowPower;
        GlowPowerEdit = newGlowPower;
    }

    internal override void Reset()
    {
        base.Reset();
        Text.fontSharedMaterial.SetFloat(GlowPowerID, _orgGlowPower);
        _glowPowerTarget = GlowPowerEdit;
    }
    private void Awake()
    {
        GlowPowerID = ShaderUtilities.ID_GlowPower;
    }

    // Start is called before the first frame update
    internal override void Start()
    {
        base.Start();
        _orgGlowPower = Text.fontSharedMaterial.GetFloat(GlowPowerID);
        _glowPowerTarget = GlowPowerEdit;
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();
        if (_edit)
        {
            EditObject();
        }
    }

    internal override void EditObject()
    {
        base.EditObject();

        float curGlowPower = Text.fontSharedMaterial.GetFloat(GlowPowerID);
        float newGlowPower = Mathf.MoveTowards(curGlowPower, _glowPowerTarget, Speed * Time.deltaTime);
        Text.fontSharedMaterial.SetFloat(GlowPowerID, newGlowPower);

        //Color newColor = Color.Lerp(Text.color, _glowPowerTarget, Speed * Time.deltaTime);
        //Text.color = newColor;
        //if (Text.color == _glowPowerTarget && Loop)
        //    _glowPowerTarget = _glowPowerTarget == _orgGlowPower ? GlowColor : _orgGlowPower;
    }
}
