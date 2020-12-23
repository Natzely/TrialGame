using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Saved : GridBlockItem
{

    public void SetPathDirection(Enums.PathDirection pathDirection)
    {
        _pathDirection = pathDirection;
    }

    public void SetColor(Color color)
    {
        _color = color;
        if (_sR != null)
            _sR.color = color;
    }

    private SpriteRenderer _sR;
    private Enums.PathDirection _pathDirection;

    private Color _color;

    private void Awake()
    {
        _sR = GetComponent<SpriteRenderer>();
        _pathDirection = Enums.PathDirection.Right;
    }

    // Start is called before the first frame update
    void Start()
    {
        switch(_pathDirection)
        {
            case Enums.PathDirection.Left:
                _sR.flipY = true;
                break;
            case Enums.PathDirection.Up:
                transform.Rotate(0, 0, 90);
                break;
            case Enums.PathDirection.Down:
                transform.Rotate(0, 0, -90);
                break;
            default:
                break;
        }
    }
}
