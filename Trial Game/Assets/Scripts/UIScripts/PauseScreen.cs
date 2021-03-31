using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseScreen : UIUnityObject
{
    public Enums.PauseState State;
    public PauseScreen_MainButton FirstSelected;

    [SerializeField] private EventSystem EventSystem;
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private Scene_Manager SceneManager;
    [SerializeField] private GameObject HightLightPanel;
    [SerializeField] private GameObject MainButtonPanel;
    [SerializeField] private GameObject ControlsPanel;
    [SerializeField] private GameObject Controls_FirstSelected;
    [SerializeField] private AudioClip Sound_Enter;
    [SerializeField] private AudioClip Sound_Exit;

    public bool IsGamePaused { get; private set; }
    public bool IsSubMenuOpened { get; private set; }
    public GameObject CurrentButton { get { return EventSystem.currentSelectedGameObject; } }

    private PauseScreen_MainButton _currentButton;
    private PauseScreen_MainButton _prevButton;
    private RectTransform _rectTransform;
    private AudioSource _aS;
    private Vector2 _moveValue;
    private Vector2 _orgPos;
    private float _soundTime;

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
                    State = Enums.PauseState.Main;
                    ShowControlsPanel();
                    break;
                default:
                    break;
            }
        }
    }

    public void OnItemSelected(PauseScreen_MainButton button)
    {
        if (button != _currentButton)
        {
            _currentButton = button;
            //HightLightPanel.transform.localPosition = _currentButton.transform.localPosition;
        }
    }

    public void OnMainButtonClick(PauseScreen_MainButton button)
    {
        _aS.Play(Sound_Enter);
        switch (button.ButtonType)
        {
            case Enums.UI_PauseButtonType.Continue:
                DebugLog("Continue");
                ShowPauseScreen();
                break;
            case Enums.UI_PauseButtonType.Restart:
                SceneManager.RestartScene(_aS.clip.length);
                break;
            case Enums.UI_PauseButtonType.Controls:
                State = Enums.PauseState.Controls;
                ShowControlsPanel();
                break;
            case Enums.UI_PauseButtonType.Quit:
                DebugLog("Quit");
                SceneManager.QuitGame(_aS.clip.length);
                break;
            case Enums.UI_PauseButtonType.Controls_OK:
                State = Enums.PauseState.Main;
                DebugLog("Controls_OK");
                ShowControlsPanel();
                break;
        }
    }

    public void PauseScreen_Submit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _currentButton.Submit();
            OnMainButtonClick(_currentButton);
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
        _aS = GetComponent<AudioSource>();
        _currentButton = FirstSelected;
        _orgPos = new Vector2(0, 1450);
    }

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        ControlsPanel.transform.localPosition = _orgPos;
        IsGamePaused = _rectTransform.anchoredPosition == Vector2.zero;
        if (IsGamePaused)
            PlayerInput.SwitchCurrentActionMap("UI");
        else
            PlayerInput.SwitchCurrentActionMap("Player");
        Time.timeScale = IsGamePaused ? 0 : 1;
    }

    private void Update()
    {
        if (_soundTime > 0)
            _soundTime -= Time.fixedDeltaTime;
    }

    private void ShowPauseScreen()
    {
        IsGamePaused = !IsGamePaused;
        Time.timeScale = IsGamePaused ? 0 : 1;

        if (IsGamePaused)
        {
            _aS.Play(Sound_Enter);
            PlayerInput.SwitchCurrentActionMap("UI");
            EventSystem.SetSelectedGameObject(EventSystem.firstSelectedGameObject);
        }
        else
        {
            _aS.Play(Sound_Exit);
            PlayerInput.SwitchCurrentActionMap("Player");
        }

        _rectTransform.anchoredPosition = IsGamePaused ? Vector2.zero : _orgPos;
        DebugLog(IsGamePaused ? "Pause" : "Unpause" + " Game");
    }

    private void ShowControlsPanel()
    {
        if (State == Enums.PauseState.Controls)
        {
            _prevButton = _currentButton;
            ControlsPanel.transform.localPosition = Vector2.zero;
            MainButtonPanel.transform.localPosition = _orgPos;
            EventSystem.SetSelectedGameObject(Controls_FirstSelected.gameObject);
        }
        else
        {
            _aS.Play(Sound_Exit);
            ControlsPanel.transform.localPosition = _orgPos;
            MainButtonPanel.transform.localPosition = Vector2.zero;
            EventSystem.SetSelectedGameObject(_prevButton.gameObject);
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
