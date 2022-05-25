using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class ContinueButton : MonoBehaviour
{
    [SerializeField] private float HoldTime;
    [SerializeField] private string Scene;

    private RectTransform _rectT;
    private bool _perfom;
    private bool _loadScene;
    private float _orgWidth;

    private void Awake()
    {
        _rectT = GetComponent<RectTransform>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (GameSettinngsManager.Instance.Language == Enums.Language.English)
            _orgWidth = 250;
        else
            _orgWidth = 305;
        _rectT.sizeDelta = new Vector2(_orgWidth, _rectT.sizeDelta.y);
    }

    // Update is called once per frame
    void Update()
    {
        if(_perfom)
        {
            float percent = Time.deltaTime / HoldTime;
            float sub = percent * _orgWidth;
            float newWidth = _rectT.sizeDelta.x - sub;
            _rectT.sizeDelta = new Vector2(newWidth, _rectT.sizeDelta.y);
            if (_rectT.sizeDelta.x <= 0)
            {
                _loadScene = true;
                _perfom = false;
                SceneManager.Instance.LoadScene(Scene);
            }
        }
    }

    public void Submit(InputAction.CallbackContext context)
    {
        if (context.started)
            _perfom = true;
        else if (context.canceled && !_loadScene)
        { 
            _perfom = false;
            _rectT.sizeDelta = new Vector2(_orgWidth, _rectT.sizeDelta.y);
        }
    }
}
