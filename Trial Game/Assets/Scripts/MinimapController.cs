using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapController : MonoBehaviour, ILog
{
    [SerializeField] private PauseScreen PauseScreen;
    [SerializeField] private GameObject MapIcons;

    private bool _showingMap;

    private string _errorMessage = "MapIcons Object not set";

    private void Awake()
    {
        if (MapIcons == null)
            LogError(_errorMessage);
        else
            MapIcons.SetActive(false);
    }

    public void ShowMinimap(InputAction.CallbackContext context)
    {
        if (!PauseScreen.IsGamePaused)
        {
            if (!_showingMap && context.performed)
            {
                if (MapIcons == null)
                {
                    LogError(_errorMessage);
                    return;
                }

                _showingMap = true;
                MapIcons.SetActive(true);
                Log("Show minimap");
            }
            else if (_showingMap && context.canceled)
            {
                _showingMap = false;
                MapIcons.SetActive(false);
                Log("Hide minimap");
            }
        }
    }

    public void Log(string msg)
    {
        Debug.Log($"{gameObject.name} | {msg}");
    }

    public void LogError(string msg)
    {
        Debug.LogError($"{gameObject.name} | {msg}");
    }
}
