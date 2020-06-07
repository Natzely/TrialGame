using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    public GameObject PauseScreenPanel;


    private bool _isGamePaused;
    private float _actionTimer;
    private float _lastRealTime;

    void Update()
    {
        bool _pause = Input.GetButtonUp("Pause");

        if (_actionTimer <= 0)
        {
            if (_pause)
            {
                if (_isGamePaused)
                {
                    Debug.Log("Unpause Game");
                    Time.timeScale = 1;
                    PauseScreenPanel.SetActive(false);
                    _isGamePaused = false;

                }
                else
                {
                    Debug.Log("Pause Game");
                    Time.timeScale = 0;
                    PauseScreenPanel.SetActive(true);
                    _isGamePaused = true;
                }

                _actionTimer = PlayerManager.ACTIONTIMER;
            }
        }
        else
        {
            _actionTimer -= Time.realtimeSinceStartup - _lastRealTime;
        }

        _lastRealTime = Time.realtimeSinceStartup;
    }
}
