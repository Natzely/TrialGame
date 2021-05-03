using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIUnityObject : MonoBehaviour, IDebugLog
{
    private TextMeshProUGUI _debugText;
    private Scrollbar _debugScroll;

    public virtual void Awake()
    {
        var debugObject = GameObject.FindGameObjectWithTag("DebugText");
        var scrollObject = GameObject.FindGameObjectWithTag("DebugScroll");
        _debugScroll = scrollObject?.GetComponent<Scrollbar>();
        _debugText = debugObject?.GetComponent<TextMeshProUGUI>();
        if (_debugText != null)
        {
            _debugText.text = "";
            _debugText.fontSize = 20;
        }
    }

    public void DebugLog(string msg)
    {
        if (_debugText != null && Application.isEditor)
        {
            _debugText.text += msg + "\n";
            _debugScroll.value = 0;
        }
    }
}
