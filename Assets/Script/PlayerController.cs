using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Spine.Unity;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using AnimatorControllerParameter = UnityEngine.AnimatorControllerParameter;

public class PlayerController : MonoBehaviour
{
    #region 变量声明
    // 组件引用
    private Rigidbody2D rb;
    private Animator animator;
    
    // 输入控制
    private float moveHorizontal;
    private bool attackInput;
    private Vector3 startV3;
    
    // 玩家属性
    [SerializeField] private float health = 150f;
    [SerializeField] public float currentSpeed = 4f;
    [SerializeField] private float attack = 10f;
    [SerializeField] private float attackSpeed = 0.8f;
    
    public float realCurrentSpeed;
    private float maxSpeed;
    
    // 计时器
    private float timeBullet;
    
    // 引用
    public DavidDie davidDie;
    public GameObject gameOver;
    public Camera mainCamera;
    public GameObject body;
    
    [Range(0, 5)]
    public int level = 1;
    
    // 屏幕边界和玩家尺寸
    private float minX, minY, maxX, maxY;
    private float playerWidth, playerHeight;
    #endregion

    #region 属性封装
    public float Health
    {
        get => health;
        set => health = value;
    }

    public float MaxSpeed
    {
        get => maxSpeed;
        set => maxSpeed = value;
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

    #region Unity 生命周期方法
    void Start()
    {
        startV3 = this.transform.position;
        rb = GetComponent<Rigidbody2D>();
        maxSpeed = 10f;
        level = 1;
        animator = GetComponentInChildren<Animator>();

        // 计算屏幕边界
        CalculateScreenBounds();

        // 获取玩家尺寸
        CalculatePlayerSize();
    }

    void Update()
    {
        level = davidDie.level;
        timeBullet += Time.deltaTime;
        realCurrentSpeed = currentSpeed;

        // 获取输入值
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        attackInput = Input.GetKey(KeyCode.J);
        bool lowSpeedInput = Input.GetKey(KeyCode.K); 

        HandleAnimation();
        HandleAttack();
        HandleSpeedControl(lowSpeedInput);
        
        if (health <= 0)
        {
            Dead();
        }
    }

    void FixedUpdate()
    {
        // 先移动，再限制边界
        MoveAndClamp();
    }
    #endregion

    #region 初始化方法
    private void CalculatePlayerSize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            playerHeight = spriteRenderer.sprite.bounds.size.y / 2;
            playerWidth = spriteRenderer.sprite.bounds.size.x / 2;
        }
        else
        {
            playerHeight = 0.5f;
            playerWidth = 0.5f;
        }
    }

    private void CalculateScreenBounds()
    {
        if (mainCamera == null)
        {
            return;
        }

        // 将屏幕坐标转换为世界坐标
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minX = bottomLeft.x;
        maxX = topRight.x;
        minY = bottomLeft.y;
        maxY = topRight.y;
    }

    public void InitializePlayer()
    {
        this.transform.position = startV3;

        // 重置基本属性
        health = 150f;
        currentSpeed = 4f;
        maxSpeed = 10f;
        attack = 10f;
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

        Debug.Log("玩家已初始化");
    }
    #endregion

    #region 动画控制
    private void HandleAnimation()
    {
        Vector3 absScale = new Vector3(Mathf.Abs(body.transform.localScale.x), body.transform.localScale.y,
            body.transform.localScale.z);
        if (moveHorizontal >= 0.1f && Time.timeScale != 0)
        {
            body.transform.localScale = absScale;
            animator.SetBool("isWalk", true);
        }
        else if (moveHorizontal <= -0.1f && Time.timeScale != 0)
        {
            body.transform.localScale = new Vector3(-absScale.x, absScale.y, absScale.z);
            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isWalk", false);
        }
    }
    #endregion

    #region 攻击系统
    private void HandleAttack()
    {
        float attackInterval = 1f / attackSpeed;
        if (attackInput && timeBullet >= attackInterval)
        {
            // 设置该状态的播放速度，例如调整为0.8倍速
            animator.SetFloat("attackSpeed", attackSpeed*2f);
            //设置动画
            animator.SetBool("attack", true);

            PerformAttack();

            // 动态减少移速，攻速，攻击力
            currentSpeed -= 0.01f;
            attackSpeed -= 0.01f * level;
            attack -= 0.1f * level;

            if (attackSpeed < 0.8f)
            {
                attackSpeed = 0.8f;
            }

            if (attack < 1)
            {
                attack = 1;
            }

            timeBullet = 0;
        }
        else
        {
            animator.SetBool("attack", false);
        }
    }

    private void PerformAttack()
    {
        BulletManager.Instance.GetBullet();
    }
    #endregion

    #region 移动与速度控制
    private void MoveAndClamp()
    {
        if (mainCamera == null) return;

        // 计算移动
        Vector2 movement = new Vector2(moveHorizontal * realCurrentSpeed,rb.velocity.y);
        Vector3 newPosition = transform.position + (Vector3)movement * Time.fixedDeltaTime;

        // 应用边界限制（允许一半身体超出）
        newPosition.x = Mathf.Clamp(newPosition.x, minX - playerWidth * 0.5f, maxX + playerWidth * 0.5f);
        
        // 直接设置位置，避免物理引擎的干扰
        rb.MovePosition(newPosition);
    }

    private void HandleSpeedControl(bool lowSpeedInput)
    {
        if (lowSpeedInput)
        {
            realCurrentSpeed = currentSpeed / 2;
        }
        else
        {
            realCurrentSpeed = currentSpeed;
        }
        
        ControlMaxSpeed();
    }

    private void ControlMaxSpeed()
    {
        if (currentSpeed > maxSpeed)
        {
           currentSpeed = maxSpeed;
        }

        if (attackSpeed > maxSpeed)
        {
            attackSpeed = maxSpeed;
        }
    }
    #endregion

    #region 碰撞与交互
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            float enemyAttack = enemy.Attack;
            health -= enemyAttack;
            Debug.Log("受到了伤害:" + enemyAttack);
            animator.SetTrigger("beAttacked");
            enemy.Dead();
        }

        if (other.CompareTag("Food"))
        {
            // 如果可以吃，则执行吃方法
            if (davidDie.CheckCanEat())
            {
                AudioManager.Instance.PlayerRandomEffect();
                animator.SetTrigger("eat");
                var food = other.GetComponent<Food>();
                if (!food.isEat)
                {
                    var foodName = food.CurrentFoodData.FoodName;
                    food.isEat = true;
                    davidDie.Eat(foodName);
                    if (food.boxCollider2D != null)
                    {
                        food.boxCollider2D.enabled = false;
                    }

                    food.ApplyEffect(this);
                    StartCoroutine(Wait2Second(food));
                }
            }
        }
    }

    private IEnumerator Wait2Second(Food other)
    {
        yield return new WaitForSeconds(0.5f);
        if (other != null)
        {
            other.isEat = false;
            other.returnToPool?.Invoke(other.gameObject);
        }
    }
    #endregion

    #region 状态管理
    private void Dead()
    {
        gameOver.GetComponent<GameOver>().OpenPanel();
    }
    #endregion
}