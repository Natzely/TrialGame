using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class SceneManager : MonoBehaviour
{
    [SerializeField] internal EventSystem UIInput;
    [SerializeField] internal UI_PanelFade LevelLoadFade;
    [SerializeField] internal float FadeMusicSpeed;
    [SerializeField] internal float TimeToLoad;
    [SerializeField] internal float TimeToFade;
    [SerializeField] internal bool DebugLog;
    [SerializeField] internal bool WriteDebugToFile;

    internal AudioSource _audioSource;
    private bool _loadScene;
    private bool _quitGame;
    private bool _fadeMusic;
    private bool _activateScene;
    private float _waitTime;
    private float _fadeTime;
    private string _currentScene;
    private string _sceneToLoad;

    public void RestartScene(float waitTime = 0)
    {
        _waitTime = waitTime;
        _loadScene = true;
        _sceneToLoad = _currentScene;
    }

    public void LoadScene(string sceneName, float waitTime = 0, float fadeTime = 0)
    {
        _waitTime = waitTime;
        _fadeTime = fadeTime;
        _loadScene = true;
        _fadeMusic = true;
        _sceneToLoad = sceneName;
        StartCoroutine(LoadScene(_sceneToLoad));
    }

    public void ActivateScene()
    {
        _activateScene = true;
    }

    public void QuitGame(float waitTime = 0)
    {
        _waitTime = waitTime;
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
            LevelLoadFade.StartFade(_fadeTime);
        }

        if (_fadeMusic && _audioSource.volume > 0)
        {
            _audioSource.volume -= Time.unscaledDeltaTime * FadeMusicSpeed;
        }
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return null;
        
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while(!asyncLoad.isDone)
        {
            if (_activateScene)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }
}
