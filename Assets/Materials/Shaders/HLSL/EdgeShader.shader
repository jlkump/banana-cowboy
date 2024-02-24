Shader"Hidden/BananaCowboyCustom/Legacy/EdgeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _CameraDepthTexture;
            float4 _MainTex_ST;
            float4 _CameraDepthTexture_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int x, y;
                fixed4 col = tex2D(_MainTex, i.uv);
                // use high precision function to get depth
                float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv));
                // if our depth is out of range, return immediately
                if(depth > 0.01) {
                    return col;
                }
                
                // sample all pixels around our current pixel
                float n = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(0, 1)).r);
                float e = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(1, 0)).r);
                float s = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(0, -1)).r);
                float w = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(-1, 0)).r);
                float ne = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(1, 1)).r);
                float nw = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(-1, 1)).r);
                float se = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(1, -1)).r);
                float sw = Linear01Depth(tex2D(_CameraDepthTexture, i.uv + _CameraDepthTexture_TexelSize * float2(-1, -1)).r);

                if (n - s > 0.0005 || w - e > 0.0005 || e - w > 0.0005 || s - n > 0.0005)
                    col = 0.0f;
                
                if (nw - se > 0.0005 || ne - sw > 0.0005 || se - nw > 0.0005 || sw - ne > 0.0005)
                    col = 0.0f;

                return col;
            }
            ENDCG
        }
    }
}
