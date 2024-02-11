Shader "BananaCowboyCustom/Soda"
{
    // modified version of Roystan's toon water shader found here:
    // https://roystan.net/articles/toon-water/
    // my goat
    Properties
    {
        // depth gradient textures
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1

        // surface noise
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _SurfaceNoiseScroll("Surface Noise Scroll Intensity", Vector) = (0.03, 0.03, 0, 0)

        // Two channel distortion texture.
        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}	
        _SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27
        
        _FoamDistance("Foam Distance", Float) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;	
				float2 noiseUV : TEXCOORD0;
				float2 distortUV : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 viewNormal : NORMAL;
            };

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float _DepthMaxDistance;

            sampler2D _CameraDepthTexture;

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            float _SurfaceNoiseCutoff;
            float2 _SurfaceNoiseScroll;

            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;
            float _SurfaceDistortionAmount;

            float _FoamDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the depth texture
                float existingDepth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)).r;
                float existingDepthLinear = LinearEyeDepth(existingDepth);
                // calculate depth of surface below water
                float depthDifference = existingDepthLinear - i.screenPos.w;
                // lerp between gradient 
                float colorDepthDifference = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, colorDepthDifference);
                // get sample of distortion map
                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, (i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
                // sample surface noise texture
                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;
                // calculate depth cutoffs for foam
                float foamDepthDifference = saturate(depthDifference / _FoamDistance);
                // cutoff surface noise so it works
                float surfaceNoiseCutoff = foamDepthDifference * _SurfaceNoiseCutoff;
                // calculate if this pixel is affected by the noise sample
                float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;
                return waterColor + surfaceNoise;
            }
            ENDCG
        }
    }
}
