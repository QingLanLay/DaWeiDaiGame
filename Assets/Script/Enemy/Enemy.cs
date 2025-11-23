using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("基础属性")]
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

    [SerializeField]
    private Sprite icon;

    [Header("移动设置")]
    public float horizontalSpeed = 3f;
    public float directionChangeInterval = 1f;
    public float normalSpeed = 2f;
    public float rushSpeed = 8f;
    public float minInterval = 0.5f;
    public float maxInterval = 2f;

    [Header("等级缩放")]
    public EnemyScaling scaling;

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private GameObject player;
    private BoxCollider2D boxCollider; // 新增：碰撞体引用

    // 状态变量
    private bool isDead = false;
    private float timer = 0f;
    private int horizontalDirection = 0;
    private int wave;
    private bool isRushing = false;
    private float nextBehaviorChangeTime = 0f;
    private AudioClip effectClip;
    public float Attack
    {
        set => attack = value;
        get => attack;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        boxCollider = GetComponent<BoxCollider2D>(); // 获取碰撞体
    }

    private void OnEnable()
    {
        isDead = false;
        // 重置物理状态
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 初始随机方向
        ChangeDirection();
        
        // TODO：如果是雨姐，修改背景音乐
        if (ID == 2)
        {
            AudioManager.Instance.PlayAmbientAudio(5);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 获取敌人数据并初始化
    /// </summary>
    public void GetEnemyData(EnemyData enemyData)
    {
        // 获取玩家等级
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        wave = EnemyManager.Instance.GetCurrentWave();

        // 设置基础属性
        ID = enemyData.ID;
        health = enemyData.Health;
        speed = enemyData.Speed;
        attack = enemyData.Attack;
        icon = enemyData.Icon;
        exp = enemyData.Exp;
        effectClip = enemyData.efffectClip;
        

        // 应用等级缩放
        ApplyLevelScaling(wave);

        // 设置外观和碰撞体
        SetupAppearanceAndCollider(enemyData);

        // 重置移动状态
        timer = 0f;
        isRushing = false;
        nextBehaviorChangeTime = 0f;

        // 根据敌人类型设置行为参数
        SetBehaviorByType();
    }

    /// <summary>
    /// 设置外观和碰撞体
    /// </summary>
    private void SetupAppearanceAndCollider(EnemyData enemyData)
    {
        // 设置精灵
        spriteRenderer.sprite = icon;
        spriteRenderer.color = Color.white;

        // 应用缩放
        float scale = enemyData.Scale;
        transform.localScale = new Vector3(scale, scale, 1f);

        // 创建或更新碰撞体
        CreateOrUpdateCollider();
    }

    /// <summary>
    /// 创建或更新碰撞体以匹配图片比例
    /// </summary>
    private void CreateOrUpdateCollider()
    {
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("敌人没有设置精灵，无法创建碰撞体");
            return;
        }

        // 确保有碰撞体组件
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // 获取精灵的原始尺寸（像素）
        Sprite sprite = spriteRenderer.sprite;
        float pixelsPerUnit = sprite.pixelsPerUnit;
        
        // 计算精灵的世界单位尺寸
        float widthInWorldUnits = sprite.rect.width / pixelsPerUnit;
        float heightInWorldUnits = sprite.rect.height / pixelsPerUnit;

        // 设置碰撞体大小为精灵的原始尺寸
        boxCollider.size = new Vector2(widthInWorldUnits, heightInWorldUnits);

        // 可选：调整碰撞体偏移以确保居中
        boxCollider.offset = Vector2.zero;

        Debug.Log($"敌人碰撞体设置完成: {sprite.name} 尺寸: {widthInWorldUnits:F2}x{heightInWorldUnits:F2}");
    }

    /// <summary>
    /// 根据敌人类型设置行为参数
    /// </summary>
    private void SetBehaviorByType()
    {
        switch (ID)
        {
            case 0: // 普通敌人
                minInterval = 1f;
                maxInterval = 3f;
                break;
            case 1: // 精英敌人
                minInterval = 0.8f;
                maxInterval = 2f;
                break;
            case 2: // BOSS
                minInterval = 0.5f;
                maxInterval = 1.5f;
                break;
        }
    }

    /// <summary>
    /// 应用等级缩放
    /// </summary>
    public void ApplyLevelScaling(int wave)
    {
        if (wave <= 1) return;

        float levelBonus = wave - 1;

        // 应用缩放
        float healthMultiplier = 1 + levelBonus * scaling.healthScale;
        float attackMultiplier = 1 + levelBonus * scaling.attackScale;
        float speedMultiplier = 1 + levelBonus * scaling.speedScale;
        float expMultiplier = 1 + levelBonus * scaling.expScale;

        health = health * healthMultiplier;
        attack = attack * attackMultiplier;
        speed = speed * speedMultiplier;
        exp = Mathf.RoundToInt(exp * expMultiplier);

        // 经验值上限
        if (exp > 50000)
        {
            exp = 50000;
        }
    }

    private void Update()
    {
        if (isDead) return;

        // 更新计时器
        timer += Time.deltaTime;

        // 定期改变方向
        if (timer >= directionChangeInterval)
        {
            ChangeDirection();
            timer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        Move();
        CheckScreenBounds();
    }

    /// <summary>
    /// 检查屏幕边界
    /// </summary>
    private void CheckScreenBounds()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);

        if (screenPos.x < 0.05f)
        {
            horizontalDirection = 1; // 左边界向右
        }
        else if (screenPos.x > 0.95f)
        {
            horizontalDirection = -1; // 右边界向左
        }
    }

    /// <summary>
    /// 敌人移动逻辑
    /// </summary>
    private void Move()
    {
        Vector2 movement;

        switch (ID)
        {
            case 0: // 普通敌人：直线下落
                movement = Vector2.down * speed;
                rb.velocity = movement;
                break;

            case 1: // 精英敌人：水平移动 + 下落
                movement = new Vector2(horizontalDirection * horizontalSpeed, -speed);
                rb.velocity = movement;
                break;

            case 2: // BOSS：冲刺与徘徊交替
                HandleBossMovement();
                break;

            default:
                movement = Vector2.down * speed;
                rb.velocity = movement;
                break;
        }
    }

    /// <summary>
    /// 处理BOSS移动逻辑
    /// </summary>
    private void HandleBossMovement()
    {
        // 行为切换
        if (Time.time >= nextBehaviorChangeTime)
        {
            if (isRushing)
            {
                // 结束冲刺，进入徘徊
                horizontalSpeed = normalSpeed;
                nextBehaviorChangeTime = Time.time + Random.Range(1f, 3f);

                // 随机旋转
                float randomRotation = Random.Range(-180f, 180f);
                StartCoroutine(SmoothRotate(randomRotation, nextBehaviorChangeTime - Time.time));
            }
            else
            {
                // 开始冲刺
                horizontalSpeed = rushSpeed;
                horizontalDirection = (Random.value > 0.7f) ? GetPlayerDirection() : Random.Range(-1, 2);
                nextBehaviorChangeTime = Time.time + Random.Range(0.3f, 1f);
            }

            isRushing = !isRushing;
        }

        // 应用移动
        Vector2 movement = new Vector2(horizontalDirection * horizontalSpeed, -speed);
        rb.velocity = movement;
    }

    /// <summary>
    /// 平滑旋转
    /// </summary>
    private IEnumerator SmoothRotate(float targetRotation, float duration)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotationQuat = startRotation * Quaternion.Euler(0, 0, targetRotation);

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotationQuat, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotationQuat;
    }

    /// <summary>
    /// 获取玩家方向
    /// </summary>
    private int GetPlayerDirection()
    {
        if (player != null && player.transform.position.x > transform.position.x)
            return 1;
        else
            return -1;
    }

    /// <summary>
    /// 改变移动方向
    /// </summary>
    private void ChangeDirection()
    {
        if (ID == 0) return; // 普通敌人不改变水平方向

        horizontalDirection = Random.Range(-1, 2);
        directionChangeInterval = Random.Range(minInterval, maxInterval);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Bullet"))
        {
            
            // TODO:音效
            // 受击触发概率
            AudioManager.Instance.PlayEffectAudio(effectClip,10f);
          

            // 受到子弹伤害
            health -= other.GetComponent<Bullet>().Attack;
            other.GetComponent<Bullet>().ReturnToPool();

            if (animator != null)
            {
                animator.SetTrigger("BeAttack");
            }

            // 检查死亡
            if (health <= 0)
            {
                Die();
                if (other.CompareTag("Bullet"))
                {
                    var playerController = player.GetComponent<PlayerController>();
                    if (ID == 2)
                    {
                        playerController.MaxSpeed += 0.5f;
                    }

                    if (ID == 4)
                    {
                        playerController.Attack += 10f;
                    }
                    // 给予经验值
                    GiveExperience();
                }
            }
        }
    }

    /// <summary>
    /// 敌人死亡
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        
        // 回收敌人
        ReturnToPool();
    }

    /// <summary>
    /// 给予玩家经验值
    /// </summary>
    private void GiveExperience()
    {
        var davidDie = GameObject.FindWithTag("UI").GetComponent<DavidDie>();
        if (davidDie != null)
        {
            int safeExp = exp;
            if (safeExp > 100000)
            {
                Debug.LogError($"异常经验值: {safeExp}，已限制为10000");
                safeExp = 10000;
            }

            davidDie.UpLevel(safeExp);
        }
    }

    /// <summary>
    /// 返回对象池
    /// </summary>
    private void ReturnToPool()
    {
        // 重置状态
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        StopAllCoroutines();

        // 禁用对象
        gameObject.SetActive(false);

        // 通知敌人管理器回收
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ReturnEnemyToPool(this.gameObject);
        }
    }

    /// <summary>
    /// 强制死亡（用于外部调用）
    /// </summary>
    public void Dead()
    {
        if (!isDead)
        {
            Die();
        }
    }
}   

[System.Serializable]
public class EnemyScaling
{
    [Header("等级缩放系数")]
    public float healthScale = 3f;

    public float attackScale = 2f;
    public float speedScale = 0.1f;
    public float expScale = 4f;
}