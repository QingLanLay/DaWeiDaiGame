using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Food : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    [FormerlySerializedAs("Mouth")]
    public GameObject mouth;
    public BoxCollider2D boxCollider2D;
    public bool isEat = false;
    
    [Header("发光效果")]
    [SerializeField] private Color glowColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private float glowSize = 0.02f;
    [SerializeField] private float glowStrength = 1.5f;
    
    private SpriteGlowEffect glowEffect;
    
    // 食物列表与数据
    [SerializeField]
    private FoodData currentFoodData;

    public FoodData CurrentFoodData
    {
        get => currentFoodData;
        set 
        { 
            currentFoodData = value;
            // 当设置新的FoodData时，立即应用
            if (currentFoodData != null)
            {
                ApplyFoodData(currentFoodData);
            }
        }
    }

    // 计时器
    private float timeCount;

    // 返回对象池委托
    public Action<GameObject> returnToPool;

    // 标记是否已初始化
    private bool isInitialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 如果没有碰撞体，自动添加一个
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider2D = GetComponent<BoxCollider2D>();
        // 初始化发光效果
        InitializeGlowEffect();
        
        isInitialized = true;
    }
    
    private void InitializeGlowEffect()
    {
        // 添加发光组件（确保组件存在）
        glowEffect = gameObject.GetComponent<SpriteGlowEffect>();
        if (glowEffect == null)
        {
            glowEffect = gameObject.AddComponent<SpriteGlowEffect>();
        }
        
        // 初始配置发光效果
        UpdateGlowEffect();
    }

    // 更新发光效果参数
    private void UpdateGlowEffect()
    {
        if (glowEffect != null)
        {
            glowEffect.SetGlowColor(glowColor);
            glowEffect.SetGlowSize(glowSize);
            glowEffect.SetGlowStrength(glowStrength);
        }
    }

    public void SetFoodType(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.smail: // 普通食物 - 绿色发光
                glowColor = new Color(0.3f, 1f, 0.3f, 1f);
                glowSize = 3f;
                glowStrength = 1.3f;
                break;
            case FoodType.normal: // 中型食物 - 蓝色发光
                glowColor = new Color(0.3f, 0.6f, 1f, 1f);
                glowSize = 3f;
                glowStrength = 1.6f;
                break;
            case FoodType.big: // 大型食物 - 金色发光
                glowColor = new Color(1f, 0.8f, 0.2f, 1f);
                glowSize = 3f;
                glowStrength = 2f;
                break;
        }
        
        // 立即更新发光效果
        UpdateGlowEffect();
    }

    private void Start() 
    {
        // 确保在Start中也应用一次数据
        if (currentFoodData != null)
        {
            ApplyFoodData(currentFoodData);
        }
    }

    private void Update()
    {
    #region 测试单元

        // 十秒后自动清除
        timeCount += Time.deltaTime;
        if (timeCount >= 10f)
        {
            // 消失前的闪烁效果
            StartCoroutine(BlinkBeforeDestroy());
        }

        if (isEat)
        {
            MoveTowardsPlayer();
        }

    #endregion
    }

#region 追踪
    [Header("追踪设置")]
    private Vector3 currentVelocity = Vector3.zero;
    [SerializeField] private float attractionRange = 3f; // 吸引范围
    [SerializeField] private float maxAttractionSpeed = 100f; // 最大吸引速度
    [SerializeField] private float acceleration = 15f; // 加速度
    [SerializeField] private float rotationSpeed = 180f; // 旋转速度
    private void MoveTowardsPlayer()
    {
        if (mouth == null) return;
        
        Vector3 targetPosition = mouth.transform.position;
        rb.velocity = Vector3.zero;
        // 使用平滑阻尼实现流畅的追踪移动
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            0.1f, // 平滑时间
            maxAttractionSpeed // 最大速度
        );
        
        // 添加旋转效果
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        
    }

