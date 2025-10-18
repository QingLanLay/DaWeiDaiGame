using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private float attack;
    private bool isReturning;
    
    public float Attack
    {
        get => attack;
        set => attack = value;
    }

    // 计时器
    private float timeCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rb.velocity = Vector2.up * 3f;
    }

    private void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount >= 5f)
        {
            ReturnToPool();
            timeCount = 0;
        }
    }

    private void OnEnable()
    {
        timeCount = 0;
        isReturning = false;
        
        rb.velocity = Vector2.up * 3f;
        attack = GetComponentInParent<PlayerController>().Attack;
        

    }

    private void OnDisable()
    {
        timeCount = 0;
        isReturning = false;
    }

    public void ReturnToPool()
    {
        if (isReturning)
        {
            return;
        }
        isReturning = true;
        
        this.transform.position = Vector3.zero;
        gameObject.SetActive(false);
        BulletManager.Instance.bulletPool.Enqueue(this.gameObject);
    }
}