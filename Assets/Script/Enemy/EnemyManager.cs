using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : SingletonMono<EnemyManager>
{
    [Header("基础配置")]
    [SerializeField] private GameObject defaultEnemy;
    [SerializeField] private List<EnemyData> enemyList;
    
    [Header("波次配置")]
    [SerializeField] private float baseEnemySpawnInterval = 3f;
    [SerializeField] private int enemiesPerWave = 8;
    [SerializeField] private List<int> normalEnemyIDs = new List<int> { 0 };
    [SerializeField] private List<int> eliteEnemyIDs = new List<int> { 1 };
    [SerializeField] private List<int> bossEnemyIDs = new List<int> { 2 };

    // 核心变量
    private Dictionary<int, EnemyData> enemyDic;
    private Queue<GameObject> enemyPool;
    private PlayerController playerController;
    private GameObject currentBoss;
    
    // 简化后的状态管理
    private int currentWave = 1;
    private int enemiesSpawnedThisWave = 0;
    private float spawnTimer = 0f;
    private bool isBossWave = false;

    // 修复：添加准确的活跃敌人计数
    private int activeEnemyCount = 0;

    protected override void Awake()
    {
        base.Awake();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        
        // 初始化敌人字典
        enemyDic = new Dictionary<int, EnemyData>();
        foreach (var enemy in enemyList)
        {
            enemyDic.Add(enemy.ID, enemy);
        }

        InitializePool();
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        enemyPool = new Queue<GameObject>();
        for (int i = 0; i < 20; i++)
        {
            var enemy = Instantiate(defaultEnemy, transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
        Debug.Log($"对象池初始化完成，创建了 {enemyPool.Count} 个敌人");
    }

    void Update()
    {
        if (playerController == null) return;

        if (isBossWave)
        {
            HandleBossWave();
        }
        else
        {
            HandleNormalWave();
        }
    }

    /// <summary>
    /// 处理普通波次 - 修复生成逻辑
    /// </summary>
    private void HandleNormalWave()
    {
        // 只有在需要生成更多敌人时才计时
        if (enemiesSpawnedThisWave < enemiesPerWave)
        {
            spawnTimer += Time.deltaTime;
            
            float currentInterval = GetSpawnInterval();
            
            // 检查是否需要生成敌人 - 修复条件
            if (spawnTimer >= currentInterval)
            {
                SpawnEnemy(false);
                spawnTimer = 0f;
                
                // 调试信息
                Debug.Log($"生成敌人 - 波次: {currentWave}, 进度: {enemiesSpawnedThisWave}/{enemiesPerWave}, " +
                         $"活跃敌人: {activeEnemyCount}, 对象池: {enemyPool.Count}, 间隔: {currentInterval:F2}s");
            }
        }

        // 检查是否应该进入BOSS波次
        if (enemiesSpawnedThisWave >= enemiesPerWave && activeEnemyCount == 0)
        {
            StartBossWave();
        }
    }

    /// <summary>
    /// 处理BOSS波次
    /// </summary>
    private void HandleBossWave()
    {
        // BOSS被击败
        if (currentBoss == null || !currentBoss.activeInHierarchy)
        {
            CompleteBossWave();
        }
    }

    /// <summary>
    /// 生成敌人 - 修复活跃计数
    /// </summary>
    private void SpawnEnemy(bool isBoss)
    {
        // 安全检查 - 使用准确的活跃计数
        if (activeEnemyCount >= 15) 
        {
            Debug.LogWarning($"活跃敌人已达上限({activeEnemyCount})，暂停生成");
            return;
        }

        // 获取敌人数据
        EnemyData enemyData = isBoss ? GetBossEnemy() : GetNormalEnemy();
        if (enemyData == null)
        {
            Debug.LogError("无法获取敌人数据！");
            return;
        }

        // 从对象池获取敌人
        GameObject enemy = GetEnemyFromPool();
        if (enemy == null)
        {
            Debug.LogError("无法从对象池获取敌人！");
            return;
        }

        // 配置敌人
        var enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.GetEnemyData(enemyData);
        }

        // 设置位置和激活
        enemy.transform.position = GetSpawnPosition();
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);

        // 更新活跃计数
        activeEnemyCount++;

        // 记录BOSS
        if (isBoss)
        {
            currentBoss = enemy;
            Debug.Log($"BOSS已生成！第 {currentWave} 波");
        }
        else
        {
            enemiesSpawnedThisWave++;
        }
    }

    /// <summary>
    /// 从对象池获取敌人
    /// </summary>
    private GameObject GetEnemyFromPool()
    {
        // 清理空引用
        enemyPool = new Queue<GameObject>(enemyPool.Where(x => x != null));

        if (enemyPool.Count > 0)
        {
            return enemyPool.Dequeue();
        }
        else
        {
            // 池子空了就创建新敌人
            Debug.LogWarning("对象池为空，创建新敌人");
            var newEnemy = Instantiate(defaultEnemy, transform);
            return newEnemy;
        }
    }

    /// <summary>
    /// 回收敌人到对象池 - 修复活跃计数
    /// </summary>
    public void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        // 如果是BOSS被回收
        if (enemy == currentBoss)
        {
            currentBoss = null;
        }

        var enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Dead();
        }

        enemy.SetActive(false);
        enemy.transform.SetParent(transform);

        if (!enemyPool.Contains(enemy))
        {
            enemyPool.Enqueue(enemy);
            
            // 修复：减少活跃计数
            activeEnemyCount--;
            if (activeEnemyCount < 0) activeEnemyCount = 0;
            
            Debug.Log($"敌人回收成功 - 活跃敌人: {activeEnemyCount}, 对象池: {enemyPool.Count}");
        }
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        float range = 2.5f;
        return new Vector3(
            transform.position.x + Random.Range(-range, range),
            transform.position.y,
            transform.position.z
        );
    }

    /// <summary>
    /// 获取普通敌人类型
    /// </summary>
    private EnemyData GetNormalEnemy()
    {
        List<int> availableEnemies;

        // 第一波只使用普通敌人
        if (currentWave == 1)
        {
            availableEnemies = normalEnemyIDs;
        }
        else
        {
            // 后续波次随机选择普通或精英敌人
            availableEnemies = Random.Range(0f, 1f) > 0.7f ? eliteEnemyIDs : normalEnemyIDs;
        }

        if (availableEnemies.Count > 0)
        {
            int enemyID = availableEnemies[Random.Range(0, availableEnemies.Count)];
            return enemyDic.ContainsKey(enemyID) ? enemyDic[enemyID] : null;
        }

        return enemyDic.Count > 0 ? enemyDic[0] : null;
    }

    /// <summary>
    /// 获取BOSS敌人类型
    /// </summary>
    private EnemyData GetBossEnemy()
    {
        if (bossEnemyIDs.Count > 0)
        {
            int bossID = bossEnemyIDs[Random.Range(0, bossEnemyIDs.Count)];
            return enemyDic.ContainsKey(bossID) ? enemyDic[bossID] : null;
        }

        return enemyDic.Count > 0 ? enemyDic[0] : null;
    }

    /// <summary>
    /// 获取生成间隔
    /// </summary>
    private float GetSpawnInterval()
    {
        float interval = baseEnemySpawnInterval;
        
        // 随波次增加而加快
        interval -= (currentWave - 1) * 0.5f;
        
        // 确保不会太快
        return Mathf.Max(0.8f, interval);
    }

    /// <summary>
    /// 开始BOSS波次
    /// </summary>
    private void StartBossWave()
    {
        isBossWave = true;
        SpawnEnemy(true);
        Debug.Log($"BOSS波次开始！第 {currentWave} 波");
    }

    /// <summary>
    /// 完成BOSS波次
    /// </summary>
    private void CompleteBossWave()
    {
        isBossWave = false;
        currentWave++;
        enemiesSpawnedThisWave = 0;
        spawnTimer = 0f;

        Debug.Log($"🎉 第 {currentWave - 1} 波完成！开始第 {currentWave} 波");

        // 每波增加难度
        if (currentWave % 1 == 0)
        {
            enemiesPerWave += 2;
            Debug.Log($"每波敌人数增加到: {enemiesPerWave}");
        }

        // 延迟后开始新波次
        StartCoroutine(StartNextWaveCoroutine());
    }

    private IEnumerator StartNextWaveCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SpawnEnemy(false); // 开始新波次
    }

    /// <summary>
    /// 获取活跃敌人数量 - 修复计数方法
    /// </summary>
    private int GetActiveEnemyCount()
    {
        // 使用我们维护的准确计数
        return activeEnemyCount;
    }

    // ========== 公开方法 ==========

    /// <summary>
    /// 完全重置敌人管理器
    /// </summary>
    public void InitializeEnemyManager()
    {
        currentWave = 1;
        enemiesSpawnedThisWave = 0;
        spawnTimer = 0f;
        isBossWave = false;
        currentBoss = null;
        enemiesPerWave = 8;
        activeEnemyCount = 0; // 修复：重置活跃计数

        // 回收所有敌人
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                ReturnEnemyToPool(child.gameObject);
            }
        }

        Debug.Log("敌人管理器已重置");
    }

    /// <summary>
    /// 敌人被击败时调用
    /// </summary>
    public void OnEnemyDefeated()
    {
        // 简化：不需要特殊处理，系统会自动检测
    }

    public int GetCurrentWave() => currentWave;
    public bool IsBossWave() => isBossWave;
    public float GetWaveProgress() => (float)enemiesSpawnedThisWave / enemiesPerWave;
}