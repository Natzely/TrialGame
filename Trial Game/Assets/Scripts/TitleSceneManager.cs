using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneManager : SceneManager
{
    public AudioClip AfterSlideInBGM;

    internal override void Awake()
    {
        base.Awake();
    }

    internal override void Start()
    {
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
