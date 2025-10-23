using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    private float moveHorizontal;
    private float moveVertical;
    private bool attackInput;
    private Vector3 startV3;

    // 基本属性：生命值、移速、攻击力、攻击频率
    [SerializeField]
    private float health = 100f;

    [FormerlySerializedAs("maxSpeed")]
    [SerializeField]
    private float currentSpeed = 3f;

    [SerializeField]
    private float attack = 1f;

    [SerializeField]
    private float attackSpeed = 1f;

    private float maxSpeed;

    // 子弹计时器
    private float timeBullet;

    public DavidDie davidDie;

    [Range(0, 5)]
    public int level = 1;
    
    public GameObject gameOver;

#region 属性

    public float Health
    {
        get => health;
        set => health = value;
    }

    public float MaxSpeed
    {
        get => currentSpeed;
        set => currentSpeed = value;
    }

    public float Attack
    {
        get => attack;
        set => attack = value;
    }

    public float AttackSpeed
    {
        get => attackSpeed;
        set => attackSpeed = value;
    }

#endregion

    void Start()
    {
        startV3 = this.transform.position;
        rb = GetComponent<Rigidbody2D>();
        maxSpeed = 8f;
        level = 1;
    }

    // Update is called once per frame
    void Update()
    {
        level = davidDie.level;
        timeBullet += Time.deltaTime;

        // 获取输入值
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");
        attackInput = Input.GetKey(KeyCode.J);
        if (attackInput && timeBullet >= 5 / AttackSpeed)
        {
            {
                PerformAttack();
                timeBullet = 0;
            }
        } // 子弹计时器

        if (health <= 0)
        {
            Dead();
        }
        
    }


    void FixedUpdate()
    {
        // 移动
        Move();
    }


    private void Move()
    {
        rb.velocity = new Vector2(moveHorizontal * currentSpeed, rb.velocity.y);

        // 速度限制
        if (rb.velocity.x > maxSpeed)
        {
            rb.velocity = Vector2.right * maxSpeed;
        }
        else if (rb.velocity.x < -maxSpeed)
        {
            rb.velocity = Vector2.left * maxSpeed;
        }
    }

    private void PerformAttack()
    {
        BulletManager.Instance.GetBullet();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            float enemyAttack = enemy.Attack;
            health -= enemyAttack;
            Debug.Log("受到了伤害:" + enemyAttack);
            enemy.Dead();
        }

        if (other.CompareTag("Food"))
        {
            // 如果可以吃，则执行吃方法
            if (davidDie.CheckCanEat())
            {
                var food = other.GetComponent<Food>();
                var foodName = food.CurrentFoodData.FoodName;
                davidDie.Eat(foodName);
                food.returnToPool?.Invoke(other.gameObject);
            }
        }
    }


    private void Dead()
    {
        gameOver.GetComponent<GameOver>().OpenPanel();
    }
    
    public void InitializePlayer()
    {
        this.transform.position = startV3;
        
        // 重置基本属性
        health = 100f;
        currentSpeed = 3f;
        attack = 1f;
        attackSpeed = 1f;
        
        // 重置计时器和等级
        timeBullet = 0f;
        level = 1;
        
        // 重置物理状态
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // 确保玩家对象激活
        gameObject.SetActive(true);
        
        // 重置位置（如果需要，可以传入初始位置参数）
        // transform.position = initialPosition;
        
        Debug.Log("玩家已初始化");
    }
}