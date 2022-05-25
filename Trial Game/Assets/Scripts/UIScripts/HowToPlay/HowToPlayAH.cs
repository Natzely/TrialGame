using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HowToPlayAH : UIActionHandler
{
    [SerializeField] private TutorialImageText TutorialText;

    public override void HandleButtonSubmit(UIButton button)
    {
    }

    public override void OnItemSelected(UIButton button)
    {
        base.OnItemSelected(button);
        HowToPlayButton htpB = (HowToPlayButton)button;

        TutorialText.UpdateTutorial(
            htpB.TutorialImage,
            htpB.LanguageTexts);// [(int)GameSettinngsManager.Instance.Language]);
    }
}
