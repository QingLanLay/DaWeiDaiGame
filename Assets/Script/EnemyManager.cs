using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
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

    // 计时器
    private float timeCount;
    private float timeBossCount;

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

        // 创建预制体
        if (enemyPool.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                var enemy = GameObject.Instantiate(defualtEnemy, transform);
                enemyPool.Enqueue(enemy);
                enemy.SetActive(false);
            }
        }
    }

    public float enemyUpdateTime = 5f;
    public float bossUpdateTime = 30f;

    void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount >= enemyUpdateTime)
        {
            GetOrCreatEnemy(false);
            timeCount = 0;
        }

        timeBossCount += Time.deltaTime;
        if (timeBossCount >= bossUpdateTime)
        {
            GetOrCreatEnemy(true);
            timeBossCount = 0;
        }
    }

    private GameObject GetOrCreatEnemy(bool boss)
    {
        // 安全检查
        if (playerController == null)
            return null;

        EnemyData enemyData = GetEnemyType(boss);
        if (enemyData == null)
        {
            UnityEngine.Debug.LogError("EnemyData is null!");
            return null;
        }

        float range = Random.Range(-2.5f, 2.5f);
        GameObject enemy = null;

        // 清理对象池中的null对象
        enemyPool = new Queue<GameObject>(enemyPool.Where(x => x != null));

        if (enemyPool.Count > 0)
        {
            enemy = enemyPool.Dequeue();
            // 确保对象有效
            if (enemy == null)
                return GetOrCreatEnemy(boss); // 递归重试
        }
        else
        {
            // 对象池没有对象，新建对象
            enemy = GameObject.Instantiate(defualtEnemy, transform);
        }

        // 配置敌人
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


    private EnemyData GetEnemyType(bool boss)
    {
        int level = playerController.level;
        int Id = 0;
        if (boss)
        {
            Id = level switch
            {
                1 => 0,
                2 => 1,
                >= 3 => 2,
                _ => 0,
            };
            return enemyDic[Id];
        }
        else if (level >= 1)
        {
            int random = Random.Range(0, 100);
            Id = random switch
            {
                > 30 => 0,
                <= 30 => 1
            };
            return enemyDic[Id];
        }

        return enemyDic[0];
    }
}