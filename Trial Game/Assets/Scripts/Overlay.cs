using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Overlay : MonoBehaviour
{
    public float MainRenderAlpha = .8f;
    public float OverlayAlpha = .2f;

    private SpriteRenderer _unitRenderer;
    private SpriteRenderer _sR;
    private UnitController _uC;

    private void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _unitRenderer = transform.parent.GetComponent<SpriteRenderer>();
        _uC = transform.parent.GetComponent<UnitController>();
        if(_uC && !_uC.Overlay)
        {
            _sR.enabled = false;
        }
        if (_unitRenderer)
            SetRenderAlpha(_unitRenderer, MainRenderAlpha);
    }

    // Update is called once per frame
    void Update()
    {
        if (_unitRenderer && ((_uC && _uC.Overlay) || !_uC))
        {
            _sR.enabled = _unitRenderer.enabled;
            _sR.sprite = _unitRenderer.sprite;
            SetRenderAlpha(_sR, OverlayAlpha);
            _sR.flipX = _unitRenderer.flipX;
            _sR.flipY = _unitRenderer.flipY;
        }
    }

    private void SetRenderAlpha(SpriteRenderer sR, float alpha)
    {
        sR.color = new Color(
            _unitRenderer.color.r,
            _unitRenderer.color.g,
            _unitRenderer.color.b,
            alpha);
    }
}
