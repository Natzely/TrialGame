using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public abstract class UIActionHandler : UIObject
{
    
    public UIButton CurrentButton { get { return _eventSystem.currentSelectedGameObject.GetComponent<UIButton>(); } }
    [SerializeField] internal PlayerInput PlayerInput;
    [SerializeField] internal UIButton FirstSelected;
    [SerializeField] internal AudioClip Sound_Enter;
    [SerializeField] internal AudioClip Sound_Exit;

    internal UIButton _currentButton;
    internal UIButton _prevButton;
    internal AudioSource _audioSource;
    internal EventSystem _eventSystem;

    public void SelectCurrentButton(bool silent) 
    {
        _currentButton.SilentSelect = silent;
        _eventSystem.SetSelectedGameObject(_currentButton.gameObject); 
    }

    public abstract void HandleButtonSubmit(UIButton button);

    protected virtual void Awake()
    {
        _currentButton = FirstSelected;
        _audioSource = GetComponent<AudioSource>();
        _eventSystem = FindObjectOfType<EventSystem>();
    }

    public virtual void OnItemSelected(UIButton button)
    {
        if (button != _currentButton)
        {
            _currentButton = button;
        }
    }
}