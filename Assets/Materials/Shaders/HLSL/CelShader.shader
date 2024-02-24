Shader"BananaCowboyCustom/Legacy/CelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0.5, 0.65, 1, 1)
        [HDR]
        _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        [HDR]
        _SpecularColor("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
        _Glossiness("Glossisness", Float) = 32.0
        [HDR]
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimAmount("Rim Intensity", Range(0, 1)) = 0.716
        _RimThreshold("Rim Spread", Range(0, 1)) = 0.1
    }
    SubShader
    {

        Pass {

Name"Cel Shader"

            Tags
{
                "RenderType"="Opaque" 
                "LightMode"="ForwardBase"
                "PassFlags"="OnlyDirectional"
}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            // make shadows work
            #pragma multi_compile_fwdbase

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 worldNormal : NORMAL;
    float3 viewDir : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                SHADOW_COORDS(2)
};

            // shader properties 
sampler2D _MainTex;
float4 _MainTex_ST;

float4 _Color;
float4 _AmbientColor;

float4 _SpecularColor;
float _Glossiness;

float4 _RimColor;
float _RimAmount;
float _RimThreshold;

            // Vertex Shader
v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.worldNormal = UnityObjectToWorldNormal(v.normal);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.viewDir = WorldSpaceViewDir(v.vertex);
    UNITY_TRANSFER_FOG(o, o.pos);
    TRANSFER_SHADOW(o);
    return o;
}

            // Fragment shader
            // https://en.wikipedia.org/wiki/Blinn%E2%80%93Phong_reflection_model
            // uses Blinn phong as a shading model to split the lighting
            // into the light and shadowed sections based on the
            // MAIN directional light defined in _WorldSpaceLightPos0
fixed4 frag(v2f i) : SV_Target
{
                // sample the texture
    float4 sample = tex2D(_MainTex, i.uv);
                // View direction and Half Vector for specular lighting
    float3 viewDir = normalize(i.viewDir);
    float3 halfDir = normalize(_WorldSpaceLightPos0 + viewDir);
                // blinn-phong calculations
    float3 norm = normalize(i.worldNormal);
    float diffuse = dot(_WorldSpaceLightPos0, norm);
                // calculate shadow attenuation
    float shadow = SHADOW_ATTENUATION(i);
                // decides whether the pixel is in shadow or not
    float intensity = smoothstep(0, 0.01, diffuse * shadow);
                // includes the color of the main light source
    float4 light = (intensity) * _LightColor0;
                // blinn-phong specularity
    float specularStr = dot(norm, halfDir);
    float specularSmooth = smoothstep(0.005, 0.01, pow(specularStr * intensity, _Glossiness * _Glossiness));
    float4 specular = specularSmooth * _SpecularColor;
                // rim lighting. Lights up the part of the object facing away from the camera
    float4 rimDot = 1 - dot(viewDir, norm);
    float rimSmooth = rimDot * pow(diffuse, _RimThreshold);
    rimSmooth = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimSmooth * intensity);
    float4 rim = rimSmooth * _RimColor;
                // apply fog
    UNITY_APPLY_FOG(i.fogCoord, col);
                // return color
    return _Color * sample * (_AmbientColor + light + specular + rim);
}
            ENDCG
        }

        // very VERY simple shadow caster pass
        Pass {
Name"Shadow Cast"

            Tags
{
                "LightMode" = "ShadowCaster"
}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

            // Vertex Shader
v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);
    o.pos = UnityApplyLinearShadowBias(o.pos);
    o.uv = v.uv;
    return o;
}


            // Fragment shader that literally does absolutely nothing
fixed4 frag(v2f i) : SV_Target
{
    return 0;
}
            ENDCG
        }
    }
}