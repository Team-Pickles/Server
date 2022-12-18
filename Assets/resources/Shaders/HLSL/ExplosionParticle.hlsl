#ifndef EXPLOSION_LOOP_INCLUDED
#define EXPLOSION_LOOP_INCLUDED

void ExplosionParticle_float(float2 uv, float time, out float Out)
{
    float clr = 0.0;

    float p = smoothstep(0.0, 20.0, 1.0 / length(uv));
    clr += p * (0.25*sin(time * 10.0) + 1.25);
    
    Out = clr;
}

#endif // EXPLOSION_LOOP_INCLUDED