Shader "Sprites/SpriteBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.5
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _BlurAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                float blurSize = _BlurAmount * 0.035;
                
                fixed4 color = fixed4(0, 0, 0, 0);
                
                color += tex2D(_MainTex, uv + float2(-blurSize, -blurSize)) * 0.0625;
                color += tex2D(_MainTex, uv + float2(0, -blurSize)) * 0.125;
                color += tex2D(_MainTex, uv + float2(blurSize, -blurSize)) * 0.0625;
                
                color += tex2D(_MainTex, uv + float2(-blurSize, 0)) * 0.125;
                color += tex2D(_MainTex, uv) * 0.25;
                color += tex2D(_MainTex, uv + float2(blurSize, 0)) * 0.125;
                
                color += tex2D(_MainTex, uv + float2(-blurSize, blurSize)) * 0.0625;
                color += tex2D(_MainTex, uv + float2(0, blurSize)) * 0.125;
                color += tex2D(_MainTex, uv + float2(blurSize, blurSize)) * 0.0625;
                
                color *= IN.color;
                color.rgb *= color.a;
                
                return color;
            }
            ENDCG
        }
    }
}
