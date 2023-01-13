using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIObject : MonoBehaviour, ILog
{
    public void Log(string msg)
    {
        DebugLogger.Instance?.Log(msg);
        Debug.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new System.NotImplementedException();
    }
}
