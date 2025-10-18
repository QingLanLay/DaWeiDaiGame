using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Food Data", menuName = "Game/Food Data")]
public class FoodData : ScriptableObject
{
    // id、名字、类型
    [Header("id、名字、类型")]
    [SerializeField]
    private int id;
    [SerializeField]
    private FoodType type;

    [SerializeField]
    private FoodName foodName;

    public FoodType Type
    {
        get => type;
        set => type = value;
    }

    // 属性
    [Header("属性")]
    [SerializeField]
    private float addSpeed;
    [SerializeField]
    private float addHeath;
    [SerializeField]
    private float addAttack;
    [SerializeField]
    private float addAttackSpeed;
    [SerializeField]
    private float damage;
    [SerializeField]
    private float gravityScale;
    
    [Header("图标")]
    [SerializeField]
    private Sprite icon;


    // 伤害
    public float Damage { get; set; }
    
    // 精灵
    public Sprite Icon
    {
        get => icon;
        set => icon = value;
    }


    public int ID
    {
        get => id;
        set => id = value;
    }



    public FoodName FoodName
    {
        get => foodName;
        set => foodName = value;
    }

    public float AddSpeed
    {
        get => addSpeed;
        set => addSpeed = value;
    }

    public float AddHeath
    {
        get => addHeath;
        set => addHeath = value;
    }


    public float AddAttack
    {
        get => addAttack;
        set => addAttack = value;
    }

    public float AddAttackSpeed
    {
        get => addAttackSpeed;
        set => addAttackSpeed = value;
    }

    public float GravityScale
    {
        get => gravityScale; 
        set => gravityScale = value; 
    }
}

public enum FoodName
{
    Default = 0,
    FangBianMian = 1,
    MenZi =2 
}

public enum FoodType
{
    smail = 0,
    normal = 1,
    big = 2,
    bad = -1
}