using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : SceneManager
{
    public Enums.GameState GameState;
    public Enums.PlayerSides Side;
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private CameraController CameraControllerScript;
    [SerializeField] private SideSelectionAH SideSelection;
    [SerializeField] private CursorController Cursor;
    [SerializeField] private GameObject AztecUnitHolder;
    [SerializeField] private GameObject SpanishUnitHolder;
    [SerializeField] private UnitManager PlayerManager;
    [SerializeField] private UnitManager EnemyManager;
    [SerializeField] private ResultText ResultText;
    [SerializeField] private float CameraPlayZoom;
    [SerializeField] private float CameraSelectionZoom;

    public void StartPlay()
    {
        Side = SideSelection.ConfirmedSide;
        PlayerManager.UnitHolder = Side == Enums.PlayerSides.Aztec ? AztecUnitHolder : SpanishUnitHolder;
        EnemyManager.UnitHolder = Side == Enums.PlayerSides.Aztec ? SpanishUnitHolder : AztecUnitHolder;
        PlayerManager.InitializeUnits();
        EnemyManager.InitializeUnits();

        //ResultText.SetupText(SideSelection.ConfirmedSide);
        Cursor.AllowMove();
        PlayerInput.SwitchCurrentActionMap("Player");
        CameraControllerScript.enabled = true;
        Destroy(SideSelection.gameObject);
        GameState = Enums.GameState.Play;
    }

    public void StartMusic(float volume, float delay)
    {
        _audioSource.volume = volume;
        _audioSource.clip = SideSelection.SelectedBGM;
        _audioSource.PlayDelayed(delay);
    }

    public void RestartScene()
    {
        LoadScene(_currentScene);
    }

    public void FinishScene(UnitManager uM)
    {
        GameState = Enums.GameState.Results;
        if (uM is EnemyManager)
            ResultText.Show(true, Side);
        else
            ResultText.Show(false, Side);

        LoadScene("Title Scene", TimeToLoad, TimeToFade);
    }

    public void StartSelection()
    {
        UIInput.SetSelectedGameObject(SideSelection.gameObject);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
