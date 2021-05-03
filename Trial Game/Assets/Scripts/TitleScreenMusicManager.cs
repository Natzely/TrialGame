using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TitleScreenMusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource Start;
    [SerializeField] private AudioSource Loop;

    private AudioSource _audioSource;

    void Awake()
    {
        Loop.PlayDelayed(Loop.clip.length);  
    }
}
