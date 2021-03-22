using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Path_Saved_Mask : MonoBehaviour
{
    public GameObject Mask;

    public UnitController Unit { get; set; }

    private Transform _maskTransform;
    private BoxCollider2D _bC;
    private bool _updateMask;

    void Awake()
    {
        _updateMask = false;
        _bC = GetComponent<BoxCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _maskTransform = Mask.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(_updateMask)
        {
            var newScale = new Vector3(
                (_maskTransform.position[Unit.LookDirIndex] - Unit.Position[Unit.LookDirIndex]) * 1.8f,
                _maskTransform.localScale.y,
                _maskTransform.localScale.z);
            _maskTransform.localScale = newScale;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject o = collision.gameObject;
        UnitController uC = o.GetComponent<UnitController>();
        if (uC != null && uC == Unit)
        {
            var dif = Mathf.Abs(_maskTransform.position[Unit.LookDirIndex] - Unit.Position[Unit.LookDirIndex]);
            if (dif < .15f)
            {
                _bC.enabled = false;
                _updateMask = true;
                //Destroy(_rB);
            }
        }
    }
}
