using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSound : MonoBehaviour
{
    public List<AudioClip> DeathSounds;

    AudioSource _aS;

    void Awake()
    {
        var clip = DeathSounds[(int)(Time.time % DeathSounds.Count)];
        _aS = GetComponent<AudioSource>();
        _aS.Play(clip);
    }

    void Update()
    {
        if(!_aS.isPlaying)
        {
            Destroy(this);
        }
    }
}
