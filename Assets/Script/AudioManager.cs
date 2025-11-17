using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMono<AudioManager>
{
    public AudioSource gameSource;
    public AudioSource AmbientSource;
    
    public List<AudioClip> AmbientSourceList = new List<AudioClip>();

    protected override void Awake()
    {
        base.Awake();
        AmbientSource.clip = AmbientSourceList[0];
    }
}
