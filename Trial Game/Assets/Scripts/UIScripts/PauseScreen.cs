using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseScreen : UIUnityObject
{
    public Enums.PauseState State;
    public PlayerInput PlayerInput;
    public EventSystem EventSystem;
    public PauseScreen_MainButton FirstSelected;
    public Scene_Manager SceneManager;

    [SerializeField] private GameObject PauseScreenPanel;
    [SerializeField] private GameObject HightLightPanel;

    public bool IsGamePaused { get; private set; }
    public bool IsSubMenuOpened { get; private set; }

    private SortedList<Enums.UI_PauseButtonType, PauseScreen_MainButton> _mainButtons;
    private PauseScreen_MainButton _currentButton;

    private RectTransform _rectTransform;
    private Vector2 _moveValue;
    private Vector2 _orgPos;

    public void ShowPauseScreen(InputAction.CallbackContext context)
    {
        if (context.performed == true)
            ShowPauseScreen();
    }

    public void OnItemSelected(PauseScreen_MainButton button)
    {
        if (button != _currentButton)
        {
            _currentButton = button;
            HightLightPanel.transform.localPosition = _currentButton.transform.localPosition;
        }
    }

    public void OnMainButtonClick(PauseScreen_MainButton button)
    {
        switch (button.ButtonType)
        {
            case Enums.UI_PauseButtonType.Continue:
                DebugLog("Continue");
                ShowPauseScreen();
                break;
            case Enums.UI_PauseButtonType.Restart:
                SceneManager.RestartScene();
                break;
            case Enums.UI_PauseButtonType.Controls:
                break;
            case Enums.UI_PauseButtonType.Quit:
                DebugLog("Quit");
                Application.Quit();
                break;
        }
    }

    public void AddButtonToList(PauseScreen_MainButton button)
    {
        _mainButtons.Add(button.ButtonType, button);
    }

    public void PauseScreen_Submit(InputAction.CallbackContext context)
    {
        if (context.performed)
            _currentButton.Submit();
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
        _mainButtons = new SortedList<Enums.UI_PauseButtonType, PauseScreen_MainButton>();
        _currentButton = FirstSelected;
        _orgPos = new Vector2(0, 1450);
    }

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        IsGamePaused = _rectTransform.anchoredPosition == Vector2.zero;
        if (IsGamePaused)
            PlayerInput.SwitchCurrentActionMap("UI");
        else
            PlayerInput.SwitchCurrentActionMap("Player");
        Time.timeScale = IsGamePaused ? 0 : 1;
    }

    private void ShowPauseScreen()
    {
        IsGamePaused = !IsGamePaused;
        Time.timeScale = IsGamePaused ? 0 : 1;

        if (IsGamePaused)
            PlayerInput.SwitchCurrentActionMap("UI");
        else
            PlayerInput.SwitchCurrentActionMap("Player");

        EventSystem.SetSelectedGameObject(EventSystem.firstSelectedGameObject);
        _rectTransform.anchoredPosition = IsGamePaused ? Vector2.zero : _orgPos;
        //PauseScreenPanel.SetActive(IsGamePaused);
        DebugLog(IsGamePaused ? "Pause" : "Unpause" + " Game");
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
