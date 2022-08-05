using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UnitStatButton : UIButton, IMoveHandler, ICancelHandler
{
    [SerializeField] private Enums.UnitStat Type;
    [SerializeField] private UnitInfoAH Parent;
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private GameObject Explanation;
    [SerializeField] private string ValueUnits;

    public void UpdateValue(UnitInfoButton info)
    {
        Text.text = info.Stats[Type] + ValueUnits;
    }

    private void Start()
    {
        Explanation.SetActive(false);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Explanation.SetActive(false);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
    }

    public override void OnSelect(BaseEventData eventData)
    {
        Explanation.SetActive(true);
        AudioSource.Play(Sound_Select);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
    }

    public void OnMove(AxisEventData eventData)
    {
        if (eventData.moveDir == MoveDirection.Left)
            Parent.CurrentUnitInfo.Select();
    }

    public void OnCancel(BaseEventData eventData)
    {
        Parent.CurrentUnitInfo.Select();
    }
}
