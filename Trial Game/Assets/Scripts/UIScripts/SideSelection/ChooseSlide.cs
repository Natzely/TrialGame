using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseSlide : MonoBehaviour
{
    [SerializeField] RectTransform MainMask;
    [SerializeField] private float Speed;

    RectTransform _transform;
    private float _startWidth;
    private float _startHeight;

    // Start is called before the first frame update
    void Start()
    {
        _startWidth = MainMask.sizeDelta.x;
        _transform = GetComponent<RectTransform>();
        _startHeight = _transform.sizeDelta.y;
    }

    // Update is called once per frame
    void Update()
    {
        _transform.sizeDelta = new Vector2(_transform.sizeDelta.x,
            _startHeight - Mathf.Abs(MainMask.sizeDelta.x - _startWidth) * Speed);
    }
}
