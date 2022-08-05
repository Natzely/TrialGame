using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PauseScreenAH : UIActionHandler
{
    public Enums.PauseState State;

    [SerializeField] private InputSystemUIInputModule InputSystem;
    [SerializeField] private GameObject HightLightPanel;
    [SerializeField] private GameObject MainButtonPanel;
    [SerializeField] private GameObject ControlsPanel;
    [SerializeField] private GameObject Controls_FirstSelected;
    [SerializeField] private ConfirmSelectionController ConfirmPanel;
    [SerializeField] private UnitInfoAH UnitInfo;

    private LevelManager LevelManager { get { return (LevelManager)SceneManager.Instance; } }
    private bool IsGamePaused { get { return LevelManager.GameState == Enums.GameState.Pause; } }
    private bool IsGamePausable 
    { 
        get { return LevelManager.GameState == Enums.GameState.Play ||
                     LevelManager.GameState == Enums.GameState.TimeStop; } 
    }

    private Enums.GameState _prevState;
    private RectTransform _rectTransform;
    private UIButton _confirmButton;
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
                case Enums.PauseState.Restart:
                case Enums.PauseState.Quit:
                    ShowConfirmPanel(Enums.PauseState.Main);
                    break;
                case Enums.PauseState.UnitInfo:
                    _audioSource.Play(Sound_Exit);
                    State = Enums.PauseState.Main;
                    break;
                default:
                    break;
            }
        }
    }

    public void UnitInfoKeyPress(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            var unitType = CursorController.Instance.CurrentUnitType;
            if (unitType != Enums.UnitInfo.None)
                ShowPauseScreen(unitType);
        }
    }


    public override void HandleButtonSubmit(UIButton button)
    {
        Log($"{gameObject.name}: HandleButtonSubmit");
        var pauseButton = button as PauseScreen_Button;
        var confirmButton = button as ConfirmPanelButton;

        if (pauseButton)
        {
            _audioSource.Play(Sound_Enter);
            switch (pauseButton.Type)
            {
                case Enums.UI_PauseButtonType.Continue:
                    Log("Continue");
                    ShowPauseScreen();
                    break;
                case Enums.UI_PauseButtonType.Restart:
                    ShowConfirmPanel(Enums.PauseState.Restart, pauseButton);
                    //LevelManager.RestartScene();// _audioSource.clip.length);
                    break;
                case Enums.UI_PauseButtonType.Controls:
                    ShowControlsPanel();
                    break;
                case Enums.UI_PauseButtonType.Quit:
                    ShowConfirmPanel(Enums.PauseState.Quit, pauseButton);
                    //DebugLog("Quit");
                    //LevelManager.QuitGame(_audioSource.clip.length);
                    break;
                case Enums.UI_PauseButtonType.Controls_OK:
                    Log("Controls_OK");
                    ShowControlsPanel();
                    break;
                case Enums.UI_PauseButtonType.Units:
                    State = Enums.PauseState.UnitInfo;
                    UnitInfo.Show();
                    break;
                default:
                    break;
            }
        }

        if (confirmButton)
        {
            switch (confirmButton.Type)
            {
                case Enums.UI_ConfirmButtonType.Confirm:
                    switch(State)
                    {
                        case Enums.PauseState.Restart:
                            LevelManager.RestartScene();
                            break;
                        case Enums.PauseState.Quit:
                            LevelManager.QuitGame();
                            break;
                    }    
                    break;
                case Enums.UI_ConfirmButtonType.Cancel:
                    ShowConfirmPanel(Enums.PauseState.Main);
                    break;
                default:
                    break;
            }
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

    protected override void Awake()
    {
        _orgPos = new Vector2(0, 1450);
        base.Awake();
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

    private void ShowPauseScreen(Enums.UnitInfo unitType = Enums.UnitInfo.None)
    {
        if (IsGamePausable)
        {
            _audioSource.Play(Sound_Enter);
            PlayerInput.SwitchCurrentActionMap("UI");
            InputSystem.moveRepeatDelay = 3;
            FirstSelected.SilentSelect = true;
            _eventSystem.SetSelectedGameObject(FirstSelected.gameObject);
            _prevState = LevelManager.GameState;
            LevelManager.GameState = Enums.GameState.Pause;

            if (unitType != Enums.UnitInfo.None)
            {
                State = Enums.PauseState.UnitInfo;
                UnitInfo.Show(unitType);
            }
        }
        else if(LevelManager.GameState == Enums.GameState.Pause)
        {
            _hidePauseScreen = true;
            _eventSystem.SetSelectedGameObject(null);
            LevelManager.GameState = _prevState;
        }

        Time.timeScale = IsGamePaused ? 0 : 1;


        _rectTransform.anchoredPosition = IsGamePaused ? Vector2.zero : _orgPos;
        Log((IsGamePaused ? "Pause" : "Unpause") + " Game");
    }

    private void ShowConfirmPanel(Enums.PauseState state, UIButton button = null)
    {
        ConfirmPanel.Edit(state != Enums.PauseState.Main);
        State = state;
        if (state == Enums.PauseState.Main)
            _confirmButton.Select();
        _confirmButton = button;            
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
}
