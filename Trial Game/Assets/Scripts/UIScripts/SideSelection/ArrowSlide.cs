using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSlide : MonoBehaviour
{
    [SerializeField] RectTransform MainMask;
    [SerializeField] private bool FlipDirection;
    [SerializeField] private float Speed;

    RectTransform _transform;
    private float _startWidth;
    private float _moveDirection;

    // Start is called before the first frame update
    void Start()
    {
        _startWidth = MainMask.sizeDelta.x;
        _transform = GetComponent<RectTransform>();
        _moveDirection = FlipDirection ? -1 : 1;
    }

    // Update is called once per frame
    void Update()
    {
        _transform.anchoredPosition = new Vector2(Mathf.Abs(MainMask.sizeDelta.x - _startWidth) * _moveDirection * Speed, 0);
    }
}
