using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmSelectionController : MonoBehaviour
{
    [SerializeField] private UIMaskEditor MaskEditor;
    [SerializeField] private UICanvasGroupEditor CanvasEditor;

    public void Edit(bool edit)
    {
        MaskEditor.Edit(edit);
        CanvasEditor.Edit(edit);
    }
}
