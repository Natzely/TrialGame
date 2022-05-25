using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIText : UIObject
{

    [SerializeField] private TextMeshProUGUI Text;
    public List<string> LanguageTexts;
    
    public void UpdateText(int index, string text)
    {
        LanguageTexts[index] = text;
    }

    private void Start()
    {
        SceneManager.Instance.LanguageChange.AddListener(ChangeText);
        ChangeText();
    }

    private void ChangeText()
    {
        if (GameSettinngsManager.Instance)
            Text.text = LanguageTexts[(int)GameSettinngsManager.Instance.Language];
    }    
}
