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
    [SerializeField] private GameObject Flag;
    public AudioClip BackgroundMusic;

    private List<Animator> _unitAnimators;
    private UIRectTransformEditor _rectEditor;
    private RectTransform _flagRectT;
    private float _orgAnchorX;
    private float _orgPosX;
    private float _orgPivotX;

    [SerializeField]
    public bool Selected
    {
        set
        {
            //Text.Edit = UnitPanel.Edit = UnitBackgroundMask.Edit = value;
            Text.Edit(value);
            UnitBackgroundMask.Edit(value);
            UnitMover.SetActive(value);
            UnitPanel.Edit(value);
            ConfirmationPanel.Edit(value);
            _flagRectT.anchorMin = _flagRectT.anchorMax = new Vector2(
                    value ? .5f : _orgAnchorX, // If Flag is selected, set the flag anchor to the middle for expansion
                    .5f);                      // Else, set back to original anchor
            _flagRectT.anchoredPosition = new Vector2(
                value ? 0 : _orgPosX,
                0);
            _flagRectT.pivot = new Vector2(
                value ? .5f : _orgPivotX,
                .5f);
        }
    }

    public bool Confirmed
    {
        set
        {
            _unitAnimators.ForEach(a => a.SetBool("Selected", value));
            ConfirmationPanel.Confirm();
            _rectEditor.Edit();
        }
    }

    private void Awake()
    {
        _rectEditor = Flag.GetComponent<UIRectTransformEditor>();
        _flagRectT = Flag.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _unitAnimators = UnitMover.GetComponentsInChildren<Animator>().ToList();
        _orgAnchorX = _flagRectT.anchorMin.x;
        _orgPosX = _flagRectT.anchoredPosition.x;
        _orgPivotX = _flagRectT.pivot.x;
        UnitMover.SetActive(false);
    }
}
