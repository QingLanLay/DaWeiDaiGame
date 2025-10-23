    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Windows;
    using Random = UnityEngine.Random;


    public class FallingObjManager : SingletonMono<FallingObjManager>
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
        
        public Dictionary<FoodName,FoodData> foodDictionary;
        
        protected override void Awake()
        {
            base.Awake();
            objectPool = new Queue<GameObject>();
            foodDictionary = new Dictionary<FoodName, FoodData>(); 

            // 列表转字典
            foreach (var data in foodList)
            {
                foodDictionary.Add(data.FoodName, data);
            }
            
            init();
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

        private void init()
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
        private void ReturnToFoodPool(GameObject aFood)
        {
            if (aFood == null)
            {
                return;
            }
            aFood.SetActive(false);
            // 重置食物
            aFood.transform.position = this.transform.position;
            aFood.transform.SetParent(this.transform);
            objectPool.Enqueue(aFood);
        }

        // 执行食物掉落
        private GameObject GetOrCreatFood(FoodData foodData )
        {
            // 时刻生成随机数
            randomXYZ = Random.Range(-2.5f, 2.5f);
            
            // 对象池为空，获取新对象,新对象注入回调方法
            if (objectPool == null || objectPool.Count == 0)
            {
                GameObject newFood = GameObject.Instantiate(food, this.transform);
                
                // 随机位置生成
                newFood.transform.position = new Vector3(randomXYZ+this.transform.position.x,
                    this.transform.position.y, this.transform.position.z);
                // 图标刷新
                var fSpriteRenderer = newFood.GetComponent<SpriteRenderer>();
                fSpriteRenderer.sprite = foodData.Icon;

                var foodComponent = newFood.GetComponent<Food>();
                if (foodComponent != null)
                {
                    foodComponent.SetReturnToPool(ReturnToFoodPool);
                    foodComponent.CurrentFoodData = foodData;
                }
                
                return newFood;
            }

            // 获取对象池对象
            GameObject aFood = objectPool.Dequeue();
            aFood.GetComponent<Food>().CurrentFoodData = foodData;
            // 图标刷新
            var spriteRenderer = aFood.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = foodData.Icon;
            // 随机位置
            aFood.transform.position = new Vector3(randomXYZ+this.transform.position.x,
                this.transform.position.y, this.transform.position.z);
            aFood.SetActive(true);
            return aFood;
        }
        
        /// <summary>
        /// 初始化掉落物管理器，重置所有状态
        /// </summary>
        public void InitializeFallingObjManager()
        {
            // 重置计时器
            timeCount = 0f;
    
            // 回收所有已激活的食物对象
            ReturnAllActiveFoodsToPool();
    
            // 重新初始化对象池
            ReinitializePool();
    
            Debug.Log("掉落物管理器已初始化");
        }

        /// <summary>
        /// 回收所有活跃的食物对象到对象池
        /// </summary>
        private void ReturnAllActiveFoodsToPool()
        {
            // 遍历所有子物体，回收激活的食物
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Food foodComponent = child.GetComponent<Food>();
                    if (foodComponent != null)
                    {
                        // 直接调用回收方法
                        ReturnToFoodPool(child.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// 重新初始化对象池
        /// </summary>
        private void ReinitializePool()
        {
            // 清空现有对象池
            if (objectPool != null)
            {
                objectPool.Clear();
            }
            else
            {
                objectPool = new Queue<GameObject>();
            }
    
            // 销毁所有现有子物体
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
    
            // 重新创建初始对象池
            for (int i = 0; i < 5; i++)
            {
                GameObject newFood = Instantiate(food, this.transform);
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