using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIObject : MonoBehaviour, ILog
{
    private TextMeshProUGUI _debugText;
    private Scrollbar _debugScroll;

    public virtual void Awake()
    {
        var debugObject = GameObject.FindGameObjectWithTag("DebugText");
        var scrollObject = GameObject.FindGameObjectWithTag("DebugScroll");
        if (debugObject && scrollObject)
        {
            _debugScroll = scrollObject?.GetComponent<Scrollbar>();
            _debugText = debugObject?.GetComponent<TextMeshProUGUI>();
            if (_debugText != null)
            {
                _debugText.text = "";
                _debugText.fontSize = 20;
            }
        }
    }

    public void Log(string msg)
    {
        DebugLogger.Instance?.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new System.NotImplementedException();
    }
}
