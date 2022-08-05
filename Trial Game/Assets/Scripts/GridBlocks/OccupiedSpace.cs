using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupiedSpace : MonoBehaviour
{
    [SerializeField] private SpriteRenderer Sprite;
    [SerializeField] private SpriteColorEditor ColorEditor;
    [SerializeField] private float MinAlpha;
    [SerializeField] private float MaxAlpha;

    public Enums.Player Player
    {
        get { return _player; }
        set
        {
            _player = value;
            SetColor();
        }
    }

    private Enums.Player _player;
    void Start()
    {
        SetColor();
    }

    private void SetColor()
    {
        Color targetColor = Player == Enums.Player.Player1 ? Colors.PlayerOccupied : Colors.EnemyOccupied;
        Sprite.color = new Color(targetColor.r, targetColor.g, targetColor.b, MaxAlpha);
        ColorEditor.ColorEdit = new Color(targetColor.r, targetColor.g, targetColor.b, MinAlpha);
        ColorEditor.Edit(true);
    }
}
