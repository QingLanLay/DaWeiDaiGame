using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DavidDie : SingletonMono<DavidDie>
{
    #region 变量声明
    public int level;
    public int currentExp;
    public bool gameStarted = false;
    
    // UI组件
    public List<Image> IconList;
    public Image icon;
    public Slider timeSlider;
    
    // 私有变量
    private bool canEat;
    private List<FoodName> inDavidDie;
    private GameObject prefab;
    private PlayerController player;
    private float time = 0;
    
    // 等级颜色配置
    private readonly Color[] rankColors = new Color[]
    {
        new Color(1.0f, 1.0f, 1.0f),    // 普通 - 白色
        new Color(0.0f, 1.0f, 0.0f),    // 精良 - 绿色
        new Color(0.0f, 0.0f, 1.0f),    // 稀有 - 蓝色
        new Color(0.5f, 0.0f, 0.5f),    // 史诗 - 紫色
        new Color(1.0f, 0.647f, 0.0f)   // 传说 - 橙色
    };
    #endregion

    #region Unity 生命周期方法
    protected override void Awake()
    {
        base.Awake();
        level = 1;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        inDavidDie = new List<FoodName>();
        CheckIcon();
        Init();

        timeSlider.interactable = false;
    }

    private void Update()
    {
        icon.color = rankColors[level - 1];
        
        // 检测是否还能继续吃
        if (CheckCanEat())
        {
            // 可以，则不进入消化倒计时
        }
        else
        {
            // 不可以，进入消化倒计时
            time += Time.deltaTime;
            if (time >= 5 / level)
            {
                time = 0;
                Digestion();
            }

            if (time == 0)
            {
                timeSlider.value = 1;
            }
            else
            {
                timeSlider.value = 1 - time/(5/level);
            }
        }

        CheckIcon();
    }
    #endregion

    #region 初始化方法
    private void Init()
    {
        foreach (var icon in IconList)
        {
            icon.color = Color.clear;
        }
    }

    /// <summary>
    /// 初始化大卫的胃系统，重置所有状态
    /// </summary>
    public void InitializeDavidDie()
    {
        // 重置等级和经验
        level = 1;
        currentExp = 0;
    
        // 清空胃袋中的食物
        if (inDavidDie != null)
        {
            inDavidDie.Clear();
        }
        else
        {
            inDavidDie = new List<FoodName>();
        }
    
        // 重置计时器
        time = 0f;
    
        // 重置UI图标
        ResetAllIcons();
    
        // 重置进度条
        if (timeSlider != null)
        {
            timeSlider.value = 1f;
        }
    
        // 重置游戏状态（根据需求决定是否重置）
        // gameStarted = false;
    
        Debug.Log("大卫的胃系统已初始化");
    }

    /// <summary>
    /// 重置所有食物图标显示
    /// </summary>
    private void ResetAllIcons()
    {
        if (IconList != null)
        {
            foreach (var icon in IconList)
            {
                if (icon != null)
                {
                    icon.sprite = null;
                    icon.color = Color.clear;
                }
            }
        }
    }
    #endregion

    #region UI与图标管理
    private void CheckIcon()
    {
        // 多余图标清零
        for (int i = 0; i < IconList.Count; i++)
        {
            if (IconList[i].sprite == null)
            {
                IconList[i].color = Color.clear;
            }
        }

        // 消化后清零
        if (inDavidDie.Count == 0)
        {
            for (int i = 0; i < IconList.Count; i++)
            {
                IconList[i].sprite = null;
                IconList[i].color = Color.clear;
            }
        }
    }
    #endregion

    #region 食物消化系统
    // 检测是否还能吃
    public void Eat(FoodName foodName)
    {
        if (canEat)
        {
            // 当前需要更新的图片
            inDavidDie.Add(foodName);
            int displayIndex = inDavidDie.Count - 1;
            // 更新图标
            if (displayIndex < IconList.Count)
            {
                // 查找对应图标
                FoodData fd = FallingObjManager.Instance.foodDictionary[foodName];
                if (fd != null)
                {
                    IconList[displayIndex].sprite = fd.Icon;
                    IconList[displayIndex].color = Color.white;
                }
            }
        }
    }

    // 从胃袋中获取相同食物的合计
    public void Digestion()
    {
        float addHealth = 0;
        float addAttack = 0;
        float addAttackSpeed = 0;
        float addSpeed = 0;
        if (inDavidDie != null && inDavidDie.Count == 4 + level * 2)
        {
            // 获取食物列表数组的合计
            var sameFoodCount = (from list in inDavidDie
                    group list by FallingObjManager.Instance.foodDictionary[list].ID
                    into g
                    select new
                    {
                        id = g.Key,
                        count = g.Count(),
                        food = g.First()
                    }
                ).ToList();

            // 食物累计增加的各项数值
            foreach (var foodList in sameFoodCount)
            {
                addHealth = FallingObjManager.Instance.foodDictionary[foodList.food].AddHeath * foodList.count *
                            ((int)FallingObjManager.Instance.foodDictionary[foodList.food].Type + 1);
                addAttack = FallingObjManager.Instance.foodDictionary[foodList.food].AddAttack * foodList.count *
                            ((int)FallingObjManager.Instance.foodDictionary[foodList.food].Type + 1);
                addAttackSpeed = FallingObjManager.Instance.foodDictionary[foodList.food].AddAttackSpeed *
                                 foodList.count *
                                 ((int)FallingObjManager.Instance.foodDictionary[foodList.food].Type + 1);
                addSpeed = FallingObjManager.Instance.foodDictionary[foodList.food].AddSpeed * foodList.count *
                           ((int)FallingObjManager.Instance.foodDictionary[foodList.food].Type + 1);
            }

            Add(addHealth, addAttack, addAttackSpeed, addSpeed);
            inDavidDie.Clear();
        }
    }

    // 检测是否还能继续吃
    public bool CheckCanEat()
    {
        if (inDavidDie.Count < 4 + 2 * level)
        {
            return canEat = true;
        }
        else
        {
            return canEat = false;
        }
    }
    #endregion

    #region 属性管理
    // 增加基础属性
    private void Add(float health, float attack, float attackSpeed, float speed)
    {
        player.Health += health;
        player.Attack += attack;
        player.AttackSpeed += attackSpeed;
        player.currentSpeed += speed;
    }
    #endregion

    #region 等级与经验系统
    public void UpLevel(int exp)
    {
        currentExp += exp;
        switch (currentExp)
        {
            case >= 100000 :
                level = 5;
                break;
            case >= 50000:
                level = 4;
                break;
            case >= 10000:
                level = 3;
                break;
            case >= 1000:
                level = 2;
                break;
            default:
                break;
        }
    }
    #endregion
}