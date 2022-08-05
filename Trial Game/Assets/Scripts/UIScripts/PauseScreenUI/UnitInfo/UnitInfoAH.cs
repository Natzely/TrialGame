using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UnitInfoAH : UIActionHandler, ICancelHandler
{
    [SerializeField] private PauseScreenAH Parent;
    [SerializeField] private GameObject MainButtons;
    [SerializeField] private Animator UnitAnimator;
    [SerializeField] private RectTransform UnitAnimatorRT;
    [SerializeField] private TextMeshProUGUI UnitText;

    public UnitInfoButton CurrentUnitInfo { get; set; }
    public UnitStatButton CurrentUnitStat { get; set; }

    private List<UnitInfoButton> _unitInfoList;
    private List<UnitStatButton> _unitStatList;


    public void Show(Enums.UnitInfo type = Enums.UnitInfo.Warrior)
    {
        gameObject.SetActive(true);
        MainButtons.SetActive(false);
        CurrentUnitStat = _unitStatList.FirstOrDefault();
        CurrentUnitInfo = _unitInfoList.FirstOrDefault(u => u.Type == type);
        CurrentUnitInfo.SilentSelect = true;
        CurrentUnitInfo.Select();
    }

    public void UpdateInfo()
    {
        UnitAnimator.runtimeAnimatorController = CurrentUnitInfo.Animator;
        UnitAnimatorRT.offsetMax = new Vector2(-CurrentUnitInfo.ImageSize.z, -CurrentUnitInfo.ImageSize.y);
        UnitAnimatorRT.offsetMin = new Vector2(CurrentUnitInfo.ImageSize.x, CurrentUnitInfo.ImageSize.w);
        var rot = UnitAnimatorRT.rotation;
        UnitAnimatorRT.rotation = new Quaternion(rot.x, CurrentUnitInfo.Flip ? 180 : 0, rot.z, rot.w);
        UnitText.text = CurrentUnitInfo.LanguageTexts[(int)GameSettinngsManager.Instance.Language];
        _unitStatList.ForEach(s => s.UpdateValue(CurrentUnitInfo));
    }

    public void PlaySelectSound()
    {
        _audioSource.Play(Sound_Enter);
    }

    protected override void Awake()
    {
        base.Awake();

        _unitInfoList = new List<UnitInfoButton>();
        _unitStatList = new List<UnitStatButton>();
    }

    void Start()
    {
        GetComponentsInChildren(_unitInfoList);
        GetComponentsInChildren(_unitStatList);
        gameObject.SetActive(false);
    }

    public override void HandleButtonSubmit(UIButton button)
    {
        throw new System.NotImplementedException();
    }

    public void OnCancel(BaseEventData eventData)
    {
        MainButtons.SetActive(true);
        gameObject.SetActive(false);
        Parent.SelectCurrentButton(true);
    }

    public override void OnItemSelected(UIButton button)
    {
        base.OnItemSelected(button);
        CurrentUnitInfo = button as UnitInfoButton;
        UpdateInfo();
    }
}
