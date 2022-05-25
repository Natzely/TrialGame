using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialImageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private Image Image;

    private List<string> _texts;
    public void UpdateTutorial(Sprite image, List<string> texts)
    {
        Image.sprite = image;
        _texts = texts;
        ChangeText();
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.Instance.LanguageChange.AddListener(ChangeText);
    }

    private void ChangeText()
    {
        if (GameSettinngsManager.Instance)
            Text.text = _texts[(int)GameSettinngsManager.Instance.Language];
    }
}

