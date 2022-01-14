using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : SceneManager
{
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private CameraController CameraControllerScript;
    [SerializeField] private SideSelectionAH SideSelection;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject AztecUnitHolder;
    [SerializeField] private GameObject SpanishUnitHolder;
    [SerializeField] private UnitManager PlayerManager;
    [SerializeField] private UnitManager EnemyManager;
    [SerializeField] private float CameraPlayZoom;
    [SerializeField] private float CameraSelectionZoom;

    public void StartPlay()
    {
        PlayerManager.UnitHolder = SideSelection.ConfirmedSide == Enums.PlayerSides.Aztec ? AztecUnitHolder : SpanishUnitHolder;
        EnemyManager.UnitHolder = SideSelection.ConfirmedSide == Enums.PlayerSides.Aztec ? SpanishUnitHolder : AztecUnitHolder;
        PlayerManager.InitializeUnits();
        EnemyManager.InitializeUnits();

        Cursor.SetActive(true);
        PlayerInput.SwitchCurrentActionMap("Player");
        CameraControllerScript.enabled = true;
        Destroy(SideSelection.gameObject);
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
