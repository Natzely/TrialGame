using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen_WhiteOut : MonoBehaviour
{
    [SerializeField] private AudioSource StrikeSource;

    private Animator _animator;

    public void StartAnimation()
    {
        _animator.SetBool("Start", true);
        StrikeSource.Play();
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        StrikeSource = GetComponent<AudioSource>();
    }
}
