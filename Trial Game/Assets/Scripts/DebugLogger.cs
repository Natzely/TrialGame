using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugLogger : MonoBehaviour
{
    public SceneManager SceneManager;
    public TextMeshProUGUI DebugText;

    public static DebugLogger Instance { get ; private set; }

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void Log(string msg)
    {
        if (SceneManager && SceneManager.DebugLog)
        {
            Debug.Log(msg);
            if (SceneManager.WriteDebugToFile)
            {
                DebugText.text += msg + "\n";
            }
        }
    }
    public void LogError(string msg)
    {
        if (SceneManager && SceneManager.DebugLog)
        {
            Debug.Log(msg);
            if (SceneManager.WriteDebugToFile)
            {
                DebugText.text += msg + "\n";
            }
        }
    }
}
