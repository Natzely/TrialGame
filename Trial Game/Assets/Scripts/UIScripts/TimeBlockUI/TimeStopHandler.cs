using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class TimeStopHandler : MonoBehaviour
{
    [SerializeField] private GameObject Background;
    [SerializeField] private float TimeSlow;
    public float RechargeSpeedMod;
    [SerializeField] private float ViolenceFill;
    [SerializeField] private float DeathFill;
    [SerializeField] private float TimeActionReduction;
    [SerializeField] private AudioClip AC_StopTime;
    [SerializeField] private AudioClip AC_StartTime;
    [SerializeField] private AudioClip AC_TimeStopped;

    public static TimeStopHandler Instance { get; private set; }
    public TimeBlock CurrentBlock { get; set; }
    public bool TimeStopped { get; private set; }

    private LevelManager LevelManager { get { return (LevelManager)SceneManager.Instance; } }

    private AudioSource _aS;

    public void TimeStop(bool stopTime)
    {
        TimeStopped = stopTime;
        LevelManager.GameState = stopTime ? Enums.GameState.TimeStop : Enums.GameState.Play;
        Time.timeScale = stopTime ? TimeSlow : 1;
        _aS.loop = false;
        _aS.Play(stopTime ? AC_StopTime : AC_StartTime);
        if (stopTime)
            StartCoroutine(PlayMidClip());
        Background.SetActive(stopTime);
    }

    public void Violence(float timeToAdd = 0)
    {
        if (timeToAdd == 0)
            timeToAdd = ViolenceFill;

        if (CurrentBlock.IsTimeFull && CurrentBlock.BlockAhead)
        {
            CurrentBlock = CurrentBlock.BlockAhead;
            CurrentBlock.FillBlock = true;
            if (timeToAdd == DeathFill)
            {
                CurrentBlock.AddTime(ViolenceFill);
            }
        }
        else
            CurrentBlock.AddTime(ViolenceFill);
    }

    public void Death()
    {
        Violence(DeathFill);
    }

    public void TimeAction()
    {
        if(LevelManager.GameState == Enums.GameState.TimeStop)
        {
            float extraTime = CurrentBlock.SubtractTime(TimeActionReduction);
            if (CurrentBlock.IsTimeEmpty && CurrentBlock.BlockBehind)
            {
                CurrentBlock = CurrentBlock.BlockBehind;
                CurrentBlock.SubtractTime(extraTime);
            }
            else if (CurrentBlock.IsTimeEmpty)
                TimeStop(false);
        }
    }

    public void SetActive()
    {
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        _aS = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Background.SetActive(false);
    }

    public void StopTimeKeyHandler(InputAction.CallbackContext context)
    {
        if (context.performed &&
            (LevelManager.GameState == Enums.GameState.TimeStop || LevelManager.GameState == Enums.GameState.Play))
        {
            TimeStop(!TimeStopped);
        }
    }

    IEnumerator PlayMidClip()
    {
        yield return new WaitUntil(() => !_aS.isPlaying);
        if (LevelManager.GameState == Enums.GameState.TimeStop)
        {
            _aS.loop = true;
            _aS.Play(AC_TimeStopped);
        }
    }
}
