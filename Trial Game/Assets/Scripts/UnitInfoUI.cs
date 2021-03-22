using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Runtime.InteropServices;

public class UnitInfoUI : MonoBehaviour
{
    [SerializeField] private PauseScreen PauseScreen;
    public Sprite[] UnitSprites;
    public RectTransform Transform;
    public Image UnitImage;
    public TextMeshProUGUI UnitName;
    public TextMeshProUGUI UnitInfo;
    public TextMeshProUGUI UnitCons;
    public TextMeshProUGUI UnitPros;
    public float Speed;

    private CursorController _cursor;
    private Image _image;
    private UnitController _currentUnit;
    private Vector2 _shownVector;
    private bool _visible;
    private bool _showing;

    public void ShowUnitInfo(InputAction.CallbackContext context)
    {   
        if (!PauseScreen.IsGamePaused)
        {
            if (_visible && context.performed)
            {
                Debug.Log("Hide unit info");
                Transform.anchoredPosition = new Vector2(Transform.rect.width, Transform.anchoredPosition.y);
                _visible = false;
            }
            else if (!_visible && _cursor.CurrentGridBlock.CurrentUnit != null && context.performed)
            {
                Debug.Log("Show unit info");
                _currentUnit = _cursor.CurrentGridBlock.CurrentUnit;
                int type = (int)(_currentUnit.Type);

                _image.color = _currentUnit == null ? Colors.UnitInfo_Blank : _currentUnit.Player == Enums.Player.Player1 ? Colors.UnitInfo_Friendly : Colors.UnitInfo_Enemy;
                UnitImage.sprite = UnitSprites[type];
                UnitName.text = _unitName[type];
                UnitInfo.text = _unitInfo[type];
                UnitPros.text = _unitPros[type];
                UnitCons.text = _unitCons[type];

                _showing = true;
            }
        }
    }

    void Awake()
    {
        _image = GetComponent<Image>();
        _cursor = FindObjectOfType<CursorController>();
        _shownVector = new Vector2(0, Transform.anchoredPosition.y);
    }

    void Update()
    {
        if(_visible && _cursor.CurrentGridBlock.CurrentUnit != _currentUnit)
        {
            _currentUnit = _cursor.CurrentGridBlock.CurrentUnit;
            int type = (_currentUnit == null ? UnitSprites.Length - 1 : (int)_currentUnit.Type);

            _image.color = _currentUnit == null ? Colors.UnitInfo_Blank : _currentUnit.Player == Enums.Player.Player1 ? Colors.UnitInfo_Friendly : Colors.UnitInfo_Enemy;
            UnitImage.sprite = UnitSprites[type];
            UnitName.text = _unitName[type];
            UnitInfo.text = _unitInfo[type];
            UnitPros.text = _unitPros[type];
            UnitCons.text = _unitCons[type];
        }

        if(_showing && Transform.position.x >= 0)
        {
            Vector2 moveVector = Vector2.MoveTowards(Transform.anchoredPosition, _shownVector, Speed * Time.deltaTime);
            Transform.anchoredPosition = moveVector;

            if(Transform.anchoredPosition == _shownVector)
            {
                _showing = false;
                _visible = true;
            }
        }
    }

    private string[] _unitName = new string[]
    {
        "Melee",
        "Range",
        "Horse",
        "",
    };

    private string[] _unitInfo = new string[]
    {
                                                                                                                                                     // Max Length   
        "The melee unit are good in squads. They cooldown faster with others around and deal more to a horse unit if three melees hit it in a row.",
        "The range unit can attack from pretty far away and pack a punch. They have low health so try to keep them away from direct battle.",
        "The horse unit is fast and ignores terrain cost. Can move again after attacking if it has movement left (max of 3 spaces).",
        "",
    };

    private string[] _unitPros = new string[]
    {
                      // Max Length, Max of 4 items
        "Low Cooldown\n" +
        "Good Defense",

        "Long Range\n" +
        "Fast",

        "High stats\n" +
        "Very Fast\n" +
        "Hides a Spear",

        "",
    };

    private string[] _unitCons = new string[]
    {
                      // Max Length, Max of 4
        "Melee Range\n" +
        "Low Damage",

        "Low Health\n" +
        "High CD",

        "Very High CD\n" +
        "Melee Range",

        "",
    };
}
