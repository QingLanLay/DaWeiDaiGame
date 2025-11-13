using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    [Header("外边框设置")]
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] [Range(0, 10)] private int outlineSize = 1;
    [SerializeField] private bool showOutline = false;

    private SpriteRenderer spriteRenderer;
    private Material material;
    private Material originalMaterial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        
        // 创建外边框材质实例
        CreateOutlineMaterial();
    }

    void OnEnable()
    {
        UpdateOutline();
    }

    void OnDisable()
    {
        // 禁用时恢复原始材质
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    void OnDestroy()
    {
        // 清理材质
        if (material != null)
        {
            if (Application.isPlaying)
                Destroy(material);
            else
                DestroyImmediate(material);
        }
    }

    /// <summary>
    /// 创建外边框材质
    /// </summary>
    private void CreateOutlineMaterial()
    {
        // 使用内置的Sprite-Outline Shader
        Shader outlineShader = Shader.Find("Sprites/Outline");
        if (outlineShader != null)
        {
            material = new Material(outlineShader);
            material.CopyPropertiesFromMaterial(originalMaterial);
        }
        else
        {
            // 备用方案：使用自定义Shader
            Shader customShader = Shader.Find("Custom/OutlineShader");
            if (customShader != null)
            {
                material = new Material(customShader);
            }
            else
            {
                Debug.LogWarning("找不到外边框Shader，使用默认材质");
                material = new Material(originalMaterial);
            }
        }
    }

    /// <summary>
    /// 更新外边框显示
    /// </summary>
    public void UpdateOutline()
    {
        if (spriteRenderer == null || material == null) return;

        if (showOutline)
        {
            spriteRenderer.material = material;
            material.SetColor("_OutlineColor", outlineColor);
            material.SetFloat("_OutlineSize", outlineSize);
        }
        else
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    /// <summary>
    /// 显示外边框
    /// </summary>
    public void ShowOutline()
    {
        showOutline = true;
        UpdateOutline();
    }

    /// <summary>
    /// 隐藏外边框
    /// </summary>
    public void HideOutline()
    {
        showOutline = false;
        UpdateOutline();
    }

    /// <summary>
    /// 设置外边框颜色
    /// </summary>
    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        UpdateOutline();
    }

    /// <summary>
    /// 设置外边框大小
    /// </summary>
    public void SetOutlineSize(int size)    
    {
        outlineSize = Mathf.Clamp(size, 0, 10);
        UpdateOutline();
    }

    /// <summary>
    /// 临时高亮显示（用于受击等效果）
    /// </summary>
    public void HighlightTemporarily(float duration = 0.2f)
    {
        ShowOutline();
        Invoke(nameof(HideOutline), duration);
    }
}