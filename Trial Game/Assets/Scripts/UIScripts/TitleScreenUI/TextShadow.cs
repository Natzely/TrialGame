using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextShadow : MonoBehaviour
{
    public TextMeshProUGUI CopyText;
    public Vector3 PositionOffset;
    [Range(0, 1)] public float Alpha = 1;


    private TextMeshProUGUI _text;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _text.text = CopyText.text;
    }

    private void Update()
    {
        if (_text.color.a != Alpha || _text.fontSize != CopyText.fontSize || 
            _text.transform.rotation != CopyText.transform.rotation ||
            (_text.rectTransform.position - PositionOffset) != CopyText.rectTransform.position)
            UpdateText();
    }

    private void UpdateText()
    {
        _text.rectTransform.anchorMin = CopyText.rectTransform.anchorMin;
        _text.rectTransform.anchorMax = CopyText.rectTransform.anchorMax;
        _text.rectTransform.position = CopyText.rectTransform.position + PositionOffset;
        _text.font = CopyText.font;
        _text.fontSize = CopyText.fontSize;
        _text.fontStyle = CopyText.fontStyle;
        _text.alignment = CopyText.alignment;
        _text.characterSpacing = CopyText.characterSpacing;
        _text.text = CopyText.text;
        _text.rectTransform.sizeDelta = CopyText.rectTransform.sizeDelta;
        _text.color = new Color(
            Color.black.r,
            Color.black.g,
            Color.black.b,
            Alpha);
    }
}
