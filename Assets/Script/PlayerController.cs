using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    private float moveHorizontal;
    private float moveVertical;

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
        rb = GetComponent<Rigidbody2D>();
        maxSpeed = 8f;
    }

    // Update is called once per frame
    void Update()
    {
        // 获取输入值
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        // 子弹计时器
        timeBullet += Time.deltaTime;
        if (timeBullet >= 5 / AttackSpeed)
        {
            PerformAttack();
            timeBullet = 0;
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
}