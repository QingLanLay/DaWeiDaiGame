using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DavidDie
{
    private int level;
    private bool canEat;
    private List<FoodData> inDavidDie = new List<FoodData>();
    private Sprite Icon1;
    private Sprite Icon2;
    private Sprite Icon3;
    private Sprite Icon4;
    private Sprite Icon5;

    public void Eat(FoodData foodData)
    {
        if (canEat)
        {
            inDavidDie.Add(foodData);
        }
    }

    public void Digestion()
    {
        if (inDavidDie != null && inDavidDie.Count == level * 5)
        {
            // 获取食物列表数组的合计
            var sameFoodCount = (from list in inDavidDie
                    group list by list.ID
                    into g
                    select new
                    {
                        id = g.Key,
                        count = g.Count(), 
                        foodData = g.First()
                    }
                ).ToString();
        }
    }

    public bool CheckCanEat()
    {
        if (inDavidDie.Count < level * 5)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}