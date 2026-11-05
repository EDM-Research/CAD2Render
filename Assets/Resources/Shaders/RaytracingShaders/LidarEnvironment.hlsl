#ifndef LIDAR_ENVIRONMENT_HLSL
#define LIDAR_ENVIRONMENT_HLSL

// Colour of the scene around the LiDAR, rendered by 6 90 degree cameras (one per cube face, any shared rotation).
// Slice i of g_FaceTex is the image of face camera i; g_FaceViewProj[i] = projection * worldToCamera of that camera.
Texture2DArray<float4> g_FaceTex;
SamplerState sampler_Linear_Clamp;
float4x4 g_FaceViewProj[6];

// dir: world-space direction from the LiDAR position (= the face cameras' position)
float4 SampleEnvironment(float3 dir)
{
    for (int i = 0; i < 6; ++i)
    {
        // w = 0: only rotate/project the direction, the camera position is the ray origin
        float4 clip = mul(g_FaceViewProj[i], float4(dir, 0));
        if (clip.w <= 0)
            continue;
        float2 ndc = clip.xy / clip.w;
        if (abs(ndc.x) <= 1.0 && abs(ndc.y) <= 1.0)
            return g_FaceTex.SampleLevel(sampler_Linear_Clamp, float3(ndc * 0.5 + 0.5, i), 0);
    }
    return float4(0, 0, 0, 1);
}

// Linear -> sRGB encoding, for colours that are stored as raw bytes (the exported point colours)
float3 LinearToSrgb(float3 c)
{
    c = saturate(c);
    float3 low = c * 12.92;
    float3 high = 1.055 * pow(c, 1.0 / 2.4) - 0.055;
    return lerp(high, low, step(c, 0.0031308));
}

#endif
