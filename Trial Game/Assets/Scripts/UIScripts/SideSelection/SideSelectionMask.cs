using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SideSelectionMask : MonoBehaviour
{
    public Enums.PlayerSides PlayerSides;

    [SerializeField] private UIMoveToAndFontSize Text;
    [SerializeField] private UIMoveTo UnitPanel;
    [SerializeField] private UIMaskEditor UnitBackgroundMask;
    [SerializeField] private ConfirmSelectionController ConfirmationPanel;
    [SerializeField] private GameObject UnitMover;

    private List<Animator> _unitAnimators;

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
            _unitAnimators.ForEach(a => a.SetBool("Selected", value));
            ConfirmationPanel.Confirm();
        }
    }

    private void Start()
    {
        _unitAnimators = UnitMover.GetComponentsInChildren<Animator>().ToList();
    }
}
