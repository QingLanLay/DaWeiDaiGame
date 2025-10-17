using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DavidDie : MonoBehaviour
{
    public int level;
    private bool canEat;
    private List<FoodName> inDavidDie;
    public List<Image> IconList;
    private GameObject prefab;
    private PlayerController player;
    
    // 经验条
    public int currentExp;
    // 滑动条
    public Slider timeSlider;

    // 消化时间
    private float time = 0;
    
    // 控制游戏开始
    public bool gameStarted = false;

    private void Awake()
    {
        level = 1;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        inDavidDie = new List<FoodName>();
        CheckIcon();
        Init();

        timeSlider.interactable = false;
    }

    private void Init()
    {
        foreach (var icon in IconList)
        {
            icon.color = Color.clear;
        }
    }


    private void Update()
    {
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
            }
        }
    }

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

    // 增加基础属性
    private void Add(float health, float attack, float attackSpeed, float speed)
    {
        player.Health += health;
        player.Attack += attack;
        player.AttackSpeed += attackSpeed;
        player.MaxSpeed += speed;
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

    public void UpLevel(int exp)
    {
        currentExp += exp;
        if (currentExp >= level*level*level*100)
        {
            if (level < 5)
            {
                level++;
            }
        }
    }
}