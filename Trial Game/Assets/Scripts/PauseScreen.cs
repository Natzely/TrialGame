using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    public GameObject PauseScreenPanel;


    private bool _isGamePaused;
    private float _actionTimer;
    private float _lastRealTime;

    private void Start()
    {
        ShowPauseScreen(true);
    }

    void Update()
    {
        bool _pause = Input.GetButtonUp("Pause");

        if (_actionTimer <= 0)
        {
            if (_pause)
            {
                if (_isGamePaused)
                    ShowPauseScreen(false);
                else
                    ShowPauseScreen(true);

                _actionTimer = PlayerManager.ACTIONTIMER;
            }
        }
        else
        {
            _actionTimer -= Time.realtimeSinceStartup - _lastRealTime;
        }

        _lastRealTime = Time.realtimeSinceStartup;
    }

    private void ShowPauseScreen(bool pause)
    {
        Debug.Log(pause ? "Pause" : "Unpause" + " Game");
        Time.timeScale = pause ? 0 : 1;
        PauseScreenPanel.SetActive(pause);
        _isGamePaused = pause;
    }
}
