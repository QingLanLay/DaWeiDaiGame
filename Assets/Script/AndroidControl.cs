

using UnityEngine;

public class VirtualInputManager : SingletonMono<VirtualInputManager>
{
    // 静态实例，方便任何脚本访问
    public static VirtualInputManager Instance;

    // 公开的布尔变量，代表按键状态
    public bool isLeftPressed = false;
    public bool isRightPressed = false;
    public bool isAttackPressed = false;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 提供给按钮调用的方法
    public void SetLeft(bool pressed) { isLeftPressed = pressed; }
    public void SetRight(bool pressed) { isRightPressed = pressed; }
    public void SetAttack(bool pressed) { isAttackPressed = pressed; }
    
    public float GetVirtualAxisHorizontal()
    {
        float axisValue = 0f;
        if (isLeftPressed) axisValue -= 1f;  // 左键按下，值减1
        if (isRightPressed) axisValue += 1f; // 右键按下，值加1
        // 如果同时按下，结果为0；都不按，结果也为0
        return axisValue;
    }
}