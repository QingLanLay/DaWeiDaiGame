using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioManager : SingletonMono<AudioManager>
{
    public AudioMixer master;
    public AudioSource gameSource;
    public AudioSource ambientSource;


    public Slider effectSlider;
    public Slider ambientSlider;

    public List<AudioClip> ambientSourceList = new List<AudioClip>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // 保持背景音乐音乐播放
        if (!ambientSource.isPlaying)
        {
            var range = Random.Range(0, ambientSourceList.Count - 1);
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
        if (ambientSourceList[index] == null)
        {
            return;
        }

        ambientSource.clip = ambientSourceList[index];
        ambientSource.Play();
    }

    public void PlayEffectAudio(AudioClip clip, float value)
    {
        var randomNum = Random.value * 100;
        if (randomNum <= value)
        {
            // 触发受击音效
            AudioManager.Instance.PlayEffectAudio(clip);
        }
    }

    public void changeMasterEffectValue()
    {
        var currentValue = effectSlider.value;

        currentValue = LerpAudioValue(currentValue);


        master.SetFloat("EffectAudioValue", currentValue);
    }

    private static float LerpAudioValue(float currentValue)
    {
        switch (currentValue)
        {
            case >= 0.5f:
                currentValue = currentValue * 6 - 3;
                break;

            case < 0.5f:
                currentValue = currentValue * 160 - 160; 
                break;
            default:
                break;
        }

        currentValue = Mathf.Clamp(currentValue, -80, 3);

        if (currentValue < -79.9f)
        {
            currentValue = -80;
        }

        return currentValue;
    }

    public void changeMasterAmbinetValue()
    {
        var currentValue = ambientSlider.value;
        currentValue = LerpAudioValue(currentValue);

        master.SetFloat("AmbientAudioValue", currentValue);
    }
}