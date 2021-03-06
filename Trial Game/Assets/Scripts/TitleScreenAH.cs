using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreenAH : UIActionHandler
{
    [SerializeField] private Transform StartAt;
    [SerializeField] private Transform MoveTo;
    [SerializeField] private TitleScreen_LevelButton FirstLevel;
    [SerializeField] private UI_PanelFade LevelLoadFade;
    [SerializeField] private AudioClip[] Sounds_LoadLevel;

    private Enums.TitleState _state;
    private UI_ShowLoadScreen _sls;

    public void Show()
    {
        transform.position = MoveTo.position;

        var buttons = GetComponentsInChildren<UIButton>();
        foreach(UIButton button in buttons)
            button.RaiseVolume();
    }

    public override void Awake()
    {
        base.Awake();
        transform.position = StartAt.position;
        _sls = FindObjectOfType<UI_ShowLoadScreen>();
    }

    public override void HandleButtonSubmit(UIButton button)
    {
        var titleButton = (TitleScreen_Button)button;

        switch(titleButton.Type)
        {
            case Enums.UI_TitleButtonType.Load:
                _state = Enums.TitleState.Levels;
                _sls.Show = true;
                FirstLevel.Select();
                break;
            case Enums.UI_TitleButtonType.Quit:
                _sceneManager.QuitGame(button.ClipLength);
                break;
            case Enums.UI_TitleButtonType.Level_Done:
                var levelButton = (TitleScreen_LevelButton)titleButton;
                _audioSource.Play(Sounds_LoadLevel[Random.Range(0,Sounds_LoadLevel.Length - 1)]);
                _sceneManager.LoadScene(levelButton.LevelName, _audioSource.clip.length);
                HideLoadPanel();
                LevelLoadFade.StartFade(_audioSource.clip.length);
                break;
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed)
            HideLoadPanel();
    }

    private void HideLoadPanel()
    {
        if (_state == Enums.TitleState.Levels)
        {
            _state = Enums.TitleState.Main;
            _sls.Show = false;
        }
    }
}
