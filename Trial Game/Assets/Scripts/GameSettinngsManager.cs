using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettinngsManager : MonoBehaviour
{
    public static GameSettinngsManager Instance { get; private set; }

    public Enums.Language Language = Enums.Language.English;
    public float UnitSpeedModifier = 1;
    public float UnitCooldownModifier = 1;
    public float GameSpeed = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
        DontDestroyOnLoad(this);
        Time.timeScale = GameSpeed;
    }
}
