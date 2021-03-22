using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float ZoomIn;
    public float ZoomOut;
    public float ZoomSpeed;

    private Camera _camera;
    private float _goalZoom;

    public void UpdateZoom(Enums.CursorState state)
    {
        switch(state)
        {
            case Enums.CursorState.Default:
                _goalZoom = ZoomOut;
                break;
            default:
                _goalZoom = ZoomIn;
                break;
        }
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _goalZoom = ZoomOut;
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera.orthographicSize = ZoomOut;
    }

    // Update is called once per frame
    void Update()
    {
        if (_camera.orthographicSize != _goalZoom)
        {
            var dif = _goalZoom - _camera.orthographicSize;
            var newZoom = Mathf.Clamp(_camera.orthographicSize + (dif * Time.deltaTime * ZoomSpeed), ZoomIn, ZoomOut);
            _camera.orthographicSize = newZoom;
        }
    }
}
