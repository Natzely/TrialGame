using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseScreen : MonoBehaviour
{
    public bool IsGamePaused { get; private set; }

    [SerializeField] private GameObject PauseScreenPanel;

    private float _actionTimer;
    private float _lastRealTime;

    public void ShowPauseScreen(InputAction.CallbackContext context)
    {
        ShowPauseScreen();
    }

    private void Start()
    {
        //ShowPauseScreen();
    }

    private void ShowPauseScreen()
    {
        IsGamePaused = !IsGamePaused;
        Time.timeScale = IsGamePaused ? 0 : 1;
        PauseScreenPanel.SetActive(IsGamePaused);
        Debug.Log(IsGamePaused ? "Pause" : "Unpause" + " Game");
    }
}
