using UnityEngine;
using TMPro;

public class Damage_TextEffect : MonoBehaviour
{
    public TextMeshProUGUI DamageText;
    public float MoveDistance;
    public float MoveSpeed;
    public float MinMoveSpeed;
    public float MovePercentage;

    public string Text { get; set; }

    private Color _color;
    private Vector2 _startPos;
    private Vector2 _endPos;
    private bool _initialize;
    private float _maxMoveSpeed;
    private float _percentToHide;


    private void Awake()
    {
        if (!_initialize)
        {
            _color = DamageText.color;
            _startPos = transform.position;
            _endPos = _startPos + (Vector2.up * MoveDistance);
            _initialize = true;
            _maxMoveSpeed = MoveSpeed;
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        transform.position = _startPos;
        DamageText.text = Text;
        DamageText.color = new Color(_color.r, _color.g, _color.b, 255);
        _percentToHide = 1 - MovePercentage;
    }

    private void Update()
    {
        var newPos = Vector2.MoveTowards(transform.position, _endPos, MoveSpeed * Time.deltaTime);
        transform.position = newPos;

        var disMoved = _endPos.y - newPos.y;
        var percentLeft = disMoved / MoveDistance;
        MoveSpeed = Mathf.Clamp(_maxMoveSpeed * percentLeft, MinMoveSpeed, _maxMoveSpeed);
        if (percentLeft <= MovePercentage)
        { 
            var alphaPercent = percentLeft / MovePercentage;
            DamageText.color = new Color(_color.r, _color.g, _color.b, alphaPercent);
            if (percentLeft <= 0)
                gameObject.SetActive(false);
        }
    }
}
