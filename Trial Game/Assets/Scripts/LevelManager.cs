using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : SceneManager
{
    public Enums.GameState GameState;
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
        PlayerManager.UnitHolder = SideSelection.ConfirmedSide == Enums.PlayerSides.Aztec ? AztecUnitHolder : SpanishUnitHolder;
        EnemyManager.UnitHolder = SideSelection.ConfirmedSide == Enums.PlayerSides.Aztec ? SpanishUnitHolder : AztecUnitHolder;
        PlayerManager.InitializeUnits();
        EnemyManager.InitializeUnits();

        ResultText.SetupText(SideSelection.ConfirmedSide);
        Cursor.AllowMove();
        PlayerInput.SwitchCurrentActionMap("Player");
        CameraControllerScript.enabled = true;
        Destroy(SideSelection.gameObject);
        GameState = Enums.GameState.Play;
    }

    public void FinishScene(UnitManager uM)
    {
        GameState = Enums.GameState.Results;
        if (uM is EnemyManager)
            ResultText.Show(true);
        else
            ResultText.Show(false);

        LoadScene("Title Scene", TimeToLoad, TimeToFade);
    }

    public void StartSelection()
    {
        UIInput.SetSelectedGameObject(SideSelection.gameObject);
    }

    // Start is called before the first frame update
    internal override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();
    }
}
