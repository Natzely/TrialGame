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
        if (!_uC.Overlay)
        {
            _sR.enabled = false;
            SetRendereAlpha(_unitRenderer, 1);
        }
        if (_unitRenderer)
            SetRendereAlpha(_unitRenderer, MainRenderAlpha);
    }

    // Update is called once per frame
    void Update()
    {
        if (_unitRenderer)
        {
            _sR.sprite = _unitRenderer.sprite;
            SetRendereAlpha(_sR, OverlayAlpha);
            _sR.flipX = _unitRenderer.flipX;
        }
    }

    private void SetRendereAlpha(SpriteRenderer sR, float alpha)
    {
        sR.color = new Color(
            _unitRenderer.color.r,
            _unitRenderer.color.g,
            _unitRenderer.color.b,
            alpha);
    }
}
