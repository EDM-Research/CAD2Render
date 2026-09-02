

float3 unpackNormal(float4 packedNormal) {
    float3 normal;
    normal.xy = packedNormal.ag * 2 - 1;
    normal.z = sqrt(saturate(1 - (dot(normal.xy, normal.xy))));
    return normal;
}

float4 packNormalOpenGL(float3 normal)
{
    float4 packed;
    packed.rg = normal.xy * 0.5 + 0.5;
    packed.b = normal.z;
    packed.a = 1;
    return packed;
}

float4 packNormal(float3 normal)
{
    float4 packed;
    packed.r = 1;//magic number must be 1
    packed.g = normal.y * 0.5 + 0.5;
    packed.b = 0; //not used
    packed.a = normal.x * 0.5 + 0.5;
    return packed;
}
