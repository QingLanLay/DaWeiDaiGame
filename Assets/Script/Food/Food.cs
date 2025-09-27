using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;


public class Food : MonoBehaviour
{
    private Rigidbody2D rb;

    // 食物列表与数据
    [SerializeField]
    private FoodData currentFoodData;

    public FoodData CurrentFoodData
    {
        get => currentFoodData;
        set => currentFoodData = value;
    }

    // 计时器
    private float timeCount;


    // 返回对象池委托
    public Action<GameObject> returnToPool;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() { }

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

        // Debug.Log($"当前物体的状态" + 
        //           $"currentFoodData: {currentFoodData.name}" +
        //           $"Attack:{currentFoodData.AddAttack}" +
        //           $"Health:{currentFoodData.AddHeath}" +
        //           $"Speed:{currentFoodData.AddSpeed}" +
        //           $"AttackSpeed:{currentFoodData.AddAttackSpeed}"
        //           );

    #endregion
    }

    private void OnEnable()
    {
        // 获取精灵渲染器
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        
        // 属性注入
        if (currentFoodData != null)
        {
            spriteRenderer.sprite = currentFoodData.Icon;
            rb.gravityScale = currentFoodData.GravityScale;
        }

        // 添加随机力
        ApplyRandomForce();

        timeCount = 0f;
    }


    // 清空FoodData
    private void OnDisable() { }

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

        // switch (currentFoodData.ID)
        // {
        //     
        // }
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
        ApplyEffect(playerController);
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
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}