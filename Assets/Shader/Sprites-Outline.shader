Shader "Sprites/Glow-SinglePass"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,0.3,0.2,1)
        _GlowSize ("Glow Size", Range(0, 0.1)) = 0.02
        _GlowStrength ("Glow Strength", Range(0, 3)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowSize;
            float _GlowStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样主纹理
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // 计算发光效果
                float glow = 0;
                float2 pixelSize = _MainTex_TexelSize.xy * _GlowSize * 50;
                
                // 只在透明区域计算发光
                if (col.a < 0.9)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            if (x == 0 && y == 0) continue;
                            
                            float2 offset = float2(x, y) * pixelSize;
                            fixed4 sampled = tex2D(_MainTex, i.uv + offset);
                            
                            if (sampled.a > 0.1)
                            {
                                float distance = length(float2(x, y));
                                float contribution = 1.0 - (distance / 3.0);
                                glow = max(glow, contribution * sampled.a);
                            }
                        }
                    }
                }
                
                // 混合精灵颜色和发光颜色
                if (glow > 0.01)
                {
                    fixed4 glowCol = _GlowColor;
                    glowCol.a *= glow * _GlowStrength;
                    
                    // 叠加发光效果到原始颜色
                    col.rgb = lerp(col.rgb, glowCol.rgb, glowCol.a);
                    col.a = max(col.a, glowCol.a * 0.5);
                }
                
                return col;
            }
            ENDCG
        }
    }
}