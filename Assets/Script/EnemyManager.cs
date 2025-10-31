using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : SingletonMono<EnemyManager>
{
    [SerializeField]
    private GameObject defualtEnemy;

    [SerializeField]
    private List<EnemyData> enemyList;

    private Dictionary<int, EnemyData> enemyDic;

    public Queue<GameObject> enemyPool;

    private PlayerController playerController;
    public DavidDie davidDie;

    // 计时器
    private float timeCount;
    private float timeBossCount;

    // 波次系统
    private int currentWave = 1;
    private float waveProgress = 0f;
    private bool isBossWave = false;
    private bool waitingForBossSpawn = false;
    private bool bossDefeatedInCurrentWave = false; // 新增：标记当前波次BOSS是否已被击败

    private float exp;

    // 动态难度配置
    [Header("难度配置")]
    [SerializeField]
    private float baseEnemySpawnInterval = 3f;

    [SerializeField]
    private float minEnemySpawnInterval = 0.5f;

    [SerializeField]
    private int enemiesPerWave = 8;

    [SerializeField]
    private float waveProgressPerEnemy = 0.125f;

    // EXP增强配置
    [Header("EXP增强配置")]
    [SerializeField]
    private int expThreshold = 10000;

    [SerializeField]
    private int maxExtraEnemiesFromExp = 10;

    [SerializeField]
    private float expSpawnSpeedBoost = 0.1f;

    // 敌人ID分组配置
    [Header("敌人ID分组配置")]
    [SerializeField]
    private List<int> normalEnemyIDs = new List<int> { 0 };

    [SerializeField]
    private List<int> eliteEnemyIDs = new List<int> { 1 };

    [SerializeField]
    private List<int> bossEnemyIDs = new List<int> { 2 };

    // 敌人类型权重配置
    [Header("敌人类型权重")]
    [SerializeField]
    private float normalEnemyBaseWeight = 60f;

    [SerializeField]
    private float normalEnemyLevelFactor = -5f;

    [SerializeField]
    private float normalEnemyWaveFactor = -3f;

    [SerializeField]
    private float eliteEnemyBaseWeight = 30f;

    [SerializeField]
    private float eliteEnemyLevelFactor = 2f;

    [SerializeField]
    private float eliteEnemyWaveFactor = 2f;

    protected override void Awake()
    {
        base.Awake();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        enemyDic = new Dictionary<int, EnemyData>();

        foreach (var enemy in enemyList)
        {
            enemyDic.Add(enemy.ID, enemy);
        }

        Initializer();
    }

    private void Initializer()
    {
        if (enemyPool == null)
        {
            enemyPool = new Queue<GameObject>();
        }

        if (enemyPool.Count == 0)
        {
            for (int i = 0; i < 15; i++)
            {
                var enemy = GameObject.Instantiate(defualtEnemy, transform);
                enemyPool.Enqueue(enemy);
                enemy.SetActive(false);
            }
        }
    }

    void Update()
    {
        exp = davidDie.currentExp;

        if (playerController == null) return;

        UpdateWaveSystem();
        UpdateEnemySpawning();
        UpdateBossSpawning();
        
        DebugEnemyWeights();
    }

    // ========== 敌人生成相关方法 ==========

    /// <summary>
    /// 获取或创建敌人
    /// </summary>
    private GameObject GetOrCreatEnemy(bool boss)
    {
        if (playerController == null)
            return null;

        EnemyData enemyData = GetEnemyType(boss);
        if (enemyData == null)
        {
            Debug.LogError("EnemyData is null!");
            return null;
        }

        float range = GetSpawnPosition();
        GameObject enemy = null;

        enemyPool = new Queue<GameObject>(enemyPool.Where(x => x != null));

        if (enemyPool.Count > 0)
        {
            enemy = enemyPool.Dequeue();
            if (enemy == null)
                return GetOrCreatEnemy(boss);
        }
        else
        {
            enemy = GameObject.Instantiate(defualtEnemy, transform);
        }

        var enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.GetEnemyData(enemyData);
        }

        enemy.transform.position = new Vector3(
            transform.position.x + range,
            transform.position.y,
            transform.position.z
        );
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);

        return enemy;
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    private float GetSpawnPosition()
    {
        float baseRange = 2.5f;
        return Random.Range(-baseRange, baseRange);
    }

    /// <summary>
    /// 根据当前状态获取敌人类型
    /// </summary>
    private EnemyData GetEnemyType(bool boss)
    {
        if (boss)
        {
            return GetBossEnemy();
        }
        else
        {
            return GetNormalEnemy();
        }
    }

    /// <summary>
    /// 获取BOSS敌人
    /// </summary>
    private EnemyData GetBossEnemy()
    {
        int playerLevel = playerController.level;

        if (bossEnemyIDs.Count > 0)
        {
            int selectedIndex;

            if (playerLevel >= 10)
            {
                selectedIndex = bossEnemyIDs.Count - 1;
            }
            else if (playerLevel >= 5)
            {
                selectedIndex = Mathf.Min(bossEnemyIDs.Count - 1,
                    Random.Range(bossEnemyIDs.Count / 2, bossEnemyIDs.Count));
            }
            else if (playerLevel >= 3)
            {
                selectedIndex = Random.Range(0, bossEnemyIDs.Count);
            }
            else
            {
                selectedIndex = Mathf.Min(bossEnemyIDs.Count - 1,
                    Random.Range(0, Mathf.Max(1, bossEnemyIDs.Count / 2)));
            }

            int bossID = bossEnemyIDs[selectedIndex];
            Debug.Log($"Spawning Boss: ID {bossID}, Wave {currentWave}, Player Level {playerLevel}");
            return enemyDic[bossID];
        }

        Debug.LogWarning("No boss enemies configured!");
        return enemyDic.Count > 0 ? enemyDic[0] : null;
    }

    /// <summary>
    /// 获取普通敌人
    /// </summary>
    private EnemyData GetNormalEnemy()
    {
        int playerLevel = playerController.level;

        // 第一波强制只使用普通敌人
        if (currentWave == 1)
        {
            if (normalEnemyIDs.Count > 0)
            {
                int selectedEnemyID = normalEnemyIDs[Random.Range(0, normalEnemyIDs.Count)];
                return enemyDic[selectedEnemyID];
            }

            return enemyDic.Count > 0 ? enemyDic[0] : null;
        }

        // 计算权重
        float normalWeight = Mathf.Max(1f, normalEnemyBaseWeight +
                                           (playerLevel * normalEnemyLevelFactor) +
                                           (currentWave * normalEnemyWaveFactor));

        float eliteWeight = Mathf.Max(1f, eliteEnemyBaseWeight +
                                          (playerLevel * eliteEnemyLevelFactor) +
                                          (currentWave * eliteEnemyWaveFactor));

        float totalWeight = normalWeight + eliteWeight;

        // 随机选择敌人类型
        float randomValue = Random.Range(0f, totalWeight);
        List<int> selectedEnemyList;

        if (randomValue <= normalWeight)
        {
            selectedEnemyList = normalEnemyIDs;
        }
        else
        {
            selectedEnemyList = eliteEnemyIDs;
        }

        if (selectedEnemyList.Count > 0)
        {
            int selectedEnemyID = selectedEnemyList[Random.Range(0, selectedEnemyList.Count)];
            return enemyDic[selectedEnemyID];
        }

        Debug.LogWarning($"No enemies in selected list! Using default.");
        return enemyDic.Count > 0 ? enemyDic[0] : null;
    }

    /// <summary>
    /// 将敌人回收到对象池
    /// </summary>
    private void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy == null) return;

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Dead();
        }

        enemy.SetActive(false);
        enemy.transform.SetParent(transform);

        if (enemyPool != null && !enemyPool.Contains(enemy))
        {
            enemyPool.Enqueue(enemy);
        }
    }

    /// <summary>
    /// 回收所有活跃的敌人对象到对象池
    /// </summary>
    private void ReturnAllActiveEnemiesToPool()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                Enemy enemyComponent = child.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    ReturnEnemyToPool(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 重新初始化敌人对象池
    /// </summary>
    private void ReinitializeEnemyPool()
    {
        if (enemyPool != null)
        {
            enemyPool.Clear();
        }
        else
        {
            enemyPool = new Queue<GameObject>();
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 15; i++)
        {
            GameObject newEnemy = Instantiate(defualtEnemy, transform);
            newEnemy.SetActive(false);
            enemyPool.Enqueue(newEnemy);
        }
    }

    // ========== 波次系统相关方法 ==========

    /// <summary>
    /// 更新波次系统
    /// </summary>
    private void UpdateWaveSystem()
    {
        // 每60帧输出一次调试信息
        if (Time.frameCount % 60 < 0.1)
        {
            int extraEnemies = CalculateExtraEnemiesFromExp();
            float speedBoost = CalculateSpawnSpeedBoost();

            Debug.Log($"Wave: {currentWave}, Progress: {waveProgress:F2}, " +
                      $"IsBossWave: {isBossWave}, WaitingForBoss: {waitingForBossSpawn}, " +
                      $"BossDefeated: {bossDefeatedInCurrentWave}, EXP: {exp}");
        }
    }

    /// <summary>
    /// 更新普通敌人生成 - 修复BOSS击败后的状态
    /// </summary>
    private void UpdateEnemySpawning()
    {
        // 只有当不在BOSS波次、不等待BOSS生成、且当前波次BOSS未被击败时才生成普通敌人
        if (isBossWave || waitingForBossSpawn || bossDefeatedInCurrentWave) return;

        timeCount += Time.deltaTime;

        float currentSpawnInterval = CalculateEnemySpawnInterval();

        if (timeCount >= currentSpawnInterval && waveProgress < 1f)
        {
            // 根据EXP决定单次生成数量
            int spawnCount = CalculateSpawnCount();
            bool enemySpawned = false;

            for (int i = 0; i < spawnCount; i++)
            {
                if (GetOrCreatEnemy(false) != null)
                {
                    enemySpawned = true;

                    // 更新波次进度（每个敌人都增加进度）
                    waveProgress += waveProgressPerEnemy;

                    // 如果进度已满，停止生成更多敌人
                    if (waveProgress >= 1f)
                    {
                        break;
                    }
                }
            }

            if (enemySpawned)
            {
                timeCount = 0;
            }

            // 波次完成，准备BOSS战
            if (waveProgress >= 1f)
            {
                StartBossWave();
            }
        }

        // 如果长时间没有生成敌人，强制重置状态
        if (timeCount > 10f && waveProgress < 1f)
        {
            Debug.LogWarning("长时间没有生成敌人，强制重置状态");
            timeCount = 0;
            GetOrCreatEnemy(false);
        }
    }

    /// <summary>
    /// 更新BOSS生成 - 修复状态管理
    /// </summary>
    private void UpdateBossSpawning()
    {
        if (!isBossWave) return;

        timeBossCount += Time.deltaTime;

        if (timeBossCount >= 2f)
        {
            if (GetOrCreatEnemy(true) != null)
            {
                timeBossCount = 0;
                isBossWave = false;
                waitingForBossSpawn = true;
                Debug.Log($"BOSS已生成，等待被击败. Wave {currentWave}");
            }
            else
            {
                Debug.LogError("BOSS生成失败！");
                // BOSS生成失败，强制进入下一波
                ForceNextWave();
            }
        }
    }

    /// <summary>
    /// 开始BOSS波次 - 重置BOSS击败状态
    /// </summary>
    private void StartBossWave()
    {
        isBossWave = true;
        bossDefeatedInCurrentWave = false; // 重置BOSS击败状态
        timeBossCount = 0;
        Debug.Log($"BOSS Wave Started! Wave {currentWave}");
    }

    /// <summary>
    /// 强制进入下一波 - 修复状态重置
    /// </summary>
    public void ForceNextWave()
    {
        currentWave++;
        waveProgress = 0f;
        isBossWave = false;
        waitingForBossSpawn = false;
        bossDefeatedInCurrentWave = false; // 重置BOSS击败状态
        timeCount = 0f;
        timeBossCount = 0f;
        Debug.Log($"强制进入下一波: Wave {currentWave}");

        // 立即开始新波次的敌人生成
        GetOrCreatEnemy(false);
    }

    /// <summary>
    /// 重置敌人刷新系统
    /// </summary>
    public void ResetEnemySpawningSystem()
    {
        // 重置波次系统
        currentWave = 1;
        waveProgress = 0f;
        isBossWave = false;
        waitingForBossSpawn = false;
        bossDefeatedInCurrentWave = false;
        enemiesPerWave = 8;
        waveProgressPerEnemy = 1f / enemiesPerWave;

        // 重置计时器
        timeCount = 0f;
        timeBossCount = 0f;

        Debug.Log($"敌人刷新系统已重置，当前波次: {currentWave}");
    }

    /// <summary>
    /// 初始化敌人管理器，重置所有状态
    /// </summary>
    public void InitializeEnemyManager()
    {
        ResetEnemySpawningSystem();

        ReturnAllActiveEnemiesToPool();
        ReinitializeEnemyPool();

        Debug.Log("敌人管理器已初始化，波次系统重置");
    }

    // ========== 工具方法 ==========

    /// <summary>
    /// 计算单次生成敌人数（基于EXP）
    /// </summary>
    private int CalculateSpawnCount()
    {
        int baseCount = 1;

        // 玩家等级超过5级时，根据EXP增加生成数量
        if (playerController.level > 5)
        {
            int extraEnemies = CalculateExtraEnemiesFromExp();
            baseCount += extraEnemies;
        }

        // 限制最大生成数量，避免一次性生成太多
        return Mathf.Min(baseCount, 3);
    }

    /// <summary>
    /// 根据EXP计算额外敌人数
    /// </summary>
    private int CalculateExtraEnemiesFromExp()
    {
        if (exp <= 0) return 0;

        int extraEnemies = Mathf.FloorToInt(exp / expThreshold);
        return Mathf.Min(extraEnemies, maxExtraEnemiesFromExp);
    }

    /// <summary>
    /// 根据EXP计算生成速度提升
    /// </summary>
    private float CalculateSpawnSpeedBoost()
    {
        if (playerController.level <= 5) return 0f;

        float boost = (exp / expThreshold) * expSpawnSpeedBoost;
        return Mathf.Min(boost, 2f);
    }

    /// <summary>
    /// 计算敌人生成间隔 - 基于EXP提升速度
    /// </summary>
    private float CalculateEnemySpawnInterval()
    {
        int playerLevel = playerController.level;

        float baseInterval = baseEnemySpawnInterval;

        // 玩家等级超过5级时，根据EXP减少生成间隔
        if (playerLevel > 5)
        {
            float speedBoost = CalculateSpawnSpeedBoost();
            baseInterval -= speedBoost;
        }

        // 波次减少（轻度影响）
        float waveReduction = (currentWave - 1) * 0.05f;
        baseInterval -= waveReduction;

        // 确保不会低于最小值
        return Mathf.Max(minEnemySpawnInterval, baseInterval);
    }

    // ========== 事件处理 ==========

    /// <summary>
    /// 敌人被击败时调用 - 强化BOSS击败处理
    /// </summary>
    public void OnEnemyDefeated(bool wasBoss = false)
    {
        Debug.Log(
            $"OnEnemyDefeated被调用 - 是否为BOSS: {wasBoss}, 当前波次: {currentWave}, WaitingForBoss: {waitingForBossSpawn}");

        if (wasBoss)
        {
            // 双重检查：只有在等待BOSS且不是BOSS波次时才处理
            if (waitingForBossSpawn && !isBossWave)
            {
                // 立即重置所有BOSS相关状态
                bossDefeatedInCurrentWave = true;
                waitingForBossSpawn = false;
                isBossWave = false;

                // 进入下一波
                currentWave++;
                waveProgress = 0f;

                // 重置所有计时器
                timeCount = 0f;
                timeBossCount = 0f;

                // 为下一波重置状态
                bossDefeatedInCurrentWave = false;

                Debug.Log($"🎉 BOSS被击败！开始Wave {currentWave}");

                // 动态调整难度
                if (currentWave % 3 == 0 && enemiesPerWave < 20)
                {
                    enemiesPerWave += 2;
                    Debug.Log($"每波敌人数增加到: {enemiesPerWave}");
                }

                waveProgressPerEnemy = 1f / enemiesPerWave;

                // 延迟开始下一波，确保状态完全重置
                StartCoroutine(StartNextWaveCoroutine());
            }
            else
            {
                Debug.LogWarning(
                    $"收到BOSS击败事件但状态异常 - waitingForBossSpawn: {waitingForBossSpawn}, isBossWave: {isBossWave}");
                // 强制恢复状态
                EmergencyRecovery();
            }
        }
        else
        {
            Debug.Log($"普通敌人被击败，波次进度: {waveProgress:F2}");
        }
    }

    /// <summary>
    /// 协程启动下一波
    /// </summary>
    private IEnumerator StartNextWaveCoroutine()
    {
        yield return new WaitForSeconds(1f); // 给1秒延迟确保状态稳定

        // 生成第一个敌人启动新波次
        if (GetOrCreatEnemy(false) != null)
        {
            Debug.Log($"新波次开始！Wave {currentWave} 的第一个敌人生成");
        }
        else
        {
            Debug.LogError("新波次敌人生成失败！");
            EmergencyRecovery();
        }
    }

    /// <summary>
    /// 增强的紧急恢复方法
    /// </summary>
    public void EmergencyRecovery()
    {
        Debug.Log("执行紧急恢复...");

        // 重置所有状态
        currentWave = Mathf.Max(1, currentWave);
        waveProgress = 0f;
        isBossWave = false;
        waitingForBossSpawn = false;
        bossDefeatedInCurrentWave = false;
        timeCount = 0f;
        timeBossCount = 0f;

        // 回收所有活跃敌人
        ReturnAllActiveEnemiesToPool();

        // 立即生成敌人重启系统
        StartCoroutine(StartNextWaveCoroutine());

        Debug.Log("紧急恢复完成");
    }

    // ========== 公开方法和调试方法 ==========

    // 获取当前波次信息（用于UI显示）
    public int GetCurrentWave() => currentWave;
    public float GetWaveProgress() => waveProgress;
    public bool IsBossWave() => isBossWave;

    // 公开方法用于外部配置敌人分组
    public void SetNormalEnemyIDs(List<int> ids) => normalEnemyIDs = ids;
    public void SetEliteEnemyIDs(List<int> ids) => eliteEnemyIDs = ids;
    public void SetBossEnemyIDs(List<int> ids) => bossEnemyIDs = ids;

    /// <summary>
    /// 判断敌人ID是否为BOSS
    /// </summary>
    public bool IsBossEnemy(int enemyID)
    {
        return bossEnemyIDs.Contains(enemyID);
    }

    /// <summary>
    /// 调试信息
    /// </summary>
    public void DebugEnemyWeights()
    {
        if (playerController == null) return;

        int level = playerController.level;

        Debug.Log($"当前波次: {currentWave}, 玩家等级: {level}, EXP: {exp}");

        int extraEnemies = CalculateExtraEnemiesFromExp();
        float speedBoost = CalculateSpawnSpeedBoost();
        float spawnInterval = CalculateEnemySpawnInterval();

        Debug.Log($"EXP额外敌人数: {extraEnemies}, 速度提升: {speedBoost:F2}, 生成间隔: {spawnInterval:F2}秒");

        if (currentWave > 1)
        {
            float normalWeight = Mathf.Max(1f, normalEnemyBaseWeight +
                                               (level * normalEnemyLevelFactor) +
                                               (currentWave * normalEnemyWaveFactor));

            float eliteWeight = Mathf.Max(1f, eliteEnemyBaseWeight +
                                              (level * eliteEnemyLevelFactor) +
                                              (currentWave * eliteEnemyWaveFactor));

            Debug.Log($"普通敌人权重: {normalWeight}, 精英敌人权重: {eliteWeight}");
        }
        else
        {
            Debug.Log("第一波: 只生成普通敌人");
        }
    }
}