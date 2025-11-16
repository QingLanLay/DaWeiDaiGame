using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteGlowEffect : MonoBehaviour
{
    [Header("基础设置")]
    [SerializeField] public bool glowEnabled = true;
    
    [Header("发光效果")]
    [SerializeField] public Color glowColor = new Color(1f, 0.3f, 0.2f, 1f);
    [SerializeField] [Range(0, 1f)] public float glowSize = 0.02f;
    [SerializeField] [Range(0, 3f)] public float glowStrength = 1.5f;

    public SpriteRenderer spriteRenderer;
    public Material glowMaterial;
    public Material originalMaterial;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        
        CreateGlowMaterial();
        UpdateGlow();
    }

    void OnDisable()
    {
        // 恢复原始材质
        if (spriteRenderer != null)
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    void OnDestroy()
    {
        // 清理材质
        if (glowMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(glowMaterial);
            else
                DestroyImmediate(glowMaterial);
        }
    }

    private void CreateGlowMaterial()
    {
        // 尝试多个Shader名称
        Shader glowShader = Shader.Find("Sprites/Glow-SinglePass");
        if (glowShader == null)
            glowShader = Shader.Find("Sprites/Glow-Complete");
        
        if (glowShader != null)
        {
            glowMaterial = new Material(glowShader);
            glowMaterial.mainTexture = spriteRenderer.sprite.texture;
        }
        else
        {
            Debug.LogError("找不到发光Shader！请确保Shader文件已创建。");
            glowMaterial = new Material(originalMaterial);
        }
    }

    public void UpdateGlow()
    {
        if (spriteRenderer == null || glowMaterial == null) return;

        if (glowEnabled)
        {
            spriteRenderer.material = glowMaterial;
            glowMaterial.SetColor("_GlowColor", glowColor);
            glowMaterial.SetFloat("_GlowSize", glowSize);
            glowMaterial.SetFloat("_GlowStrength", glowStrength);
            glowMaterial.SetFloat("_GlowIntensity", glowStrength); // 兼容不同Shader
        }
        else
        {
            spriteRenderer.material = originalMaterial;
        }
    }

    public void SetGlowColor(Color color)
    {
        glowColor = color;
        UpdateGlow();
    }

    public void SetGlowSize(float size)
    {
        glowSize = Mathf.Clamp(size, 0, 0.1f);
        UpdateGlow();
    }

    public void SetGlowStrength(float strength)
    {
        glowStrength = Mathf.Clamp(strength, 0, 3f);
        UpdateGlow();
    }

    /// <summary>
    /// 临时增强发光效果
    /// </summary>
    public void PulseGlow(Color pulseColor, float duration = 0.5f)
    {
        Color originalColor = glowColor;
        float originalStrength = glowStrength;
        
        glowColor = pulseColor;
        glowStrength = originalStrength * 2f;
        UpdateGlow();
        
        Invoke(nameof(ResetGlow), duration);
    }

    private void ResetGlow()
    {
        // 恢复到默认设置或根据类型重置
        UpdateGlow();
    }

    /// <summary>
    /// 为敌人设置预设发光效果
    /// </summary>
    public void SetEnemyGlow(int enemyType)
    {
        switch (enemyType)
        {
            case 0: // 普通敌人
                glowColor = new Color(1f, 0.2f, 0.1f, 1f);
                glowSize = 0.015f;
                glowStrength = 1.2f;
                break;
            case 1: // 精英敌人
                glowColor = new Color(1f, 0.8f, 0.1f, 1f);
                glowSize = 0.025f;
                glowStrength = 1.8f;
                break;
            case 2: // BOSS
                glowColor = new Color(0.8f, 0.1f, 1f, 1f);
                glowSize = 0.04f;
                glowStrength = 2.5f;
                break;
        }
        UpdateGlow();
    }

    // 在Inspector中修改时立即更新
    void OnValidate()
    {
        if (Application.isPlaying && spriteRenderer != null)
        {
            CreateGlowMaterial();
            UpdateGlow();
        }
    }
}