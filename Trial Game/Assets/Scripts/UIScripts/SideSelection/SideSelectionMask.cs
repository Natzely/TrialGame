using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SideSelectionMask : MonoBehaviour
{
    public Enums.PlayerSides PlayerSides;

    [SerializeField] private UIMoveToAndFontSize Text;
    [SerializeField] private UIMoveTo UnitPanel;
    [SerializeField] private UIMaskEditor UnitBackgroundMask;
    [SerializeField] private ConfirmSelectionController ConfirmationPanel;

   [SerializeField]
    public bool Selected 
    {
        set 
        {
            //Text.Edit = UnitPanel.Edit = UnitBackgroundMask.Edit = value;
            Text.Edit(value);
            UnitBackgroundMask.Edit(value);
            UnitPanel.Edit(value);
            ConfirmationPanel.Edit(value);
        }
    }

    public bool Confirmed
    {
        set
        {
            Debug.Log("Side chosen");
        }
    }
}
