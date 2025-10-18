using System;
using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        timeCount += Time.deltaTime;
        if (timeCount >= 5f)
        {
            GetOrCreatEnemy();
            timeCount = 0;
        }
    }

    private GameObject GetOrCreatEnemy()
    {
        // 根据规则获取敌人数据
        EnemyData enemyData = GetEnemyType();
        float range = Random.Range(-2.5f, 2.5f);
        // 对象池还有对象
        if (enemyPool != null && enemyPool.Count > 0)
        {
            // 取出GameObject，激活，随机位置
            var enemy = enemyPool.Dequeue();
            enemy.SetActive(true);
            enemy.transform.position =
                new Vector3(transform.position.x + range, transform.position.y, transform.position.z);

            // 数据注入
            enemy.GetComponent<Enemy>().GetEnemyData(enemyData);

            return enemy;
        }

        // 对象池没有对象，新建对象
        var aEnemy = GameObject.Instantiate(defualtEnemy, transform);
        aEnemy.transform.position =
            new Vector3(transform.position.x + range, transform.position.y, transform.position.z);
        aEnemy.GetComponent<Enemy>().GetEnemyData(enemyData);
        return aEnemy;
    }

    private EnemyData GetEnemyType()
    {
        if (playerController.level >=2)
        {
            return enemyDic[1];
        }
        return enemyDic[0];
    }
}