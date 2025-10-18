using System.Collections.Generic;
using UnityEngine;

public class BulletManager : SingletonMono<BulletManager>
{
    // 子弹预制体
    [SerializeField]
    private GameObject bullet;

    protected GameObject mouth;

    // 子弹对象池
    public Queue<GameObject> bulletPool;

    protected override void Awake()
    {
        Initializer();
         mouth = transform.Find("Mouth");
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
            var bullet = bulletPool.Dequeue();
            bullet.transform.position = this.transform.position+new Vector3(0,1,0);
            bullet.SetActive(true);
            return bullet;
        }
        // 对象池内无对象
        if (bulletPool != null)
        {
            bullet = Instantiate(bullet, transform);
            bullet.transform.position = this.transform.position;
            return bullet;
        }
        
        return null;
    }
}