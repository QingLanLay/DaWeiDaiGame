using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;


public class FallingObjManager : MonoBehaviour
{
    // 对象池
    private Queue<GameObject> objectPool;
    // 食物预制体
    [SerializeField]
    private GameObject food;
    // 计时器
    private float timeCount;
    // 随机数
    private float randomXYZ;
    // 食物列表
    [SerializeField]
    private List<FoodData> foodList;
    
    private Dictionary<FoodName,FoodData> foodDictionary;
    
    private void Awake()
    {
        foodDictionary = new Dictionary<FoodName, FoodData>(); 

        // 列表转字典
        foreach (var data in foodList)
        {
            Debug.Log(data.FoodName);
            foodDictionary.Add(data.FoodName, data);
        }
        
        initializer();
    }

    private void Update()
    {
        
    #region 测试单元
        timeCount += Time.deltaTime;

        if (timeCount >= 1)
        {
            GetOrCreatFood(foodDictionary[FoodName.FangBianMian]);
            GetOrCreatFood(foodDictionary[FoodName.MenZi]);

            timeCount = 0;
        }

    #endregion
    }

    private void initializer()
    {
        // 初始化对象池
        if (objectPool == null)
        {
            objectPool = new Queue<GameObject>();
        }

        // 创建对象池内默认对象
        if (objectPool.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject newFood = GameObject.Instantiate<GameObject>(food, this.transform);

                // 获取food组件，设置回调
                var foodComponent = newFood.GetComponent<Food>();
                if (foodComponent != null)
                {
                    foodComponent.SetReturnToPool(ReturnToFoodPool);
                }
                newFood.SetActive(false);
                objectPool.Enqueue(newFood);
            }
        }
    }

    // 返回对象池
    private void ReturnToFoodPool(GameObject food)
    {
        if (food == null)
        {
            return;
        }
        food.SetActive(false);
        // 重置食物
        food.transform.position = this.transform.position;
        food.transform.SetParent(this.transform);
        objectPool.Enqueue(food);
    }

    // 执行食物掉落
    private GameObject GetOrCreatFood(FoodData foodData = null)
    {
        // 时刻生成随机数
        randomXYZ = Random.Range(-2.5f, 2.5f);
        
        // 对象池为空，获取新对象,新对象注入回调方法
        if (objectPool == null || objectPool.Count == 0)
        {
            GameObject newFood = GameObject.Instantiate(food, this.transform);
            Debug.Log("创建了新的food");
            
            // 随机位置生成
            newFood.transform.position = new Vector3(randomXYZ+this.transform.position.x,
                this.transform.position.y, this.transform.position.z);
            
            var foodComponent = newFood.GetComponent<Food>();
            if (foodComponent != null)
            {
                foodComponent.SetReturnToPool(ReturnToFoodPool);
                foodComponent.CurrentFoodData = foodData;
            }
            
            return newFood;
        }

        Debug.Log("当前对象池内对象数："+objectPool.Count);
        // 获取对象池对象
        GameObject aFood = objectPool.Dequeue();
        aFood.GetComponent<Food>().CurrentFoodData = foodData;
        // 随机位置
        aFood.transform.position = new Vector3(randomXYZ+this.transform.position.x,
            this.transform.position.y, this.transform.position.z);
        aFood.SetActive(true);
        return aFood;
    }
}