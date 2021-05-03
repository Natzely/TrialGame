using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class Scene_Manager : MonoBehaviour
{
    [SerializeField] private float FadeMusicSpeed;

    private float WaitTime
    {
        set { _waitTime = value + .5f; }
    }

    private AudioSource _audioSource;
    private bool _loadScene;
    private bool _quitGame;
    private bool _fadeMusic;
    private float _waitTime;
    private string _currentScene;
    private string _sceneToLoad;

    public void RestartScene(float waitTime = 0)
    {
        WaitTime = waitTime;
        _loadScene = true;
        _sceneToLoad = _currentScene;
    }

    public void LoadScene(string sceneName, float waitTime = 0)
    {
        WaitTime = waitTime;
        _loadScene = true;
        _fadeMusic = true;
        _sceneToLoad = sceneName;
    }

    public void StartMusic()
    {
        _audioSource.PlayDelayed(.5f);
    }

    public void QuitGame(float waitTime = 0)
    {
        WaitTime = waitTime;
        _quitGame = true;
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _currentScene = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        if (_waitTime > 0)
            _waitTime -= Time.unscaledDeltaTime;

        if (_quitGame && _waitTime <= 0)
        {
            Application.Quit();
        }
        else if (_loadScene && _waitTime <= 0)
        {
            _loadScene = false;
            StartCoroutine(LoadScene(_sceneToLoad));
        }

        if(_fadeMusic && _audioSource.volume > 0)
        {
            _audioSource.volume -= Time.fixedDeltaTime * FadeMusicSpeed;
        }
    }

    public IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asynLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asynLoad.isDone)
        {
            yield return null;
        }
    }
}
