using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class UIObjectEditor : MonoBehaviour
{
    [SerializeField] internal bool EditOnStart = false;
    [SerializeField] internal bool Loop;
    [SerializeField] internal float Speed = 1;
    [SerializeField] internal float Acceleration = 1;
    [SerializeField] internal float Decceleration = 1;
    [Tooltip("At what percent should you start slowing down")]
    [SerializeField] [Range(0, 1)] internal float SlowDownPercent = 1;
    [SerializeField] internal float MinSpeed;
    [SerializeField] internal float MaxSpeed;
    [Tooltip("Time to wait before edit begins in seconds")]
    [SerializeField] internal float WaitTime;
    [SerializeField] internal UnityEvent EditEvent;

    internal float DistancePer { get => 1 - (_curDistance / _distance); }

    internal bool _edit { get; private set; }
    internal float _speed;
    internal float _curDistance;
    internal float _distance;

    private bool _startWaitTimer;
    private float _waitTimer;

    public virtual void Edit(bool edit = true)
    {
        if (edit && WaitTime > 0)
        {
            _startWaitTimer = true;
            _waitTimer = WaitTime;
        }
        else
            _edit = edit;

        //if (!edit)
        //    Reset();
    }

    public void UpdateSpeed(float newSpeed)
    {
        Speed = newSpeed;
    }

    /// <inheritdoc Make sure to set _curDistance and _distance to the difference of the main variable being edited/>>
    /// <summary>
    /// Make sure to set _curDistance and _distance to the difference of the main variable being edited
    /// </summary>
    internal virtual void Start()
    {
        _waitTimer = WaitTime;
        _speed = Speed;
        if (EditOnStart)
            Edit();
    }

    internal virtual void Update()
    {
        if (_waitTimer > 0 && _startWaitTimer)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer < 0)
                _edit = true;
        }
    }

    /// <summary>
    /// Make sure to update _curDistance;
    /// </summary>
    internal virtual void EditObject()
    {
        if (DistancePer >= SlowDownPercent)
            _speed *= Decceleration;
        else
            _speed *= Acceleration;
        _speed = Mathf.Clamp(_speed, MinSpeed, MaxSpeed);
    }

    internal virtual void Reset()
    {
        _speed = Speed;
        _edit = false;
        _waitTimer = WaitTime;
        _startWaitTimer = false;
    }
}
