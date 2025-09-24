using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float health;

    [SerializeField]
    private float attack;

    [SerializeField]
    private float speed;

    private Sprite icon;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public float Attack
    {
        set => attack = value;
        get => attack;
    }
       

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = icon;
    }

    private void FixedUpdate()
    {
        rb.velocity = Vector2.down * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            health -= other.GetComponent<Bullet>().Attack;
        }

        if (health <= 0)
        {
            Dead();
        }
    }

    private void OnEnable()
    {
    }

    public void Dead()
    {
        EnemyManager.Instance.enemyPool.Enqueue(this.gameObject);
        gameObject.transform.position = Vector3.zero;
        icon = null;
        spriteRenderer.sprite = null;
        this.gameObject.SetActive(false);
    }
    

    public void GetEnemyData(EnemyData enemyData)
    {
        health = enemyData.Health;
        speed = enemyData.Speed;
        attack = enemyData.Attack;
        icon = enemyData.Icon;
        spriteRenderer.sprite = icon;
        spriteRenderer.size = new Vector2(1, 1);

    }
}