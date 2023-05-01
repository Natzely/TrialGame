using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTestMode : MonoBehaviour
{
    public bool TestMode;
    public Enums.PlayerSides Side;
    public Enums.Language Language;
    public GameObject FadeIn;
    public GameObject SideSelection;
    public Cursor_Move CursorMove;

    private void Awake()
    {
        FadeIn.SetActive(!TestMode);
        SideSelection.SetActive(!TestMode);
        CursorMove.enabled = TestMode;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (TestMode)
        {
            GameSettinngsManager.Instance.Language = Language;
            LevelManager.Instance.StartPlay(Side);
        }
    }
}
