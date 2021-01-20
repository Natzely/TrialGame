using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Saved : GridBlockItem
{
    public SpriteRenderer SavedPathSprite;

    public void SetPathDirection(Vector2 pathRotation)
    {
        _pathRotation = pathRotation;
    }

    public void SetColor(Color color)
    {
        if (SavedPathSprite != null)
            SavedPathSprite.color = color;
    }

    private Vector2 _pathRotation;

    // Start is called before the first frame update
    void Start()
    {
        if (_pathRotation == Vector2.left)
            transform.Rotate(0, 0, 180);
        else if (_pathRotation == Vector2.up)
            transform.Rotate(0, 0, 90);
        else if (_pathRotation == Vector2.down)
            transform.Rotate(0, 0, -90);
    }
}
