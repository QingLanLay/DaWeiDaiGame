using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [System.Serializable]
    public class EnemyScaling
    {
        [Header("等级缩放系数")]
        public float healthScale = 3f;
        public float attackScale = 2f;
        public float speedScale = 0.1f;
        public float expScale = 4f;
    }

    #region 属性和字段
    // 属性
    public float Attack
    {
        set => attack = value;
        get => attack;
    }

    [Header("基础属性")]
    [SerializeField] private int ID;
    [SerializeField] private float health;
    [SerializeField] private float attack;
    [SerializeField] private float speed;
    [SerializeField] private int exp;
    [SerializeField] private Sprite icon;

    [Header("移动设置")]
    public float horizontalSpeed = 3f;
    public float directionChangeInterval = 1f;
    public float normalSpeed = 2f;
    public float rushSpeed = 8f;
    public float minInterval = 0.5f;
    public float maxInterval = 2f;

    [Header("等级缩放")]
    public EnemyScaling scaling;

    // 私有字段
    private float maxHealth;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private GameObject player;
    private BoxCollider2D boxCollider;
    private FoodData BaBa;

    // 状态变量
    private bool isDead = false;
    private float timer = 0f;
    private int horizontalDirection = 0;
    private int wave;
    private bool isRushing = false;
    private float nextBehaviorChangeTime = 0f;
    private AudioClip effectClip;
    private float bossTime;
    private float bossOnAirTime = 0;
    #endregion

    #region Unity 生命周期方法
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        boxCollider = GetComponent<BoxCollider2D>();
        BaBa = FallingObjManager.Instance.GetFoodDataByID(14);
    }

    private void OnEnable()
    {
        isDead = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        ChangeDirection();

        if (ID == 2)
        {
            AudioManager.Instance.PlayAmbientAudio(5);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if (isDead) return;

        timer += Time.deltaTime;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Bullet"))
        {
            AudioManager.Instance.PlayEffectAudio(effectClip, 10f);

            health -= other.GetComponent<Bullet>().Attack;
            other.GetComponent<Bullet>().ReturnToPool();

            if (animator != null)
            {
                animator.SetTrigger("BeAttack");
                AudioManager.Instance.PlayBulletAudio(1);
            }

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

                    GiveExperience();
                }
            }
        }
    }
    #endregion

    #region 公有方法
    public void GetEnemyData(EnemyData enemyData)
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        wave = EnemyManager.Instance.GetCurrentWave();

        ID = enemyData.ID;
        health = enemyData.Health;
        speed = enemyData.Speed;
        attack = enemyData.Attack;
        icon = enemyData.Icon;
        exp = enemyData.Exp;
        effectClip = enemyData.efffectClip;

        ApplyLevelScaling(wave);
        SetupAppearanceAndCollider(enemyData);

        timer = 0f;
        isRushing = false;
        nextBehaviorChangeTime = 0f;

        SetBehaviorByType();
    }

    public void ApplyLevelScaling(int wave)
    {
        if (wave <= 1) return;

        float levelBonus = wave - 1;

        float healthMultiplier = 1 + levelBonus * scaling.healthScale;
        float attackMultiplier = 1 + levelBonus * scaling.attackScale;
        float speedMultiplier = 1 + levelBonus * scaling.speedScale;
        float expMultiplier = 1 + levelBonus * scaling.expScale;

        health = health * healthMultiplier;
        maxHealth = health * healthMultiplier;
        attack = attack * attackMultiplier;
        speed = speed * speedMultiplier;
        exp = Mathf.RoundToInt(exp * expMultiplier);

        if (exp > 50000)
        {
            exp = 50000;
        }
    }

    public void Dead()
    {
        if (!isDead)
        {
            Die();
        }
    }
    #endregion

    #region 初始化配置方法
    private void SetupAppearanceAndCollider(EnemyData enemyData)
    {
        spriteRenderer.sprite = icon;
        spriteRenderer.color = Color.white;

        float scale = enemyData.Scale;
        transform.localScale = new Vector3(scale, scale, 1f);

        CreateOrUpdateCollider();
    }

    private void CreateOrUpdateCollider()
    {
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning("敌人没有设置精灵，无法创建碰撞体");
            return;
        }

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        Sprite sprite = spriteRenderer.sprite;
        float pixelsPerUnit = sprite.pixelsPerUnit;

        float widthInWorldUnits = sprite.rect.width / pixelsPerUnit;
        float heightInWorldUnits = sprite.rect.height / pixelsPerUnit;

        boxCollider.size = new Vector2(widthInWorldUnits, heightInWorldUnits);
        boxCollider.offset = Vector2.zero;

        Debug.Log($"敌人碰撞体设置完成: {sprite.name} 尺寸: {widthInWorldUnits:F2}x{heightInWorldUnits:F2}");
    }

    private void SetBehaviorByType()
    {
        switch (ID)
        {
            case 0:
                minInterval = 1f;
                maxInterval = 3f;
                break;
            case 1:
                minInterval = 0.8f;
                maxInterval = 2f;
                break;
            case 2:
                minInterval = 0.5f;
                maxInterval = 1.5f;
                break;
        }
    }
    #endregion

    #region 移动和行为方法
    private void Move()
    {
        Vector2 movement;

        switch (ID)
        {
            case 0:
                movement = Vector2.down * speed;
                rb.velocity = movement;
                break;

            case 1:
                movement = new Vector2(horizontalDirection * horizontalSpeed, -speed);
                rb.velocity = movement;
                break;

            case 2:
                var state1 = CheckCurrentBlooldAndTime();
                if (state1)
                {
                    CheckTransitionAndMove();
                }
                else
                {
                    HandleBossMovement();
                }
                break;

            default:
                movement = Vector2.down * speed;
                rb.velocity = movement;
                break;
        }
    }

    private void CheckScreenBounds()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);

        if (screenPos.x < 0.05f)
        {
            horizontalDirection = 1;
        }
        else if (screenPos.x > 0.95f)
        {
            horizontalDirection = -1;
        }
    }

    private void CheckTransitionAndMove()
    {
        if (transform.position.y > -1.3f)
        {
            var movement = Vector2.down * speed;
            rb.velocity = movement;
        }
        else
        {
            var movement = new Vector2(horizontalDirection * horizontalSpeed, 0f);
            rb.velocity = movement;
            bossTime += Time.deltaTime;
            if (bossTime >= 1f * DavidDie.Instance.level)
            {
                bossTime = 0f;
                FallingObjManager.Instance.GetOrCreatFood(BaBa, this.transform.position);
            }
        }
    }

    private bool CheckCurrentBlooldAndTime()
    {
        bossOnAirTime += Time.deltaTime;
        if (bossOnAirTime >= 50f)
        {
            return false;
        }

        if (health >= 0.2f * maxHealth)
        {
            return true;
        }

        return false;
    }

    private void HandleBossMovement()
    {
        if (Time.time >= nextBehaviorChangeTime)
        {
            if (isRushing)
            {
                horizontalSpeed = normalSpeed;
                nextBehaviorChangeTime = Time.time + Random.Range(1f, 3f);

                float randomRotation = Random.Range(-180f, 180f);
                StartCoroutine(SmoothRotate(randomRotation, nextBehaviorChangeTime - Time.time));
            }
            else
            {
                horizontalSpeed = rushSpeed;
                horizontalDirection = (Random.value > 0.7f) ? GetPlayerDirection() : Random.Range(-1, 2);
                nextBehaviorChangeTime = Time.time + Random.Range(0.3f, 1f);
            }

            isRushing = !isRushing;
        }

        Vector2 movement = new Vector2(horizontalDirection * horizontalSpeed, -speed);
        rb.velocity = movement;
    }

    private void ChangeDirection()
    {
        if (ID == 0) return;

        horizontalDirection = Random.Range(-1, 2);
        directionChangeInterval = Random.Range(minInterval, maxInterval);
    }

    private int GetPlayerDirection()
    {
        if (player != null && player.transform.position.x > transform.position.x)
            return 1;
        else
            return -1;
    }
    #endregion

    #region 状态和生命周期管理
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        bossOnAirTime = 0;

        AudioManager.Instance.PlayEffectAudio(effectClip,50f);
        if (ID == 2)
        {
            AudioManager.Instance.ChangeBGM();
        }

        ReturnToPool();
    }

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

    private void ReturnToPool()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        StopAllCoroutines();
        gameObject.SetActive(false);

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ReturnEnemyToPool(this.gameObject);
        }
    }
    #endregion

    #region 协程方法
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
    #endregion
}