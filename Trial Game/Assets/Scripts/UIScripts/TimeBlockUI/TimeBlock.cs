using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBlock : MonoBehaviour
{
    [SerializeField] private TimeStopHandler TimeStopHandler;
    public TimeBlock BlockBehind;
    public TimeBlock BlockAhead;
    [SerializeField] private RectTransform MaskTransform;
    public float BlockPowerTime;

    private LevelManager LevelManager { get { return (LevelManager)SceneManager.Instance; } }
    public bool IsTimeFull { get { return CurrentTime == BlockPowerTime; } }
    public bool IsTimeEmpty { get { return CurrentTime == 0; } }
    public bool FillBlock { get; set; }
    public float CurrentTime { get; private set; }

    private float _orgMaskX;

    public float AddTime(float addTime)
    {
        float extraTime = 0;

        CurrentTime += addTime;

        if (CurrentTime >= BlockPowerTime)
        {
            extraTime = CurrentTime - BlockPowerTime;
            CurrentTime = BlockPowerTime;
        }

        SetMaskSize();
        return extraTime;
    }

    public float SubtractTime(float subTime)
    {
        float extraTime = 0;

        CurrentTime -= subTime;

        if (CurrentTime <= 0)
        {
            extraTime = 0 - CurrentTime;
            CurrentTime = 0;
        }

        SetMaskSize();
        return extraTime;
    }

    private void Awake()
    {
        CurrentTime = BlockPowerTime;
        FillBlock = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!BlockAhead)
            TimeStopHandler.CurrentBlock = this;
        _orgMaskX = MaskTransform.sizeDelta.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.GameState == Enums.GameState.TimeStop && TimeStopHandler.CurrentBlock == this)
        {
            CurrentTime -= Time.unscaledDeltaTime;

            if(CurrentTime <= 0)
            {
                CurrentTime = 0;
                if (BlockBehind)
                {
                    TimeStopHandler.CurrentBlock = BlockBehind;
                    FillBlock = false;
                }
                else
                {
                    TimeStopHandler.TimeStop(false);
                }
            }
            SetMaskSize();
        }
        else if(LevelManager.GameState == Enums.GameState.Play)
        {
            if (TimeStopHandler.CurrentBlock == this && !IsTimeFull && FillBlock)
            {
                CurrentTime += Time.deltaTime * TimeStopHandler.RechargeSpeedMod;
                if(CurrentTime >= BlockPowerTime)
                {
                    CurrentTime = BlockPowerTime;
                }
                SetMaskSize();
            }
        }
    }

    private void SetMaskSize()
    {
        float sizePer = CurrentTime / BlockPowerTime;
        Vector2 newSize = new Vector2(_orgMaskX * sizePer, MaskTransform.sizeDelta.y);
        MaskTransform.sizeDelta = newSize;
    }
}
