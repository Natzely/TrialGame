using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class SceneManager : MonoBehaviour
{
    [SerializeField] internal EventSystem UIInput;
    [SerializeField] internal float FadeMusicSpeed;

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

    internal virtual void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    internal virtual void Start()
    {
        _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    internal virtual void Update()
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
            _audioSource.volume -= Time.unscaledDeltaTime * FadeMusicSpeed;
        }
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asynLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        yield return null;
    }
}
