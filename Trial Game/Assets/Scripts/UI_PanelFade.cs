using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_PanelFade : MonoBehaviour
{
    [SerializeField] private bool StartHidden;
    [SerializeField] private bool BlockAction;
    [SerializeField] private float FadeSpeed;

    private Image _image;
    private bool _start;
    private int _fadeDirection = -1;
    private float _fadeTime;

    ///<summary>
    ///Start the panel fade. Assign how long, in seconds, it should take instead of using the Fade Speed value
    ///</summary>
    public void StartFade(float fadeTime = 0)
    {
        _start = true;
        _fadeTime = fadeTime;
        if (StartHidden)
            _image.enabled = true;
    }

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    void Start()
    {
        if (StartHidden)
        {
            SetNewAlpha(0);
            _fadeDirection = 1;
        }

        _image.enabled = BlockAction;
    }

    // Update is called once per frame
    void Update()
    {
        if(_start)
        {
            float speedOrTime = _fadeTime > 0 ? (1 / _fadeTime) : FadeSpeed;
            float newAlpha = _image.color.a + (_fadeDirection * Time.deltaTime * speedOrTime);
            SetNewAlpha(newAlpha);
            if (_image.color.a <= 0 || _image.color.a >= 1)
            {
                _start = false;
                if (!StartHidden)
                    _image.enabled = false;
            }
        }
    }

    private void SetNewAlpha(float alpha)
    {
        _image.color = new Color(
            _image.color.r,
            _image.color.g,
            _image.color.b,
            alpha);
    }
}
