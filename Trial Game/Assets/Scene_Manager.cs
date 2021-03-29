using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    private Scene _currentScene;

    public void RestartScene()
    {
        SceneManager.LoadSceneAsync(_currentScene.name);
    }

    void Start()
    {
        _currentScene = SceneManager.GetActiveScene();
    }
}
