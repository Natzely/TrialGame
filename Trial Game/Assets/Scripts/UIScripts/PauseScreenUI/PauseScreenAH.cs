using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PauseScreenAH : UIActionHandler, ILog
{
    public Enums.PauseState State;

    [SerializeField] private LevelManager LevelManager;
    [SerializeField] private InputSystemUIInputModule InputSystem;
    [SerializeField] private GameObject HightLightPanel;
    [SerializeField] private GameObject MainButtonPanel;
    [SerializeField] private GameObject ControlsPanel;
    [SerializeField] private GameObject Controls_FirstSelected;

    private bool IsGamePaused { get { return LevelManager.GameState == Enums.GameState.Pause; } }
    public bool IsSubMenuOpened { get; private set; }

    private RectTransform _rectTransform;
    private Vector2 _moveValue;
    private Vector2 _orgPos;
    private float _soundTime;
    private bool _hidePauseScreen;

    public void EscapeKeyPress(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (State)
            {
                case Enums.PauseState.Main:
                    ShowPauseScreen();
                    break;
                case Enums.PauseState.Controls:
                    ShowControlsPanel();
                    break;
                default:
                    break;
            }
        }
    }


    public override void HandleButtonSubmit(UIButton button)
    {
        Log($"{gameObject.name}: HandleButtonSubmit");
        var pauseButton = (PauseScreen_Button)button;
        _audioSource.Play(Sound_Enter);

        switch (pauseButton.Type)
        {
            case Enums.UI_PauseButtonType.Continue:
                DebugLog("Continue");
                ShowPauseScreen();
                break;
            case Enums.UI_PauseButtonType.Restart:
                _sceneManager.RestartScene(_audioSource.clip.length);
                break;
            case Enums.UI_PauseButtonType.Controls:
                ShowControlsPanel();
                break;
            case Enums.UI_PauseButtonType.Quit:
                DebugLog("Quit");
                _sceneManager.QuitGame(_audioSource.clip.length);
                break;
            case Enums.UI_PauseButtonType.Controls_OK:
                DebugLog("Controls_OK");
                ShowControlsPanel();
                break;
        }
    }

    public void PauseScreen_Submit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HandleButtonSubmit(_currentButton);
        }
    }

    public void PauseScreen_Move(InputAction.CallbackContext context)
    {
        if(context.started)
            _moveValue = context.ReadValue<Vector2>();
        if (context.performed)
            Move(_moveValue);
    }

    public void ShowSubMenu()
    {
        IsSubMenuOpened = true;
    }

    public override void Awake()
    {
        base.Awake();
        _orgPos = new Vector2(0, 1450);
    }

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        ControlsPanel.transform.localPosition = _orgPos;
        Time.timeScale = IsGamePaused ? 0 : 1;
    }

    private void Update()
    {
        if (_soundTime > 0)
            _soundTime -= Time.unscaledDeltaTime;

        if (_hidePauseScreen)
        {
            _audioSource.Play(Sound_Exit);
            PlayerInput.SwitchCurrentActionMap("Player");
            _hidePauseScreen = false;
        }
    }

    private void ShowPauseScreen()
    {
        if (LevelManager.GameState == Enums.GameState.Play)
        {
            _audioSource.Play(Sound_Enter);
            PlayerInput.SwitchCurrentActionMap("UI");
            InputSystem.moveRepeatDelay = 3;
            _eventSystem.SetSelectedGameObject(FirstSelected.gameObject);
            LevelManager.GameState = Enums.GameState.Pause;
            
        }
        else if(LevelManager.GameState == Enums.GameState.Pause)
        {
            _hidePauseScreen = true;
            LevelManager.GameState = Enums.GameState.Play;
            //_audioSource.Play(Sound_Exit);
            //PlayerInput.SwitchCurrentActionMap("Player");
        }

        Time.timeScale = IsGamePaused ? 0 : 1;

        _rectTransform.anchoredPosition = IsGamePaused ? Vector2.zero : _orgPos;
        DebugLog((IsGamePaused ? "Pause" : "Unpause") + " Game");
    }

    private void ShowControlsPanel()
    {
        if (State == Enums.PauseState.Main)
        {
            _prevButton = _currentButton;
            ControlsPanel.transform.localPosition = Vector2.zero;
            MainButtonPanel.transform.localPosition = _orgPos;
            _eventSystem.SetSelectedGameObject(Controls_FirstSelected);
            State = Enums.PauseState.Controls;
        }
        else
        {
            _audioSource.Play(Sound_Exit);
            ControlsPanel.transform.localPosition = _orgPos;
            MainButtonPanel.transform.localPosition = Vector2.zero;
            _eventSystem.SetSelectedGameObject(_prevButton.gameObject);
            State = Enums.PauseState.Main;
        }
    }

    private void Move(Vector2 move)
    {
        switch (State)
        {
            case Enums.PauseState.Main:
                Move_MainMenu(move);
                break;
            case Enums.PauseState.Controls:
                break;
        }
    }

    private void Move_MainMenu(Vector2 move)
    {
        if (move.y != 0)
        {
            move.x = 0;
            _currentButton.MoveToNextUIObject(move);
        }
    }

    public void Log(string msg)
    {
        DebugLogger.Instance.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new System.NotImplementedException();
    }
}
