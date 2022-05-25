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
    internal AudioSource _audioSource;
    internal EventSystem _eventSystem;

    public abstract void HandleButtonSubmit(UIButton button);

    protected virtual void Awake()
    {
        _currentButton = FirstSelected;
        _audioSource = GetComponent<AudioSource>();
        _eventSystem = FindObjectOfType<EventSystem>();
    }

    protected virtual void Start()
    {
        //_currentButton.Select();
    }

    public virtual void OnItemSelected(UIButton button)
    {
        if (button != _currentButton)
        {
            _currentButton = button;
        }
    }
}