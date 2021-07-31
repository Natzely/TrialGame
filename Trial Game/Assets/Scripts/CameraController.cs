using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public float ZoomIn;
    public float ZoomOut;
    public float ZoomSpeed;

    private CinemachineVirtualCamera _camera;
    private float _goalZoom;

    public void UpdateZoom(Enums.CursorState state)
    {
        _goalZoom = state switch
        {
            Enums.CursorState.Default => ZoomOut,
            _ => ZoomIn,
        };
    }

    private void Awake()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _goalZoom = ZoomOut;
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera.m_Lens.OrthographicSize = ZoomOut;
    }

    // Update is called once per frame
    void Update()
    {
        if (_camera.m_Lens.OrthographicSize != _goalZoom)
        {
            var dif = _goalZoom - _camera.m_Lens.OrthographicSize;
            var newZoom = Mathf.Clamp(_camera.m_Lens.OrthographicSize + (dif * Time.deltaTime * ZoomSpeed), ZoomIn, ZoomOut);
            _camera.m_Lens.OrthographicSize = newZoom;
        }
    }
}
