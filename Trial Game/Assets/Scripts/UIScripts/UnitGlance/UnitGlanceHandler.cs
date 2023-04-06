using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitGlanceHandler : MonoBehaviour
{
    [SerializeField] private GameObject UnitGlanceObject;
    [SerializeField] private float SizeSpeed;

    public static UnitGlanceHandler Instance { get; private set; }

    public float Height
    {
        get { return _recT.sizeDelta.y; }
    }

    private RectTransform _recT;
    private List<UnitGlance> _glances;
    private float _handlerHeight;
    private float _glanceHeight;

    /// <summary>
    /// Initialize 
    /// </summary>
    /// <param name="unitImage"></param>
    /// <returns></returns>
    public UnitGlance CreateUnitGlance(string name, Sprite unitImage)
    {
        var newGlanceObject = Instantiate(UnitGlanceObject, transform);
        newGlanceObject.name = name + "_Glance";

        UnitGlance uG = newGlanceObject.GetComponent<UnitGlance>();
        _glanceHeight = uG.Height;

        uG.Handler = this;
        uG.SetOrigin(new Vector2(0, -(_glanceHeight * _glances.Count)));
        uG.UnitPortrait.sprite = unitImage;

        _glances.Add(uG);
        _handlerHeight = _glances.Count * uG.Height;

        return uG;
    }

    public void RemoveGlance(UnitGlance uG)
    {
        if(_glances.Contains(uG))
        {
            _glances.Remove(uG);
            _handlerHeight = _glances.Count * uG.Height;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        _recT = GetComponent<RectTransform>();

        _glances = new List<UnitGlance>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Height != _handlerHeight)
        {
            float newHeight = Mathf.MoveTowards(Height, _handlerHeight, SizeSpeed * Time.deltaTime);
            _recT.sizeDelta = new Vector2(_recT.sizeDelta.x, newHeight);

            _recT.anchoredPosition = new Vector2(_recT.anchoredPosition.x, newHeight / 2);
        }

        var glanceCDOrder = _glances
            .OrderByDescending(g => g.CooldownPercent)
            .ToList();
        float avlblGlances = _glances.Count - glanceCDOrder.Count;
        glanceCDOrder.ForEach(g => CooldownPosition(g, glanceCDOrder.IndexOf(g)));//, avlblGlances););
    }

    private void CooldownPosition(UnitGlance uG, float cdIndex)//, float glancesNotInCooldown)
    {
        Vector2 newPos = new Vector2(0, -(_glanceHeight * cdIndex));//(glancesNotInCooldown + cdIndex)));
        uG.SetPosition(newPos);
    }
}