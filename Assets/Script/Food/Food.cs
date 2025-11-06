using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Food : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

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
        
        isInitialized = true;
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
            timeCount = 0;
            returnToPool?.Invoke(this.gameObject);
        }

    #endregion
    }

    private void OnEnable()
    {
        // 立即应用，不要延迟
        if (currentFoodData != null && isInitialized)
        {
            ApplyFoodData(currentFoodData);
        }

        ApplyRandomForce();
        timeCount = 0f;
    }

// 移除 DelayedApplyFoodData 协程

// 移除 DelayedApplyFoodData 协程，它可能导致时机问题

    // 延迟应用食物数据，确保组件已初始化
    private IEnumerator DelayedApplyFoodData()
    {
        yield return null; // 等待一帧
        ApplyFoodData(currentFoodData);
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
        }
        
        if (rb != null)
        {
            rb.gravityScale = data.GravityScale;
        }
        
        // 强制调整食物大小和碰撞体
        ForceAdjustFoodSizeAndCollider();
    }

    // 强制调整食物大小和碰撞体
    private void ForceAdjustFoodSizeAndCollider()
    {
        if (currentFoodData == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // 完全重置状态
        transform.localScale = Vector3.one;

        // 直接使用固定的 5x5 缩放
        transform.localScale = new Vector3(currentFoodData.DisplayScale, currentFoodData.DisplayScale, 1f);

        Debug.Log($"应用固定缩放: {currentFoodData.FoodName} -> 5x5");

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
        // 重置状态
        isInitialized = false;
    }

    // 方法注入
    public void SetReturnToPool(Action<GameObject> callBack)
    {
        returnToPool = callBack;
    }

    // 物品效果
    public void ApplyEffect(PlayerController player)
    {
        // 通用效果
        NormalEffect(player);
    }

    // 通用效果
    public void NormalEffect(PlayerController player)
    {
        player.Attack += currentFoodData.AddAttack;
        player.Health += currentFoodData.AddHeath;
        player.AttackSpeed += currentFoodData.AddAttackSpeed;
        player.MaxSpeed += currentFoodData.AddSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Mouth"))
        {
            return;
        }

        // 获取玩家对象
        var playerController = other.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            ApplyEffect(playerController);
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


    // 重置食物状态（用于对象池）
    public void ResetFood()
    {
        // 完全重置所有状态
        transform.localScale = Vector3.one;
    
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
        }
    
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    
        currentFoodData = null;
        timeCount = 0f;
    
        Debug.Log("食物已重置");
    }
}