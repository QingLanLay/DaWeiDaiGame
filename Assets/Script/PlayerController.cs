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
    private Rigidbody2D rb;

    private float moveHorizontal;
    private bool attackInput;
    private Vector3 startV3;

    // 基本属性：生命值、移速、攻击力、攻击频率
    [SerializeField]
    private float health = 150f;

    [FormerlySerializedAs("maxSpeed")]
    [SerializeField]
    private float currentSpeed = 4f;

    [SerializeField]
    private float attack = 10f;

    [SerializeField]
    private float attackSpeed = 0.8f;

    private float maxSpeed;

    // 子弹计时器
    private float timeBullet;

    public DavidDie davidDie;

    [Range(0, 5)]
    public int level = 1;

    public GameObject gameOver;
    public Camera mainCamera;

    [Header("动画")]
    private Animator animator;

    public GameObject body;

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
        maxSpeed = 10f;
        level = 1;
        animator = GetComponentInChildren<Animator>();

        // 计算屏幕边界
        CalculateScreenBounds();

        // 获取玩家尺寸
        CalculatePlayerSize();
    }

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

// Update is called once per frame
    void Update()
    {
        level = davidDie.level;
        timeBullet += Time.deltaTime;

        // 获取输入值
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        attackInput = Input.GetKey(KeyCode.J);

        Vector3 absScale = new Vector3(Mathf.Abs(body.transform.localScale.x), body.transform.localScale.y,
            body.transform.localScale.z);
        if (moveHorizontal >= 0.1f)
        {
            body.transform.localScale = absScale;
            animator.SetBool("isWalk", true);
        }
        else if (moveHorizontal <= -0.1f)
        {
            body.transform.localScale = new Vector3(-absScale.x, absScale.y, absScale.z);
            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isWalk", false);
        }

        float attackInterval = 1f / attackSpeed;
        if (attackInput && timeBullet >= attackInterval)
        {
            {
                // 设置该状态的播放速度，例如调整为0.8倍速
                animator.SetFloat("attackSpeed", attackSpeed*2f);
                //设置动画
                animator.SetBool("attack", true);

                PerformAttack();

                // 动态减少移速，攻速，攻击力
                currentSpeed -= 0.01f;
                attackSpeed -= 0.02f * level;
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
        }
        else
        {
            animator.SetBool("attack", false);
        }

        if (health <= 0)
        {
            Dead();
        }

        ControlMaxSpeed();
    }

    private void ControlMaxSpeed()
    {
        if (currentSpeed > maxSpeed)
        {
           currentSpeed = maxSpeed;
        }

        if (attackSpeed >= maxSpeed)
        {
            attackSpeed = maxSpeed;
        }
    }


    void FixedUpdate()
    {
        // 先移动，再限制边界
        MoveAndClamp();
    }

    private void MoveAndClamp()
    {
        if (mainCamera == null) return;

        // 计算移动
        Vector2 movement = new Vector2(moveHorizontal * currentSpeed,rb.velocity.y);
        Vector3 newPosition = transform.position + (Vector3)movement * Time.fixedDeltaTime;

        // 应用边界限制（允许一半身体超出）
        newPosition.x = Mathf.Clamp(newPosition.x, minX - playerWidth * 0.5f, maxX + playerWidth * 0.5f);
        
        // 直接设置位置，避免物理引擎的干扰
        rb.MovePosition(newPosition);
    }

    private float minX, minY, maxX, maxY;
    private float playerWidth, playerHeight;


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
            animator.SetTrigger("beAttacked");
            enemy.Dead();
        }

        if (other.CompareTag("Food"))
        {
            // 如果可以吃，则执行吃方法
            if (davidDie.CheckCanEat())
            {
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

    private void Dead()
    {
        gameOver.GetComponent<GameOver>().OpenPanel();
    }

    public void InitializePlayer()
    {
        this.transform.position = startV3;

        // 重置基本属性
        health = 150f;
        currentSpeed = 4f;
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

        // 重置位置（如果需要，可以传入初始位置参数）
        // transform.position = initialPosition;

        Debug.Log("玩家已初始化");
    }
}