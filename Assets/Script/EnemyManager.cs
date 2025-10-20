using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        // 根据规则获取敌人数据
        EnemyData enemyData = GetEnemyType(boss);
        float range = Random.Range(-2.5f, 2.5f);
        // 对象池还有对象
        if (enemyPool != null && enemyPool.Count > 0)
        {
            // 取出GameObject，激活，随机位置
            var enemy = enemyPool.Dequeue();
            enemy.GetComponent<Enemy>().GetEnemyData(enemyData);

            enemy.SetActive(true);
            enemy.transform.position =
                new Vector3(transform.position.x + range, transform.position.y, transform.position.z);
            enemy.transform.rotation = Quaternion.identity;

            // 数据注入

            return enemy;
        }

        // 对象池没有对象，新建对象
        var aEnemy = GameObject.Instantiate(defualtEnemy, transform);
        aEnemy.transform.position =
            new Vector3(transform.position.x + range, transform.position.y, transform.position.z);
        aEnemy.GetComponent<Enemy>().GetEnemyData(enemyData);
        return aEnemy;
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