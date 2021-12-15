using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public abstract class UIActionHandler : UIObject
{
    public GameObject CurrentButton { get { return _eventSystem.currentSelectedGameObject; } }
    [SerializeField] internal PlayerInput PlayerInput;
    [SerializeField] internal UIButton FirstSelected;
    [SerializeField] internal AudioClip Sound_Enter;
    [SerializeField] internal AudioClip Sound_Exit;

    internal UIButton _currentButton;
    internal UIButton _prevButton;
    internal SceneManager _sceneManager;
    internal AudioSource _audioSource;
    internal EventSystem _eventSystem;

    public abstract void HandleButtonSubmit(UIButton type);

    public override void Awake()
    {
        base.Awake();
        _currentButton = FirstSelected;
        _audioSource = GetComponent<AudioSource>();
        _sceneManager = FindObjectOfType<SceneManager>();
        _eventSystem = FindObjectOfType<EventSystem>();
    }

    public void OnItemSelected(UIButton button)
    {
        if (button != _currentButton)
        {
            _currentButton = button;
        }
    }
}