using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class SceneManager : MonoBehaviour
{
    [SerializeField] protected EventSystem UIInput;
    [SerializeField] protected UI_PanelFade LevelLoadFade;
    [SerializeField] protected float FadeMusicSpeed;
    [SerializeField] protected float TimeToLoad;
    [SerializeField] protected float TimeToFade;
    [SerializeField] public bool DebugLog;
    [SerializeField] public bool WriteDebugToFile;

    protected AudioSource _audioSource;
    protected string _currentScene;

    private bool _loadScene;
    private bool _quitGame;
    private bool _quitGameAfterFade;
    private bool _fadeMusic;
    private bool _activateScene;
    private float _waitTime;
    private float _fadeTime;
    private string _sceneToLoad;

    public void LoadScene(string sceneName, float waitTime = 0, float fadeTime = 0)
    {
        _waitTime = waitTime;
        _fadeTime = fadeTime;
        _loadScene = true;
        _fadeMusic = true;
        _sceneToLoad = sceneName;
    }

    public void ActivateScene()
    {
        _activateScene = true;
        if (_quitGameAfterFade)
            Application.Quit();
    }

    public void QuitGame(float waitTime = 0)
    {
        _waitTime = waitTime;
        _quitGame = true;
    }

    protected virtual void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    protected virtual void Update()
    {
        if (_waitTime > 0)
            _waitTime -= Time.unscaledDeltaTime;

        if (_quitGame && _waitTime <= 0)
        {
            _quitGame = false;
            _quitGameAfterFade = true;
            LevelLoadFade.StartFade();
        }
        else if (_loadScene && _waitTime <= 0)
        {
            _loadScene = false;
            LevelLoadFade.StartFade();
            StartCoroutine(LoadScene(_sceneToLoad));
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
