using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : SingletonMono<AudioManager>
{
    public AudioSource gameSource;
    public AudioSource AmbientSource;
    
    public List<AudioClip> AmbientSourceList = new List<AudioClip>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // 保持背景音乐音乐播放
        if (!AmbientSource.isPlaying)
        {
            var range = Random.Range(0, AmbientSourceList.Count);
            PlayAmbientAudio(range);
        }
    }

    public void PlayEffectAudio(AudioClip clip)
    {
        if (gameSource.isPlaying)
        {
            return;
        }
        gameSource.clip = clip;
        gameSource.Play();
    }
    
    public void PlayAmbientAudio(int index)
    {
        if (AmbientSourceList[index] == null)
        {
            return;
        }
        AmbientSource.clip = AmbientSourceList[index];
        AmbientSource.Play();
    }

}
