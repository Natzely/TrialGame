using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmSelectionController : MonoBehaviour, ILog
{
    [SerializeField] private UIMaskEditor MaskEditor;
    [SerializeField] private UICanvasGroupEditor CanvasEditor;
    [SerializeField] private UIButton CancelButton;

    public bool Showing { get { return _edit; } }

    private bool _edit;

    public void Edit(bool edit)
    {
        Log(edit ? "Showing" : "Hiding");
        if(edit)
            CancelButton.Select(true);
        _edit = edit;
        MaskEditor.Edit(edit);
        CanvasEditor.Edit(edit);
    }

    public void Confirm()
    {
        CanvasEditor.AlphaEdit = 0;
    }

    public void Log(string msg)
    {
        DebugLogger.Instance?.Log(msg);
    }

    public void LogError(string msg)
    {
        throw new System.NotImplementedException();
    }
}
