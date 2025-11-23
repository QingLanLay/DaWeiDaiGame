using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioManager : SingletonMono<AudioManager>
{
    #region 变量声明
    public AudioMixer master;
    public AudioSource gameSource;
    public AudioSource ambientSource;

    public Slider effectSlider;
    public Slider ambientSlider;

    public List<AudioClip> ambientSourceList = new List<AudioClip>();
    #endregion

    #region Unity 生命周期方法
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
    #endregion

    #region 音频播放方法
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
    #endregion

    #region 音量控制方法
    public void changeMasterEffectValue()
    {
        var currentValue = effectSlider.value;

        currentValue = LerpAudioValue(currentValue);

        master.SetFloat("EffectAudioValue", currentValue);
    }

    public void changeMasterAmbinetValue()
    {
        var currentValue = ambientSlider.value;
        currentValue = LerpAudioValue(currentValue);

        master.SetFloat("AmbientAudioValue", currentValue);
    }

    private static float LerpAudioValue(float currentValue)
    {
        switch (currentValue)
        {
            case >= 0.5f:
                // 第三区间: 0.5-1 → -20到1
                currentValue = -20f + ((currentValue - 0.5f) / 0.5f) * 21f;
                break;
            case > 0.1f and < 0.5f:
                // 第二区间: 0.1-0.5 → -40到-20
                currentValue = -40f + ((currentValue - 0.1f) / 0.4f) * 20f;
                break;
            case <= 0.1f:
                // 第一区间: 0-0.1 → -80到-40
                currentValue = -80f + (currentValue / 0.1f) * 40f;
                break;
            default:
                break;
        }

        currentValue = Mathf.Clamp(currentValue, -80, 1);

        if (currentValue < -79.9f)
        {
            currentValue = -80;
        }

        return currentValue;
    }
    #endregion

    #region 协程方法
    public void ChangeBGM()
    {
        StartCoroutine(Wait3Seconds());
    }

    public IEnumerator Wait3Seconds()
    {
        yield return new WaitForSeconds(3);
        var range = Random.Range(0, ambientSourceList.Count - 1);
        PlayAmbientAudio(range);
    }
    #endregion
}