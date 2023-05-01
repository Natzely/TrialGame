using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitGlanceIcon : MonoBehaviour
{
    [SerializeField] private Sprite AztecIcon;
    [SerializeField] private Sprite SpanishIcon;
    [SerializeField] private Color AztecColor;
    [SerializeField] private Color SpanishColor;

    private Image _image;

    public Enums.PlayerSides Side
    {
        set
        {
            _image.sprite = value == Enums.PlayerSides.Aztec ? AztecIcon : SpanishIcon;
            _image.color = value == Enums.PlayerSides.Aztec ? AztecColor : SpanishColor;
        }
    }

    private void Awake()
    {
        _image = GetComponent<Image>();   
    }
}
