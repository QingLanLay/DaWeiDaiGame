using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("属性")]
    [SerializeField]
    private int id;

    [SerializeField]
    private string enemyName;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float attack;
    
    [SerializeField]
    private float health;

    [SerializeField]
    private int exp;

    [Header("外观")]
    [SerializeField]
    private Sprite icon;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float scale = 1f; // 新增：图片缩放比例

    public int ID
    {
        get => id;
        set => id = value;
    }

    public string EnemyName
    {
        get => enemyName;
        set => enemyName = value;
    }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public float Attack
    {
        get => attack;
        set => attack = value;
    }

    public float Health
    {
        get => health;
        set => health = value;
    }

    public Sprite Icon
    {
        get => icon;
        set => icon = value;
    }

    public int Exp
    {
        get => exp;
        set => exp = value;
    }

    public float Scale
    {
        get => scale;
        set => scale = value;
    }
}