#ifndef EXPLOSION_LOOP_INCLUDED
#define EXPLOSION_LOOP_INCLUDED

void ExplosionLoop_float(float2 uv, float seed, float particles, float res, float gravity, float time, out float Out)
{
    float clr = 0.0;
    float timecycle = frac(time);
    seed = (seed + floor(time));

    float invres = 1.0 / res;
    float invparticles = 1.0 / particles;

    float i = 0.0;
    for (i = 0.0; i < particles; i += 1.0)
    {
        seed += i + tan(seed);
        float2 tPos = (float2(cos(seed), sin(seed))) * i * invparticles;

        float2 pPos = float2(0.0, 0.0);
        pPos.x = ((tPos.x) * timecycle);
        pPos.y = -gravity * (timecycle * timecycle) + tPos.y * timecycle + pPos.y;

        pPos = floor(pPos * res) * invres; //-----------------------------------------comment this out for smooth version 

        float2 p1 = pPos;
        float4 r1 = float4(float2(step(p1.x, uv.x), step(p1.y, uv.y)), 1.0 - float2(step(p1.x + invres, uv.x), step(p1.y + invres, uv.y)));
        float px1 = r1.x * r1.y * r1.z * r1.w;
        float px2 = smoothstep(0.0, 200.0, (1.0 / distance(uv, pPos + .015)));//added glow
        px1 = max(px1, px2);

        clr += px1 * (sin(time * 20.0 + i) + 1.0);
    }
    Out = clr;
}

#endif // EXPLOSION_LOOP_INCLUDED