#endregion

    // 消失前的闪烁效果
    private IEnumerator BlinkBeforeDestroy()
    {
        // 改变发光颜色作为警告
        if (glowEffect != null)
        {
            glowEffect.SetGlowColor(Color.yellow);
            glowEffect.SetGlowStrength(2.5f);
        }

        // 闪烁3次
        for (int i = 0; i < 3; i++)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
                yield return new WaitForSeconds(0.15f);
                spriteRenderer.enabled = true;
                yield return new WaitForSeconds(0.15f);
            }
        }
        
        // 返回到对象池
        timeCount = 0;
        returnToPool?.Invoke(this.gameObject);
    }

    private void OnEnable()
    {
        if (!boxCollider.enabled)
        {
            boxCollider.enabled = true;
        }
        currentVelocity = Vector3.zero;
        // 立即应用，不要延迟
        if (currentFoodData != null && isInitialized)
        {
            ApplyFoodData(currentFoodData);
            isEat = false;
        }
        
        // 设置食物类型
        if (currentFoodData != null)
        {
            SetFoodType(currentFoodData.Type);
        }
        
        ApplyRandomForce();
        timeCount = 0f;
        
        // 确保发光效果启用
        if (glowEffect != null)
        {
            glowEffect.glowEnabled = true;
            glowEffect.UpdateGlow();
        }

        mouth = GameObject.FindWithTag("Mouth");
    }

    // 应用食物数据
    public void ApplyFoodData(FoodData data)
    {
        if (data == null) return;
        
        currentFoodData = data;
        
        // 确保组件已获取
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        
        if (spriteRenderer != null)
        {
            // 设置新精灵
            spriteRenderer.sprite = data.Icon;
            
            // 更新发光材质的纹理
            if (glowEffect != null && glowEffect.glowMaterial != null && data.Icon != null)
            {
                glowEffect.glowMaterial.mainTexture = data.Icon.texture;
            }
        }
        
        if (rb != null)
        {
            rb.gravityScale = data.GravityScale;
        }
        
        // 强制调整食物大小和碰撞体
        ForceAdjustFoodSizeAndCollider();
        
        // 更新发光效果
        UpdateGlowEffect();
    }

    // 强制调整食物大小和碰撞体
    private void ForceAdjustFoodSizeAndCollider()
    {
        if (currentFoodData == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // 完全重置状态
        transform.localScale = Vector3.one;

        // 直接使用固定的缩放
        transform.localScale = new Vector3(currentFoodData.DisplayScale, currentFoodData.DisplayScale, 1f);

        Debug.Log($"应用固定缩放: {currentFoodData.FoodName} -> {currentFoodData.DisplayScale}");

        // 更新碰撞体
        UpdateColliderToMatchSprite();
    }

    // 更新碰撞体大小以匹配精灵
    private void UpdateColliderToMatchSprite()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        if (boxCollider == null) return;

        // 获取精灵的原始边界大小（不考虑缩放）
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        
        // 设置碰撞体大小为精灵的原始大小
        // 缩放会在物理引擎中自动应用
        boxCollider.size = spriteSize;
        
        // 重置碰撞体偏移
        boxCollider.offset = Vector2.zero;
        
        Debug.Log($"更新碰撞体: {currentFoodData.FoodName}, 大小: {spriteSize}");
    }

    // 清空FoodData
    private void OnDisable() 
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 重置状态
        isInitialized = false;
        
        // 禁用发光效果
        if (glowEffect != null)
        {
            glowEffect.glowEnabled = false;
            glowEffect.UpdateGlow();
        }
    }

    // 方法注入
    public void SetReturnToPool(Action<GameObject> callBack)
    {
        returnToPool = callBack;
    }

    // 物品效果
    public void ApplyEffect(PlayerController player)
    {
        // 播放收集特效
        PlayCollectEffect();
        
        // 通用效果
        NormalEffect(player);
    }

    // 通用效果
    public void NormalEffect(PlayerController player)
    {
        player.Attack += currentFoodData.AddAttack;
        player.Health += currentFoodData.AddHeath;
        player.AttackSpeed += currentFoodData.AddAttackSpeed;
        player.currentSpeed += currentFoodData.AddSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Mouth"))
        {
            return;
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Mouth"))
        {
            return;
        }
    }

    private void ApplyRandomForce()
    {
        // 生成随机方向
        float direction = Random.Range(0, 2f) * 2 - 1;
        // 生成随机大小的力
        float forceMagnitude = Random.Range(0f, 1f);
        // 计算力的最终方向
        Vector2 force = new Vector2(direction * forceMagnitude, 0f);

        // 施加力
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
    
    // 播放收集特效
    private void PlayCollectEffect()
    {
        // 强光脉冲效果
        if (glowEffect != null)
        {
            StartCoroutine(CollectGlowEffect());
        }
        
        // 缩放动画
        StartCoroutine(CollectAnimation());
        
        Debug.Log($"获得食物: {currentFoodData.FoodName}");
    }
    
    // 收集时的发光效果
    private IEnumerator CollectGlowEffect()
    {
        Color originalColor = glowColor;
        float originalStrength = glowStrength;
        
        // 增强发光
        glowEffect.SetGlowColor(Color.white);
        glowEffect.SetGlowStrength(originalStrength * 2f);
        
        yield return new WaitForSeconds(0.2f);
        
        // 恢复原状（虽然马上就要被回收了）
        glowEffect.SetGlowColor(originalColor);
        glowEffect.SetGlowStrength(originalStrength);
    }

    // 收集动画
    private IEnumerator CollectAnimation()
    {
        float duration = 0.3f;
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            
            // 先放大后缩小
            float scale = progress < 0.5f ? 
                Mathf.Lerp(1f, 1.3f, progress * 2f) : 
                Mathf.Lerp(1.3f, 0.5f, (progress - 0.5f) * 2f);
            
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    // 重置食物状态（用于对象池）
    public void ResetFood()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 完全重置所有状态
        transform.localScale = Vector3.one;
    
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = true;
        }
    
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    
        currentFoodData = null;
        timeCount = 0f;
        
        // 重置发光效果
        if (glowEffect != null)
        {
            glowEffect.glowEnabled = true;
            UpdateGlowEffect();
        }
    
        Debug.Log("食物已重置");
    }
}