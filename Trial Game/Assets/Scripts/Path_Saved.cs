using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Saved : GridBlockItem
{
    public SpriteRenderer SavedPathSprite;

    public void SetPathDirection(Enums.PathDirection pathRotation)
    {
        _pathRotation = pathRotation;
    }

    public void SetColor(Color color)
    {
        if (SavedPathSprite != null)
            SavedPathSprite.color = color;
    }

    private Enums.PathDirection _pathRotation;

    // Start is called before the first frame update
    void Start()
    {
        if (_pathRotation == Enums.PathDirection.Up)
            transform.Rotate(0, 0, 90);
        else if (_pathRotation == Enums.PathDirection.Down)
            transform.Rotate(0, 0, -90);
        else if (_pathRotation == Enums.PathDirection.Left)
            transform.Rotate(0, 0, 180);
    }
}
