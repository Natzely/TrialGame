using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleSceneManager : SceneManager
{
    public TextMeshProUGUI Title;
    public AudioClip AfterSlideInBGM;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        Title.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected void StartMusic()
    {
        _audioSource.Play(AfterSlideInBGM);
    }
}
