using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    private float WaitTime
    {
        set { _waitTime = value + .5f; }
    }
    private Scene _currentScene;
    private Scene _sceneToLoad;
    private bool _loadScene;
    private bool _quitGame;
    private float _waitTime;

    public void RestartScene(float waitTime = 0)
    {
        WaitTime = waitTime;
        _loadScene = true;
        _sceneToLoad = _currentScene;
    }

    public void QuitGame(float waitTime = 0)
    {
        WaitTime = waitTime + 1;
        _quitGame = true;
    }

    public void Update()
    {
        if (_waitTime > 0)
            _waitTime -= Time.fixedDeltaTime;

        if (_quitGame && _waitTime <= 0)
        {
            Application.Quit();
        }
        else if (_loadScene && _waitTime <= 0)
        {
            _loadScene = false;
            StartCoroutine(LoadScene(_sceneToLoad));
        }
    }

    void Start()
    {
        _currentScene = SceneManager.GetActiveScene();
    }

    public IEnumerator LoadScene(Scene scene)
    {
        AsyncOperation asynLoad = SceneManager.LoadSceneAsync(scene.name);

        while (!asynLoad.isDone)
        {
            yield return null;
        }
    }
}
