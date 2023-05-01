using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitGlance : MonoBehaviour
{
    public Image UnitPortrait;
    public GameObject MovingIcon;
    public GameObject UnitCooldownBar;
    public GameObject GlanceDeath;
    public bool ShakeAndMove;
    public float PositionSpeed;
    public float ShakeDistance;
    public float ShakeSpeed;
    public float ShakeAgainstModifier;
    public float ShakeTowardsModifier;
    public float DeathAngle;
    public float DeathAngleSpeed;
    public float DeathSpeed;

    public UnitGlanceHandler Handler { get; set; }
    public float CooldownPercent 
    {
        get { return _cooldownTimer / _cooldown; }
    }
    public float Height { get { return _recT.sizeDelta.y; } }

    private RectTransform _recT;
    private RectTransform _barRect;
    private Image _barImage;
    private Queue<Vector2> _shakePoints;
    private Vector3 _deathTilt;
    private Vector2 _origin;
    private Vector2 _moveToPoint;
    private Vector2 _currPos;
    private Vector2 _shakeDelta;
    private Vector2 _deathPoint;
    private bool _moveToOrigin;
    private bool _shake;
    private bool _death;
    private bool _newMovePoint;
    private float _cooldown;
    private float _cooldownTimer;
    private float _cooldownBarOrgWidth;
    private float _cooldownBarSpeed;
    private float _deathTime;
    private float _moveSpeed;
    private int _severity;

    public void SetOrigin(Vector2 newOrigin)
    {
        if (_origin != newOrigin)
        {
            _origin = _moveToPoint = newOrigin;
            _newMovePoint = true;
            _moveSpeed = PositionSpeed;
        }
    }

    public void SetPosition(Vector2 newPosition)
    {
        if(_moveToPoint != newPosition && !_death)
        {
            _moveToPoint = newPosition;
            _newMovePoint = true;
            _moveSpeed = PositionSpeed;
        }
    }

    public void UnitMoving(bool moving)
    {
        MovingIcon.SetActive(moving);
    }

    public void StartCooldown(float cooldownTimer)
    {
        _cooldown = cooldownTimer;
        _cooldownTimer = 0;
        _barRect.sizeDelta = new Vector2(0, _barRect.sizeDelta.y);
        _cooldownBarSpeed = _cooldownBarOrgWidth / _cooldown;
        _newMovePoint = true;
    }

    public void TakeDamage(float lifePercentage, float damage)
    {
        Color newColor;
        if (lifePercentage >= .66f)
            newColor = Colors.Health_Full;
        else if (lifePercentage >= .33f)
            newColor = Colors.Health_Half;
        else
            newColor = Colors.Health_Low;
        _barImage.color = newColor;

        if (damage < 3)
            _severity = 1;
        else if (damage < 5)
            _severity = 2;
        else
            _severity = 3;

        DamageShake();

        //if (_severity == 3)
        //    Death();
    }

    public void Death()
    {
        _death = true;
        if (!_shake || !ShakeAndMove)   // Wait till shaking is over to stop movement
        {
            Handler.RemoveGlance(this);
            _newMovePoint = false;
        }

        float rotZ = Random.Range(5, DeathAngle);
        if ((int)(Time.deltaTime * 10000) % 2 == 0)
            rotZ = -rotZ;
        _deathTilt = new Vector3(0, 0, rotZ);
        _deathPoint = new Vector2(_recT.anchoredPosition.x, -(Handler.Height + _recT.sizeDelta.y));

        if (!_newMovePoint)
            CreateSkull();

    }

    public void Reset()
    {
        _barImage.color = Colors.Health_Full;
        _barRect.sizeDelta = new Vector2(
            _cooldownBarOrgWidth,
            _barRect.sizeDelta.y);
        _newMovePoint = false;
        _moveToOrigin = false;
        _shake = false;
        _death = false;
        _recT.anchoredPosition = _moveToPoint = _currPos = _origin;
        _shakeDelta = Vector2.zero;
        _recT.eulerAngles = Quaternion.identity.eulerAngles;
        _deathTime = 0;
        _cooldown = _cooldownTimer = 0;
    }

    private void Awake()
    {
        _recT = GetComponent<RectTransform>();
        _barRect = UnitCooldownBar.GetComponent<RectTransform>();
        _barImage = UnitCooldownBar.GetComponent<Image>();
        
        _cooldownBarOrgWidth = _barRect.sizeDelta.x;
        _shakePoints = new Queue<Vector2>();
        _cooldown = _cooldownTimer = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_newMovePoint && (!_shake || ShakeAndMove))
        {
            Vector2 newPos = Vector2.MoveTowards(_currPos, _moveToPoint, _moveSpeed * Time.deltaTime);

            if (ShakeAndMove) // If it's okay to move while shaking
            {
                float moveYDiff = newPos.y - _currPos.y; // Grab how much movement was done.
                _currPos = newPos; // Save what the location would be without the shaking
                float newAnchorY = _recT.anchoredPosition.y + moveYDiff; // Calculate new y position
                _recT.anchoredPosition = new Vector2(_recT.anchoredPosition.x, newAnchorY); // Assign the actual position with the shake
            }
            else
                _recT.anchoredPosition = _currPos = newPos;

            if (_currPos == _moveToPoint)
                _newMovePoint = false;
        }

        if(_barRect.sizeDelta.x < _cooldownBarOrgWidth && !_death)
        {
            //float moveBar = Mathf.MoveTowards(_barRect.sizeDelta.x, _cooldownBarOrgWidth, _cooldownBarSpeed * Time.deltaTime * PlayerManager.Instance.CDReductionMult);
            _cooldownTimer = Mathf.MoveTowards(_cooldownTimer, _cooldown, Time.deltaTime * PlayerManager.Instance.CDReductionMult);
            float moveBar = _cooldownBarOrgWidth * (_cooldownTimer / _cooldown);
            _barRect.sizeDelta = new Vector2(moveBar, _barRect.sizeDelta.y);
        }

        if(_shake)
        {
            Vector2 shakePoint = _currPos + _shakeDelta;
            Vector2 moveTo = Vector2.MoveTowards(
                _recT.anchoredPosition, shakePoint, ShakeSpeed * _severity * Time.deltaTime);
            _recT.anchoredPosition = moveTo;

            if(_recT.anchoredPosition == shakePoint)
            {
                if(_moveToOrigin)
                {
                    _moveToOrigin = false;
                    _shakeDelta = Vector2.zero;
                }
                else
                {
                    _moveToOrigin = true;
                    _shakeDelta = _shakePoints.Dequeue();
                }

                if (_newMovePoint && ShakeAndMove)
                {
                    float moveYDiff = _moveToPoint.y - _currPos.y;
                    Vector2 tmpShake = _currPos + _shakeDelta;
                    float shakeYDiff = tmpShake.y - _recT.anchoredPosition.y; // Check if the shake is moving along with the new position move
                    if (Utility.SameSign(moveYDiff, shakeYDiff))    // If the shake is the same direction as the moving
                        _moveSpeed = PositionSpeed - ShakeTowardsModifier;         // Adjust the speed to go a little slower
                    else                                            // Or if it's against the move
                        _moveSpeed = PositionSpeed - ShakeAgainstModifier;         // Adjust the speed to go much slower 

                    Debug.Log(_moveSpeed);
                }

                if (_recT.anchoredPosition == _currPos && _shakePoints.Count == 0)
                {
                    _shake = false;
                    _moveSpeed = PositionSpeed;
                    if (_death)
                    {
                        Handler.RemoveGlance(this);
                        _newMovePoint = false;
                        if (ShakeAndMove)
                            CreateSkull();
                    }
                }
            }
        }
        else if(_death && !_shake)
        {
            Vector3 curAngle;
            if (_deathTilt.z < 0 && _recT.eulerAngles.z > 180)
                curAngle = new Vector3(0, 0, _recT.eulerAngles.z - 360);
            else
                curAngle = _recT.eulerAngles;

            Vector3 newV3 = Vector3.MoveTowards(curAngle, _deathTilt, Time.deltaTime * DeathAngleSpeed);
            _recT.eulerAngles = newV3;

            newV3 = Vector2.MoveTowards(_recT.anchoredPosition, _deathPoint, DeathSpeed + (_deathTime * 9.8f));
            _recT.anchoredPosition = newV3;

            if (_recT.anchoredPosition == _deathPoint)
                Destroy(gameObject);

            _deathTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Create points to shake the unit glance based on the severity of damage taken
    /// </summary>
    /// <param name="severity">The rank (1-3) of the damage taken</param>
    private void DamageShake()
    {
        float shakeSev = _severity * ShakeDistance;  // set how far to shake based on severity
        float minShake = .75f;                       // minimum amount of shake in %
        float maxShake = 1;                          // maximum amount of shake in %
        int numofPoint = 9 + _severity;              // how many shake points to create

        List<Vector2> tmpList = new List<Vector2>();
        for(int i = 0; i < numofPoint; i++)
        {
            float tmpShake = shakeSev * (_newMovePoint && ShakeAndMove ? 2 : 1); // If moving double the severity range for x;
            float pointX = CreateShakePoint(tmpShake, minShake, maxShake);
            float pointY = CreateShakePoint(shakeSev, minShake, maxShake);

            Vector2 newShakePoint = new Vector2(pointX, pointY);
            _shakePoints.Enqueue(newShakePoint);
            tmpList.Add(newShakePoint);

            shakeSev -= (shakeSev * (i+1) / numofPoint);  // decrease the shake severity as we create more points
        }

        _shakeDelta = _shakePoints.Dequeue();
        _shake = true;
        //_moveSpeed = PositionSpeed / _severity;
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

    private void CreateSkull()
    {
        var skull = Instantiate(GlanceDeath, Vector3.zero, Quaternion.identity, Handler.gameObject.transform);
        UnitGlanceDeath ugd = skull.GetComponent<UnitGlanceDeath>();
        ugd.SetPosition(new Vector2(
            _currPos.x + _recT.sizeDelta.x / 2,
            _currPos.y - _recT.sizeDelta.y / 2));
    }
}
