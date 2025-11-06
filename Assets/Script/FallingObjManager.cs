using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FallingObjManager : SingletonMono<FallingObjManager>
{
    // 对象池
    private Queue<GameObject> objectPool;

    // 食物预制体
    [SerializeField]
    private GameObject food;

    // 计时器
    private float timeCount;

    // 食物列表
    [SerializeField]
    private List<FoodData> foodList;

    public Dictionary<FoodName, FoodData> foodDictionary;

    // 类型权重配置
    [System.Serializable]
    public class FoodTypeWeight
    {
        public FoodType foodType;
        public float baseWeight; // 基础权重
        public float levelFactor; // 等级影响因子
    }

    [SerializeField]
    private List<FoodTypeWeight> foodTypeWeights;

    // 刷新配置
    [SerializeField]
    private float baseSpawnInterval = 2f; // 基础刷新间隔

    [SerializeField]
    private float minSpawnInterval = 0.5f; // 最小刷新间隔

    [SerializeField]
    private int maxFoodPerSpawn = 3; // 单次最大生成数量

    // 位置生成配置
    [SerializeField]
    private float spawnAreaWidth = 5f; // 生成区域宽度

    [SerializeField]
    private float minDistanceBetweenFood = 1f; // 食物间最小距离

    private PlayerController playerController;
    private List<Vector3> recentSpawnPositions; // 最近生成位置记录

    protected override void Awake()
    {
        base.Awake();
        objectPool = new Queue<GameObject>();
        foodDictionary = new Dictionary<FoodName, FoodData>();
        recentSpawnPositions = new List<Vector3>();

        // 列表转字典
        foreach (var data in foodList)
        {
            if (!foodDictionary.ContainsKey(data.FoodName))
            {
                foodDictionary.Add(data.FoodName, data);
            }
        }

        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found!");
        }

        Init();
    }

    private void Update()
    {
        if (playerController == null) return;

        timeCount += Time.deltaTime;

        // 动态计算刷新间隔（随等级提高刷新加快）
        float currentSpawnInterval = Mathf.Max(minSpawnInterval,
            baseSpawnInterval - (playerController.level * 0.4f));

        if (timeCount >= currentSpawnInterval)
        {
            // 根据玩家等级决定单次生成数量
            int spawnCount = CalculateSpawnCount();

            // 获取均匀分布的位置
            List<Vector3> spawnPositions = GetUniformSpawnPositions(spawnCount);

            for (int i = 0; i < spawnCount && i < spawnPositions.Count; i++)
            {
                SpawnRandomFood(spawnPositions[i]);
            }

            timeCount = 0;
        }

        // 清理过时的位置记录（每帧清理一次，保持列表不会太大）
        if (recentSpawnPositions.Count > 10)
        {
            recentSpawnPositions.RemoveAt(0);
        }
    }

    /// <summary>
    /// 获取均匀分布的生成位置
    /// </summary>
    private List<Vector3> GetUniformSpawnPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();

        if (count <= 0) return positions;

        // 如果只生成一个，使用简单随机位置
        if (count == 1)
        {
            float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
            Vector3 position = new Vector3(
                transform.position.x + randomX,
                transform.position.y,
                transform.position.z
            );
            positions.Add(position);
            return positions;
        }

        // 多个食物时使用均匀分布
        float segmentWidth = spawnAreaWidth / (count + 1);

        for (int i = 0; i < count; i++)
        {
            // 在分段内随机位置，避免完全对齐
            float minX = -spawnAreaWidth / 2 + (i + 0.25f) * segmentWidth;
            float maxX = -spawnAreaWidth / 2 + (i + 0.75f) * segmentWidth;

            float randomX = Random.Range(minX, maxX);

            Vector3 position = new Vector3(
                transform.position.x + randomX,
                transform.position.y,
                transform.position.z
            );

            // 检查是否与最近生成的位置太近
            if (!IsPositionTooCloseToRecentSpawns(position))
            {
                positions.Add(position);
                recentSpawnPositions.Add(position); // 记录这个位置
            }
            else
            {
                // 如果太近，尝试找一个替代位置
                Vector3 alternativePosition = FindAlternativePosition(positions);
                if (alternativePosition != Vector3.zero)
                {
                    positions.Add(alternativePosition);
                    recentSpawnPositions.Add(alternativePosition);
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// 检查位置是否与最近生成的位置太近
    /// </summary>
    private bool IsPositionTooCloseToRecentSpawns(Vector3 position)
    {
        foreach (var recentPos in recentSpawnPositions)
        {
            if (Vector3.Distance(position, recentPos) < minDistanceBetweenFood)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 寻找替代位置
    /// </summary>
    private Vector3 FindAlternativePosition(List<Vector3> existingPositions)
    {
        // 尝试几次寻找合适位置
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
            Vector3 testPosition = new Vector3(
                transform.position.x + randomX,
                transform.position.y,
                transform.position.z
            );

            // 检查是否与现有位置和最近位置都保持距离
            bool validPosition = true;

            // 检查与本次生成的其他位置
            foreach (var pos in existingPositions)
            {
                if (Vector3.Distance(testPosition, pos) < minDistanceBetweenFood)
                {
                    validPosition = false;
                    break;
                }
            }

            // 检查与最近生成的位置
            if (validPosition && IsPositionTooCloseToRecentSpawns(testPosition))
            {
                validPosition = false;
            }

            if (validPosition)
            {
                return testPosition;
            }
        }

        // 如果找不到理想位置，返回一个随机位置
        float fallbackX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        return new Vector3(
            transform.position.x + fallbackX,
            transform.position.y,
            transform.position.z
        );
    }

    /// <summary>
    /// 计算单次生成数量
    /// </summary>
    private int CalculateSpawnCount()
    {
        if (playerController == null) return 1;

        int level = playerController.level;

        // 基础概率表：等级 -> [最小数量, 最大数量]
        var spawnTable = new Dictionary<int, Vector2Int>
        {
            { 1, new Vector2Int(1, 1) },
            { 2, new Vector2Int(1, 2) },
            { 3, new Vector2Int(1, 2) },
            { 4, new Vector2Int(1, 3) },
            { 5, new Vector2Int(2, 3) }
        };

        Vector2Int range = spawnTable.ContainsKey(level) ? spawnTable[level] : new Vector2Int(1, maxFoodPerSpawn);

        return Random.Range(range.x, range.y + 1);
    }

    /// <summary>
    /// 生成随机类型的食物
    /// </summary>
    private void SpawnRandomFood(Vector3 spawnPosition)
    {
        FoodData foodData = GetRandomFoodByType();
        if (foodData != null)
        {
            GetOrCreatFood(foodData, spawnPosition);
        }
    }

    /// <summary>
    /// 根据类型权重随机选择食物
    /// </summary>
    private FoodData GetRandomFoodByType()
    {
        if (playerController == null || foodList.Count == 0)
            return foodList.Count > 0 ? foodList[0] : null;

        int playerLevel = playerController.level;

        // 计算各类型的动态权重
        Dictionary<FoodType, float> currentWeights = new Dictionary<FoodType, float>();
        float totalWeight = 0f;

        // 初始化所有权重
        foreach (FoodType type in Enum.GetValues(typeof(FoodType)))
        {
            currentWeights[type] = 0f;
        }

        // 设置配置的权重
        foreach (var weightConfig in foodTypeWeights)
        {
            // 动态权重 = 基础权重 + 等级 × 影响因子
            float dynamicWeight = weightConfig.baseWeight + (playerLevel * weightConfig.levelFactor);
            currentWeights[weightConfig.foodType] = Mathf.Max(0, dynamicWeight);
            totalWeight += dynamicWeight;
        }

        // 如果没有配置权重，使用默认权重
        if (totalWeight <= 0)
        {
            SetDefaultWeights(currentWeights, playerLevel);
            totalWeight = CalculateTotalWeight(currentWeights);
        }

        // 随机选择类型
        float randomValue = Random.Range(0f, totalWeight);
        float current = 0f;
        FoodType selectedType = FoodType.smail;

        foreach (var kvp in currentWeights)
        {
            current += kvp.Value;
            if (randomValue <= current && kvp.Value > 0)
            {
                selectedType = kvp.Key;
                break;
            }
        }

        // 从该类型的食物中随机选择一个
        var typeFoods = foodList.FindAll(food => food.Type == selectedType);
        if (typeFoods.Count > 0)
        {
            return typeFoods[Random.Range(0, typeFoods.Count)];
        }

        // 如果没有该类型食物，返回第一个食物
        return foodList[0];
    }

    /// <summary>
    /// 设置默认权重
    /// </summary>
    private void SetDefaultWeights(Dictionary<FoodType, float> weights, int playerLevel)
    {
        // 默认权重配置
        weights[FoodType.smail] = Mathf.Max(0, 40 - (playerLevel * 2)); // 小食物随等级减少
        weights[FoodType.normal] = 30; // 普通食物保持不变
        weights[FoodType.big] = Mathf.Max(0, 5 + (playerLevel * 1)); // 大食物随等级增加
        weights[FoodType.bad] = Mathf.Max(0, 25 + (playerLevel * 1)); // 坏食物随等级减少
    }

    /// <summary>
    /// 计算总权重
    /// </summary>
    private float CalculateTotalWeight(Dictionary<FoodType, float> weights)
    {
        float total = 0f;
        foreach (var weight in weights.Values)
        {
            total += weight;
        }

        return total;
    }

    private void Init()
    {
        // 初始化对象池
        if (objectPool == null)
        {
            objectPool = new Queue<GameObject>();
        }

        // 创建对象池内默认对象
        if (objectPool.Count == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                CreateNewPoolObject();
            }
        }
    }

    /// <summary>
    /// 创建新的池对象
    /// </summary>
    private void CreateNewPoolObject()
    {
        GameObject newFood = GameObject.Instantiate<GameObject>(food, this.transform);
        var foodComponent = newFood.GetComponent<Food>();
        if (foodComponent != null)
        {
            foodComponent.SetReturnToPool(ReturnToFoodPool);
        }

        newFood.SetActive(false);
        objectPool.Enqueue(newFood);
    }

    // 返回对象池
    private void ReturnToFoodPool(GameObject aFood)
    {
        if (aFood == null)
        {
            return;
        }

        // 重置食物状态
        var foodComponent = aFood.GetComponent<Food>();
        if (foodComponent != null)
        {
            foodComponent.ResetFood();
        }

        aFood.SetActive(false);
        // 重置食物位置到管理器位置
        aFood.transform.position = this.transform.position;
        aFood.transform.SetParent(this.transform);

        if (objectPool == null)
        {
            objectPool = new Queue<GameObject>();
        }

        objectPool.Enqueue(aFood);
    }

    // 执行食物掉落（使用指定位置）
    private GameObject GetOrCreatFood(FoodData foodData, Vector3 spawnPosition)
    {
        if (objectPool == null || objectPool.Count == 0)
        {
            CreateNewPoolObject();
        }

        GameObject aFood = objectPool.Dequeue();
        var foodComponent = aFood.GetComponent<Food>();

        if (foodComponent != null)
        {
            // 重置食物状态
            foodComponent.ResetFood();
        
            // 设置新的数据
            foodComponent.CurrentFoodData = foodData;
            foodComponent.SetReturnToPool(ReturnToFoodPool);
        }

        // 设置位置和激活
        aFood.transform.position = spawnPosition;
        aFood.SetActive(true);
    
        return aFood;
    }

    /// <summary>
    /// 初始化掉落物管理器，重置所有状态
    /// </summary>
    public void InitializeFallingObjManager()
    {
        // 重置计时器
        timeCount = 0f;

        // 清空位置记录
        recentSpawnPositions.Clear();

        // 回收所有已激活的食物对象
        ReturnAllActiveFoodsToPool();

        // 重新初始化对象池
        ReinitializePool();

        Debug.Log("掉落物管理器已初始化");
    }

    /// <summary>
    /// 回收所有活跃的食物对象到对象池
    /// </summary>
    private void ReturnAllActiveFoodsToPool()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                Food foodComponent = child.GetComponent<Food>();
                if (foodComponent != null)
                {
                    foodComponent.ResetFood();
                    ReturnToFoodPool(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 重新初始化对象池
    /// </summary>
    private void ReinitializePool()
    {
        if (objectPool != null)
        {
            objectPool.Clear();
        }
        else
        {
            objectPool = new Queue<GameObject>();
        }

        foreach (Transform child in transform)
        {
            if (child != null && child.gameObject != null)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < 10; i++)
        {
            CreateNewPoolObject();
        }
    }

    /// <summary>
    /// 根据食物名称直接获取食物（用于特定情况）
    /// </summary>
    public void SpawnSpecificFood(FoodName foodName)
    {
        if (foodDictionary.ContainsKey(foodName))
        {
            Vector3 spawnPosition = GetUniformSpawnPositions(1)[0];
            GetOrCreatFood(foodDictionary[foodName], spawnPosition);
        }
        else
        {
            Debug.LogWarning($"食物 {foodName} 不存在于字典中");
        }
    }

    // 在Inspector中可视化生成区域（调试用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(spawnAreaWidth, 0.2f, 0);
        Gizmos.DrawWireCube(center, size);
    }
}