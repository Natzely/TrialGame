using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[SerializeField] public class MoveTextEvent : UnityEvent { }

public class TitleTextMove : MonoBehaviour
{
    [SerializeField] private Vector2 StartAt;
    [SerializeField] private Vector2 MoveTo;
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float WaitTimer;
    [Tooltip("Percentage to how close the text is to its destination")]
    [SerializeField] [Range(0, 1)] private float EventActivation = 1;
    public UnityEvent AfterMoveEvent;

    private AudioSource _audioSource;
    private RectTransform _rectT;
    private Vector2 _currentPos;
    private bool _eventTriggered;
    private bool _soundTriggered;
    private float _waitTime;
    private float _orgDistance;
    private float _currDis;
    private float _perDone;

    private void Awake()
    {

        _rectT = GetComponent<RectTransform>();
        _audioSource = GetComponent<AudioSource>();
        _rectT.anchoredPosition = _currentPos = StartAt;
        _waitTime = WaitTimer;
        _orgDistance = Vector2.Distance(MoveTo, StartAt);
    }

    // Update is called once per frame
    void Update()
    {
        if(_currentPos != MoveTo && _waitTime <= 0)
        {
            var newPos = Vector2.MoveTowards(_currentPos, MoveTo, MoveSpeed * Time.deltaTime);
            _rectT.anchoredPosition = _currentPos = newPos;
            _currDis = Vector2.Distance(MoveTo, _currentPos);
            _perDone = 1 - (_currDis / _orgDistance);
        }

        if (AfterMoveEvent != null && _perDone >= EventActivation && !_eventTriggered)
        {
            Debug.Log("Done");
            _eventTriggered = true;
            AfterMoveEvent.Invoke();
        }

        if (_waitTime > 0)
            _waitTime -= Time.deltaTime;
        else if (_waitTime <= 0 && !_soundTriggered)
        {
            _audioSource.Play();
            _soundTriggered = true;
        }
    }
}
