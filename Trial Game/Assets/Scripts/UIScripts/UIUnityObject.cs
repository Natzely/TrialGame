using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIUnityObject : MonoBehaviour, IDebugLog
{
    private TextMeshProUGUI _debugText;

    public virtual void Awake()
    {
        var debugObject = GameObject.FindGameObjectWithTag("DebugText");
        _debugText = debugObject?.GetComponent<TextMeshProUGUI>();
        _debugText.text = "";
        _debugText.fontSize = 20;
    }

    public void DebugLog(string msg)
    {
        if(_debugText != null)
            _debugText.text += msg + "\n";
    }
}
