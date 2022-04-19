using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleSceneManager : SceneManager
{
    public TextMeshProUGUI Title;
    public AudioClip AfterSlideInBGM;

    internal override void Awake()
    {
        base.Awake();
    }

    internal override void Start()
    {
        Title.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0);
        base.Start();
    }

    internal override void Update()
    {
        base.Update();
    }

    public void StartMusic()
    {
        _audioSource.Play(AfterSlideInBGM);
    }
}
