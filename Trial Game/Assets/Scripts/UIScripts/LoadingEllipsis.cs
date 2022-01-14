using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingEllipsis : MonoBehaviour
{
    public bool Loading;

    [SerializeField] private float AddDotTime;

    private TextMeshProUGUI _text;
    private float _addTimer;

    public void LoadingStart()
    {
        Loading = true;
        _text.enabled = true;
    }

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _addTimer = AddDotTime;
    }
    // Update is called once per frame
    void Update()
    {
        if (Loading)
        {
            _addTimer -= Time.deltaTime;
            if (_addTimer <= 0)
            {
                if (_text.text.Length == 3)
                    _text.text = ".";
                else
                    _text.text += ".";
                _addTimer = AddDotTime;
            }
        }
    }
}
