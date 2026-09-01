
static const float PI = 3.14159265358979323846f;
uint randSeed;

uint createThreadRandomSeed(uint2 id, uint seed)
{
    return seed + id.x * 7919 + id.y * 35317;
}

uint wang_hash(uint seed)
{
    seed = (seed ^ 61) ^ (seed >> 16);
    seed *= 9;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2d;
    seed = seed ^ (seed >> 15);
    return seed;
}

/**
 * Takes our seed, updates it, and returns a pseudorandom float in [0..1]
 */
float nextRand(inout uint s, bool updateSeed = true)
{
    uint seed = wang_hash(s);
    // Xorshift algorithm from George Marsaglia's paper
    if (updateSeed)
    {
        s ^= (s << 13);
        s ^= (s >> 17);
        s ^= (s << 5);
        s = (1664525u * s + 1013904223u);
    }
    return float(seed & 0x00FFFFFF) / float(0x01000000);
}

float sNoise(float2 v, uint noiseMapSeed);
// return simplex noise sum between [0:1]
float FractalBrownianMotion(uint nrOfOctaves, float zoom, float xSkew, inout uint seed, float2 coord, int2 resolution)
{
    float value = 0.0f;
    float totalAmplitute = 0;

    float rotation = nextRand(seed) * PI;
    float sinc = sin(rotation);
    float cosc = cos(rotation);
    float2 newCoord;
    newCoord.x = (coord.x * cosc) - (coord.y * sinc);
    newCoord.y = (coord.x * sinc) + (coord.y * cosc);
    
    float amplitute = 1;
    for (uint i = 0; i < nrOfOctaves; ++i)
    {
        //if (i != nrOfOctaves - 1)
        //    continue;
        float currentZoom = exp2(i) * zoom;
        amplitute *= 0.5;
        totalAmplitute += amplitute;
        
        //float2 offset = float2(nextRand(seed) * resolution.x, nextRand(seed) * resolution.y);
        float2 offset = float2(
            nextRand(seed) * 289.0,
            nextRand(seed) * 289.0
        );
        float2 octaveCoord = offset + newCoord * currentZoom;
        octaveCoord.x *= xSkew;
        
        value += sNoise(octaveCoord, (uint) (nextRand(seed) * 289)) * amplitute;
    }
    return value / totalAmplitute;
}




float2 hash22(float2 p, uint seed)
{
    p += float2(seed, seed * 0.713f);

    float3 p3 = frac(float3(p.xyx) * 0.1031f);
    p3 += dot(p3, p3.yzx + 33.33f);

    return frac((p3.xx + p3.yz) * p3.zy);
}

float sNoise(float2 v, uint noiseMapSeed)
{
    const float2 K1 = float2(
        0.3660254038f, // (sqrt(3)-1)/2
        0.3660254038f
    );

    const float2 K2 = float2(
        0.2113248654f, // (3-sqrt(3))/6
        0.2113248654f
    );

    // Skew into simplex grid
    float2 i = floor(v + dot(v, K1));
    float2 x0 = v - i + dot(i, K2);

    // Determine simplex corner
    float2 i1 = (x0.x > x0.y)
        ? float2(1.0f, 0.0f)
        : float2(0.0f, 1.0f);

    float2 x1 = x0 - i1 + K2;
    float2 x2 = x0 - 1.0f + 2.0f * K2;

    // Random gradients
    float2 g0 = hash22(i, noiseMapSeed) * 2.0f - 1.0f;
    float2 g1 = hash22(i + i1, noiseMapSeed) * 2.0f - 1.0f;
    float2 g2 = hash22(i + 1.0f, noiseMapSeed) * 2.0f - 1.0f;

    g0 = normalize(g0);
    g1 = normalize(g1);
    g2 = normalize(g2);

    // Contribution from each corner
    float t0 = 0.5f - dot(x0, x0);
    float t1 = 0.5f - dot(x1, x1);
    float t2 = 0.5f - dot(x2, x2);

    float n0 = 0.0f;
    float n1 = 0.0f;
    float n2 = 0.0f;

    if (t0 > 0.0f)
    {
        t0 *= t0;
        n0 = t0 * t0 * dot(g0, x0);
    }

    if (t1 > 0.0f)
    {
        t1 *= t1;
        n1 = t1 * t1 * dot(g1, x1);
    }

    if (t2 > 0.0f)
    {
        t2 *= t2;
        n2 = t2 * t2 * dot(g2, x2);
    }

    // Scale approximately to [0,1]
    return saturate(0.5f + 35.0f * (n0 + n1 + n2));
}