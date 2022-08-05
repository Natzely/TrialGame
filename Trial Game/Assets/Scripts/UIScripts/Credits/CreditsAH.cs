using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsAH : UIActionHandler
{
    [SerializeField] private string LevelName;

    private bool _sceneLoaded;
    public override void HandleButtonSubmit(UIButton button)
    {
        if (!_sceneLoaded)
            SceneManager.Instance.LoadScene(LevelName);
        _sceneLoaded = true;
    }
}
