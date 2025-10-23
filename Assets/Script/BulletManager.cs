using System.Collections.Generic;
using UnityEngine;

public class BulletManager : SingletonMono<BulletManager>
{
    // 子弹预制体
    [SerializeField]
    private GameObject bullet;
    private GameObject mouth;

    // 子弹对象池
    public Queue<GameObject> bulletPool;


    protected override  void Awake()
    {
        base.Awake();
        
        Initializer(); 
        mouth = transform.parent.Find("Mouth")?.gameObject;
    }

    private void Initializer()
    {
        // 初始化对象池
        if (bulletPool == null)
        {
            bulletPool = new Queue<GameObject>();
        }
        // 初始空对象
        if (bulletPool.Count == 0)
        {
            for (int i = 0; i < 5; i++)
            {
                var newBullet = GameObject.Instantiate(bullet, transform);
                bulletPool.Enqueue(newBullet);
                newBullet.SetActive(false);
            }
        }
    }

    public GameObject GetBullet()
    {
        // 对象池内有对象
        if (bulletPool != null && bulletPool.Count > 0)
        {
            var bulletObj = bulletPool.Dequeue();
            bulletObj.transform.position = GetBulletSpawnPosition();
            bulletObj.SetActive(true);
            return bulletObj;
        }
    
        // 对象池内无对象，创建新子弹
        GameObject newBullet = Instantiate(bullet, transform);
        newBullet.transform.position = GetBulletSpawnPosition();
        newBullet.SetActive(true);
        return newBullet;
    }

    /// <summary>
    /// 获取子弹生成位置
    /// </summary>
    private Vector3 GetBulletSpawnPosition()
    {
        if (mouth != null)
        {
            return mouth.transform.position;
        }
        else
        {
            return transform.position + new Vector3(0, 1, 0);
        }
    }
    /// <summary>
    /// 初始化子弹管理器，重置所有状态
    /// </summary>
    public void InitializeBulletManager()
    {
        // 回收所有活跃的子弹
        ReturnAllActiveBulletsToPool();
    
        // 重新初始化对象池
        ReinitializeBulletPool();
    
        // 确保mouth引用正确
        EnsureMouthReference();
    
        Debug.Log("子弹管理器已初始化");
    }

    /// <summary>
    /// 回收所有活跃的子弹对象到对象池
    /// </summary>
    private void ReturnAllActiveBulletsToPool()
    {
        // 遍历所有子物体，回收激活的子弹
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                ReturnBulletToPool(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 将子弹回收到对象池
    /// </summary>
    /// <param name="bulletObj">子弹对象</param>
    private void ReturnBulletToPool(GameObject bulletObj)
    {
        if (bulletObj == null) return;
    
        // 重置子弹状态
        Bullet bulletComponent = bulletObj.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.ReturnToPool(); // 假设 Bullet 类有重置方法
        }
    
        bulletObj.SetActive(false);
        bulletObj.transform.SetParent(transform);
        bulletObj.transform.position = transform.position;
    
        // 如果对象池不为空且不包含该对象，则加入对象池
        if (bulletPool != null && !bulletPool.Contains(bulletObj))
        {
            bulletPool.Enqueue(bulletObj);
        }
    }

    /// <summary>
    /// 将子弹返回到对象池
    /// </summary>
    /// <param name="bulletObj">要回收的子弹</param>
   
    /// <summary>
    /// 重新初始化子弹对象池
    /// </summary>
    private void ReinitializeBulletPool()
    {
        // 清空现有对象池
        if (bulletPool != null)
        {
            bulletPool.Clear();
        }
        else
        {
            bulletPool = new Queue<GameObject>();
        }
    
        // 销毁所有现有子物体
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    
        // 重新创建初始对象池
        for (int i = 0; i < 5; i++)
        {
            GameObject newBullet = Instantiate(bullet, transform);
            newBullet.SetActive(false);
            bulletPool.Enqueue(newBullet);
        }
    }

    /// <summary>
    /// 确保mouth引用正确
    /// </summary>
    private void EnsureMouthReference()
    {
        if (mouth == null)
        {
            mouth = transform.parent?.Find("Mouth")?.gameObject;
        }
    }
}