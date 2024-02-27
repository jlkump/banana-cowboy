#ifndef LIGHTING_CEL_SHADED_INCLUDED
#define LIGHTING_CEL_SHADED_INCLUDED

#ifndef SHADERGRAPH_PREVIEW

// shading is based on Robin Seibold's implementation of cel shading since
// his explanation of handling multiple lights was digestible for my dumb ass
// which is ALSO based on Roystans toon shader linked here https://roystan.net/articles/toon-shader/

// for smoothstepping the edges of each component of the cel shading
// so it doesnt look as harsh
struct ThresholdConstants {
    float diffuse;
    float specular;
    float rim;
    float distanceAttenuation;
    float shadowAttenuation;
};

// It was suggested I do this to keep things organized and (easier) to understand and I like it
struct SurfaceValues {
    float3 normal;
    float3 viewDir;
    float smoothness;
    float shininess;
    float rimStrength;
    float rimAmount;
    float rimThreshold;
    ThresholdConstants tc;
};

float3 calculateCelShading(Light l, SurfaceValues s) {
    // shadow attenuation
    float attenuation = smoothstep(0.0f, s.tc.shadowAttenuation, l.shadowAttenuation) * 
        smoothstep(0.0f, s.tc.distanceAttenuation, l.distanceAttenuation);
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
    rim *= diffuse;
    // smoothstep everything here after all calcs are done to avoid weird lighting stuff
    diffuse = smoothstep(0.0f, s.tc.diffuse, diffuse);
    specular = s.smoothness * smoothstep(0.005f, 0.005f + s.tc.specular * s.smoothness, specular);
    rim = s.rimStrength * smoothstep( s.rimAmount - 0.5f * s.tc.rim, 
      s.rimAmount + 0.5f * s.tc.rim, rim );
    // final color output
    // return rim; // Isolating rim component
    // return s.albedo * l.color * (diffuse + s.ambient + max(specular, rim)); // cant figure out why this dumb line wont work
    return l.color * (diffuse + max(specular, rim));
}
#endif

void LightingCelShaded_float(float3 Normal, float3 View, float Smoothness, float RimStrength, float RimAmount, float RimThreshold,
      float3 Position, float EdgeDiffuse, float EdgeSpecular, float EdgeRim, float EdgeDistanceAttenuation, 
      float EdgeShadowAttenuation, float4 Ambient, float4 Albedo, out float3 Col) {
    #if defined(SHADERGRAPH_PREVIEW)
        Col = half3(0.5f, 0.5f, 0.5f);
    #else
        //Col = Albedo;
        // populate our surface shading variables
        SurfaceValues s;
        s.normal = normalize(Normal);
        s.viewDir = normalize(View);
        s.smoothness = Smoothness;
        s.shininess = exp2(10 * Smoothness + 1);
        s.rimStrength = RimStrength;
        s.rimAmount = RimAmount;
        s.rimThreshold = RimThreshold;
        // populate thresholding constants for shading
        ThresholdConstants tcon;
        tcon.diffuse = EdgeDiffuse;
        tcon.specular = EdgeSpecular;
        tcon.rim = EdgeRim;
        tcon.distanceAttenuation = EdgeDistanceAttenuation;
        tcon.shadowAttenuation = EdgeShadowAttenuation;
        s.tc = tcon;
        // shadow stuff
        #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(Position);
            float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(Position);
        #endif
        // get light information and calculate color
        Light light = GetMainLight(shadowCoord);
        Col = calculateCelShading(light, s);
        // get additional lighting information
        int additionalLightsCount = GetAdditionalLightsCount();
        for(int i = 0; i < additionalLightsCount; i++) {
            light = GetAdditionalLight(i, Position, 1);
            Col += calculateCelShading(light, s);
        }
        // do final lighting calculations
        Col += Ambient;
        Col *= Albedo;
    #endif
}


// structs for terrain shading
struct TerrainThresholdConstants {
    float diffuse;
    float distanceAttenuation;
    float shadowAttenuation;
};

struct TerrainSurfaceValues {
    float3 normal;
    float3 viewDir;
    TerrainThresholdConstants tc;
};

// calculate cel shading for terrain
float3 calculateTerrainShading(Light l, TerrainSurfaceValues ts) {
    // shadow attenuation
    float attenuation = smoothstep(0.0f, ts.tc.shadowAttenuation, l.shadowAttenuation) * 
        smoothstep(0.0f, ts.tc.distanceAttenuation, l.distanceAttenuation);
    // simple lambertian diffuse
    float diffuse = saturate(dot(ts.normal, l.direction));
    diffuse *= attenuation;
    // smoothstep our diffuse
    diffuse = smoothstep(0.0f, ts.tc.diffuse, diffuse);
    // final color calculation
    return l.color * diffuse;
}

// Main lighting function for terrain
void TerrainLightingCel_float(float3 Normal, float3 View, float3 Position, float EdgeDiffuse, 
    float EdgeDistanceAttenuation, float EdgeShadowAttenuation, float4 Ambient, float4 Albedo, out float3 Col) {
    
    #if defined(SHADERGRAPH_PREVIEW)
        Col = half3(0.5f, 0.5f, 0.5f);
    #else
        // populate Terrain Surface Values
        TerrainSurfaceValues ts;
        ts.normal = Normal;
        ts.viewDir = View;
        // populate Thresholding Constants
        TerrainThresholdConstants ttcon;
        ttcon.diffuse = EdgeDiffuse;
        ttcon.distanceAttenuation = EdgeDistanceAttenuation;
        ttcon.shadowAttenuation = EdgeShadowAttenuation;
        ts.tc = ttcon;
        // shadow stuff
        #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(Position);
            float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(Position);
        #endif
        // get light information and calculate color
        Light light = GetMainLight(shadowCoord);
        Col = calculateTerrainShading(light, ts);
        // get additional lighting information
        int additionalLightsCount = GetAdditionalLightsCount();
        for(int i = 0; i < additionalLightsCount; i++) {
            light = GetAdditionalLight(i, Position, 1);
            Col += calculateTerrainShading(light, ts);
        }
        // do final lighting calculations
        Col += Ambient;
        Col *= Albedo;
    #endif
}

#endif