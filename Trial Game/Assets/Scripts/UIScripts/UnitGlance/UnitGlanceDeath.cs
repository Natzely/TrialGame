using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGlanceDeath : MonoBehaviour
{
    public void SetPosition(Vector2 position)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
