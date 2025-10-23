using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private int ID;

    [SerializeField]
    private float health;

    [SerializeField]
    private float attack;

    [SerializeField]
    private float speed;

    [SerializeField]
    private int exp;

    private Sprite icon;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public GameObject player;

    private bool isDead = false;


    public float horizontalSpeed = 3f; // 水平移动的速度
    public float directionChangeInterval = 1f; // 方向改变间隔（秒）
    private float timer = 0f;
    private int horizontalDirection = 0; // 1=右, -1=左, 0=无水平移动
    private int level;

    public float Attack
    {
        set => attack = value;
        get => attack;
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = icon;
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        // 初始随机方向
        ChangeDirection(0);
        }

    private void OnEnable()
    {
        isDead = false;
        // 确保物理状态重置
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
    public void GetEnemyData(EnemyData enemyData)
    {
        // 每次获取数据时都重新获取玩家等级
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
            
        level = player.GetComponent<PlayerController>().level;

        ID = enemyData.ID;
        health = enemyData.Health;
        speed = enemyData.Speed;
        attack = enemyData.Attack;
        icon = enemyData.Icon;
        exp = enemyData.Exp;
        
        spriteRenderer.sprite = icon;
        spriteRenderer.size = new Vector2(1, 1);
        spriteRenderer.color = Color.white;

        if (level > 1)
        {
            health += 100 * level;
            attack += 100 * level;
            speed += 1 * level * 0.5f;
            exp += 100 * level;
        }

        // 重置移动状态
        timer = 0f;
        isRushing = false;
        nextBehaviorChangeTime = 0f;
        ChangeDirection(ID);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
            
        
        if (other.CompareTag("Bullet"))
        {
            health -= other.GetComponent<Bullet>().Attack;
            var bullet = other.GetComponent<Bullet>();
            bullet.ReturnToPool();
            animator.SetTrigger("BeAttack");
        }

        if (health <= 0)
        {
            isDead = true;
            var davidDie = GameObject.FindWithTag("UI").GetComponent<DavidDie>();
            davidDie.UpLevel(exp);
            Dead();
        }
    }
    
    public void Dead()
    {
        if (!isDead)
        {
            isDead = true;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        StopAllCoroutines();
        
        // 重置位置到管理器位置而不是(0,0,0)
        transform.position = EnemyManager.Instance.transform.position;
        
        EnemyManager.Instance.enemyPool.Enqueue(this.gameObject);
        gameObject.transform.position = Vector3.zero;
        
        icon = null;
        
        spriteRenderer.sprite = null;
        spriteRenderer.color = Color.white;
        this.gameObject.SetActive(false);
    }
    




    private void Update()
    {
        // 更新计时器
        timer += Time.deltaTime;

        // 每隔指定时间改变方向
        if (timer >= directionChangeInterval)
        {
            ChangeDirection(ID);
            timer = 0f; // 重置计时器
        }
    }

    private void FixedUpdate()
    {
        Move();
        needReverse();
    }

    void needReverse()
    {
        // 屏幕边界检查
        Vector3 screenPos = Camera.main.WorldToViewportPoint(this.transform.position);
        horizontalDirection = screenPos.x switch
        {
            < 0.05f => 1, // 左边界向右
            > 0.95f => -1, // 右边界外向左
            _ => horizontalDirection
        };
    }

    private void Move()
    {
        Vector2 movement = new Vector2();
        switch (ID)
        {
            case 0:
                rb.velocity = Vector2.down * speed;
                break;
            case 1:
                movement = new Vector2(horizontalDirection * horizontalSpeed, -speed);
                rb.velocity = movement;
                break;
            case 2:
                // 冲刺与徘徊：在普通移动和高速冲刺间切换
                if (Time.time >= nextBehaviorChangeTime)
                {
                    if (isRushing)
                    {
                        // 结束冲刺，进入徘徊
                        horizontalSpeed = normalSpeed;
                        nextBehaviorChangeTime = Time.time + Random.Range(1f, 3f); // 徘徊时间

                        // 徘徊开始时，来一个缓慢的随机旋转，模拟疑惑或寻找
                        float randomRotation = Random.Range(-180f, 180f);
                        // 在徘徊的这段时间内慢慢转过去
                        StartCoroutine(SmoothRotateOverTime(randomRotation, nextBehaviorChangeTime - Time.time));
                    }
                    else
                    {
                        // 开始冲刺，速度急剧加快并向玩家方向（或随机）冲刺
                        horizontalSpeed = rushSpeed;
                        horizontalDirection =
                            (Random.value > 0.7f) ? GetPlayerDirection() : Random.Range(-1, 2); // 70%概率向玩家冲刺
                        nextBehaviorChangeTime = Time.time + Random.Range(0.3f, 1f); // 冲刺持续时间

                        // 冲刺开始时，快速随机旋转一下，制造晕头转向的效果
                        // transform.Rotate(0, 0, Random.Range(-30f, 30f)); // 绕Z轴旋转，更显平面2D的滑稽
                    }

                    isRushing = !isRushing;
                }

                movement = new Vector2(horizontalDirection * horizontalSpeed, -speed);
                rb.velocity = movement;
                break;
            default:
                break;
        }
    }

    private IEnumerator SmoothRotateOverTime(float randomRotation, float time)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, randomRotation); // 绕Z轴旋转

        while (elapsedTime < time)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    private bool isRushing = false;
    private float nextBehaviorChangeTime = 0f;
    public float normalSpeed = 2f;
    public float rushSpeed = 8f;

// 一个简单的辅助方法，判断玩家在左还是右
    private int GetPlayerDirection()
    {
        // 假设你有一个参考玩家位置的对象，比如 player.transform
        if (player.transform.position.x > transform.position.x) return 1;
        else return -1;
    }

    [Header("随机性设置")]
    public float minInterval = 0.5f;

    public float maxInterval = 2f;

    void ChangeDirection(int id)
    {
        if (id == 0)
        {
            return;
        }

        if (id == 1)
        {
            minInterval = 1f;
            maxInterval = 3f;
        }

        if (id == 2)
        {
            minInterval = 0.5f;
            maxInterval = 1.5f;
        }

        horizontalDirection = Random.Range(-1, 2);
        directionChangeInterval = Random.Range(minInterval, maxInterval); // 随机间隔时间
    }


    



    
}