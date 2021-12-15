using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SideSelectionMask : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public float TextMoveToHeight;
    public float MoveSpeed;
    public float MaxTextSize;

   [SerializeField]
    public bool Selected { get; set; }

    private RectTransform _textTrans;
    private float _orgTextSize;
    private float _orgY;

    void Start()
    {
        _textTrans = Text.rectTransform;
        _orgY = _textTrans.anchoredPosition.y;
        _orgTextSize = Text.fontSize;
    }

    // Update is called once per frame
    void Update()
    {
        if(Selected && _textTrans.anchoredPosition.y != TextMoveToHeight)
        {
            float newPosY = Mathf.MoveTowards(_textTrans.anchoredPosition.y, TextMoveToHeight, MoveSpeed * Time.deltaTime);
            _textTrans.anchoredPosition = new Vector2(_textTrans.anchoredPosition.x, newPosY);
            float newFontSize = _orgTextSize + (MaxTextSize - _orgTextSize) * (_textTrans.anchoredPosition.y / TextMoveToHeight);
            Text.fontSize = newFontSize;
        }
        else if(!Selected  && _textTrans.anchoredPosition.y != _orgY)
        {
            _textTrans.anchoredPosition = new Vector2(_textTrans.anchoredPosition.x, _orgY);
            Text.fontSize = _orgTextSize;
        }
    }
}
