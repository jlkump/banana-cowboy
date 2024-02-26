#ifndef LIGHTING_CEL_SHADED_INCLUDED
#define LIGHTING_CEL_SHADED_INCLUDED

#ifndef SHADERGRAPH_PREVIEW

// It was suggested I do this to keep things organized and (easier) to understand and I like it
struct ShadingVariables {
    float3 normal;
    float3 viewDir;
    float smoothness;
    float shininess;
    float rimThreshold;
};

float3 CalculateCelShading(Light l, ShadingVariables s) {
    // shadow attenuation
    float attenuation = l.shadowAttenuation * l.distanceAttenuation;
    // diffuse calculations
    float diffuse = saturate(dot(s.normal, l.direction));
    diffuse *= attenuation;
    // blinn-phong specularity calculations
    float3 halfDir = normalize(l.direction + s.viewDir);
    float specular = saturate(dot(s.normal, halfDir));
    specular = pow(specular, s.shininess);
    specular *= diffuse;
    // rim lighting
    float rim = 1 - dot(s.viewDir, s.normal);
    rim *= pow(diffuse, s.rimThreshold);
    // final color output
    return l.color * (diffuse + max(specular, rim));
}
#endif

void LightingCelShaded_float(float3 Normal, float3 View, float Smoothness, float RimThreshold, float3 Position, out float3 Col) {
    #if defined(SHADERGRAPH_PREVIEW)
        Col = half3(0.5f, 0.5f, 0.5f);
    #else
        // populate our shading variables
        ShadingVariables s;
        s.normal = normalize(Normal);
        s.viewDir = normalize(View);
        s.smoothness = Smoothness;
        s.shininess = exp2(10 * Smoothness + 1);
        s.rimThreshold = RimThreshold;
        // shadow stuff
        #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(Position);
            float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(Position);
        #endif
        // get light information and calculate color
        Light light = GetMainLight(shadowCoord);
        Col = CalculateCelShading(light, s);
        // get additional lighting information
        int additionalLightsCount = GetAdditionalLightsCount();
        for(int i = 0; i < additionalLightsCount; i++) {
            light = GetAdditionalLight(i, Position, 1);
            Col += CalculateCelShading(light, s);
        }
    #endif
}

#endif