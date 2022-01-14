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

    // Update is called once per frame
    void Update()
    {
        if (_camera.m_Lens.OrthographicSize != _goalZoom)
        {
            float newZoom = Mathf.MoveTowards(_camera.m_Lens.OrthographicSize, _goalZoom, Time.deltaTime * ZoomSpeed);
            _camera.m_Lens.OrthographicSize = newZoom;
        }
    }
}
