using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitGlance : MonoBehaviour
{
    public Image UnitPortrait;
    public GameObject UnitCooldownBar;
    public GameObject GlanceDeath;
    public float PositionSpeed;
    public float ShakeDistance;
    public float ShakeSpeed;
    public float DeathAngle;
    public float DeathSpeed;

    public UnitGlanceHandler Handler { get; set; }
    public float CooldownTimer { get; private set; }
    public float Height { get { return _recT.sizeDelta.y; } }

    private RectTransform _recT;
    private RectTransform _barRect;
    private Image _barImage;
    private Queue<Vector2> _shakePoints;
    private Vector3 _deathTilt;
    private Vector2 _origin;
    private Vector2 _moveToPoint;
    private Vector2 _shakePoint;
    private Vector2 _deathPoint;
    private bool _moveToOrigin;
    private bool _shake;
    private bool _death;
    private bool _start;
    private float _cooldownBarOrgWidth;
    private float _cooldownBarSpeed;
    private float _deathTime;

    public void SetOrigin(Vector2 newOrigin)
    {
        if (_origin != newOrigin)
        {
            _origin = _moveToPoint = newOrigin;
            Debug.Log($"{gameObject.name} start pos {_moveToPoint}");
            _start = true;
        }
    }

    public void Cooldown(float cooldownTimer)
    {
        CooldownTimer = cooldownTimer;
        _barRect.sizeDelta = new Vector2(0, _barRect.sizeDelta.y);
        _cooldownBarSpeed = _cooldownBarOrgWidth / CooldownTimer;
    }

    public void Hurt(float lifePercentage, float damage)
    {
        Color newColor;
        if (lifePercentage >= .66f)
            newColor = Colors.Health_Full;
        else if (lifePercentage >= .33f)
            newColor = Colors.Health_Half;
        else
            newColor = Colors.Health_Low;
        _barImage.color = newColor;

        float severity = 0;
        if (damage < 3)
            severity = 1;
        else if (damage < 5)
            severity = 2;
        else
            severity = 3;

        DamageShake(severity);

        if (severity == 3)
            Death();
    }

    public void Death()
    {
        _death = true;
        float rotZ = Random.Range(5, DeathAngle);
        if ((int)(Time.deltaTime * 10000) % 2 == 0)
            rotZ = -rotZ;
        Debug.Log(rotZ);
        _deathTilt = new Vector3(0, 0, rotZ);
        _deathPoint = new Vector2(_recT.anchoredPosition.x, -(Handler.Height + _recT.sizeDelta.y));
        var skull = Instantiate(GlanceDeath, Vector3.zero, Quaternion.identity, Handler.gameObject.transform);
        UnitGlanceDeath ugd = skull.GetComponent<UnitGlanceDeath>();
        ugd.SetPosition(new Vector2(
            _origin.x + _recT.sizeDelta.x / 2,
            _origin.y - _recT.sizeDelta.y / 2));
    }
    public void Reset()
    {
        _barImage.color = Colors.Health_Full;
        _barRect.sizeDelta = new Vector2(
            _cooldownBarOrgWidth,
            _barRect.sizeDelta.y);
        _moveToOrigin = false;
        _shake = false;
        _death = false;
        _recT.anchoredPosition = _origin;
        _recT.eulerAngles = Quaternion.identity.eulerAngles;
        _deathTime = 0;
    }

    private void Awake()
    {
        _recT = GetComponent<RectTransform>();
        _barRect = UnitCooldownBar.GetComponent<RectTransform>();
        _barImage = UnitCooldownBar.GetComponent<Image>();
        
        _cooldownBarOrgWidth = _barRect.sizeDelta.x;
        _shakePoints = new Queue<Vector2>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_start)
        {
            Vector2 newPos = Vector2.MoveTowards(_recT.anchoredPosition, _moveToPoint + _shakePoint, PositionSpeed * Time.deltaTime);
            _recT.anchoredPosition = newPos;

            if (_recT.anchoredPosition == _moveToPoint)
                _start = false;
        }

        if(_barRect.sizeDelta.x < _cooldownBarOrgWidth && !_death)
        {
            float moveBar = _barRect.sizeDelta.x + (_cooldownBarSpeed * Time.deltaTime);
            moveBar = Mathf.Clamp(moveBar, 0, _cooldownBarOrgWidth);

            CooldownTimer -= Time.deltaTime;

            _barRect.sizeDelta = new Vector2(moveBar, _barRect.sizeDelta.y);
        }

        if(_shake)
        {
            Vector2 moveTo = Vector2.MoveTowards(
                _recT.anchoredPosition, _shakePoint, ShakeSpeed * Time.deltaTime);
            _recT.anchoredPosition = moveTo;

            if(_recT.anchoredPosition == _shakePoint)
            {
                if(_moveToOrigin)
                {
                    _moveToOrigin = false;
                    _shakePoint = _shakePoints.Dequeue();
                }
                else
                {
                    _moveToOrigin = true;
                    _shakePoint = _origin;
                }

                if (_recT.anchoredPosition == _origin && _shakePoints.Count == 0)
                    _shake = false;
            }
        }
        else if(_death)
        {
            Vector3 curAngle;
            if (_deathTilt.z < 0 && _recT.eulerAngles.z > 180)
                curAngle = new Vector3(0, 0, _recT.eulerAngles.z - 360);
            else
                curAngle = _recT.eulerAngles;

            Debug.Log(curAngle.z);
            Vector3 newV3 = Vector3.MoveTowards(curAngle, _deathTilt, DeathSpeed);
            _recT.eulerAngles = newV3;

            newV3 = Vector2.MoveTowards(_recT.anchoredPosition, _deathPoint, DeathSpeed + (_deathTime * 9.8f));
            _recT.anchoredPosition = newV3;

            if (_recT.anchoredPosition == _deathPoint)
                _death = false;// Destroy(gameObject);

            _deathTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Create points to shake the unit glance based on the severity of damage taken
    /// </summary>
    /// <param name="severity">The rank (1-3) of the damage taken</param>
    private void DamageShake(float severity)
    {
        float shakeSev = severity * ShakeDistance;  // set how far to shake based on severity
        float minShake = .75f;                       // minimum amount of shake in %
        float maxShake = 1;                          // maximum amount of shake in %
        int numofPoint = 10;                          // how many shake points to create

        for(int i = 0; i < numofPoint; i++)
        {
            float pointX = CreateShakePoint(shakeSev, minShake, maxShake);
            float pointY = _recT.anchoredPosition.y + CreateShakePoint(shakeSev, maxShake, minShake);

            Vector2 newShakePoint = new Vector2(pointX, pointY);
            _shakePoints.Enqueue(newShakePoint);

            shakeSev -= (shakeSev * (i+1) / numofPoint);  // decrease the severity
        }

        _shakePoint = _shakePoints.Dequeue();
        _shake = true;
    }

    private float CreateShakePoint(float shakeSev, float minShake, float maxShake)
    {
        float shake = Random.Range(-shakeSev, shakeSev);
        if (shake < 0)
            shake = Mathf.Clamp(shake, -shakeSev * minShake, -shakeSev * maxShake);
        else
            shake = Mathf.Clamp(shake, shakeSev * minShake, shakeSev * maxShake);

        return shake;
    }
}
