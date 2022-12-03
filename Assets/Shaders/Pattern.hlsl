#ifndef PATTERN_INCLUDED
#define PATTERN_INCLUDED

#include "Noise.hlsl"

void Pattern_float(float3 worldPos, float scale, out float val)
{
    float noise1 = SimplexNoise(float3(worldPos.xz * scale, 0.1));
    val = noise1;


    //val =  abs( int(worldPos.x) * int(worldPos.z) ) % 3;   
}

void PatternTiles_float(float3 worldPos, float scale, out float val){
    val =  abs( int(worldPos.x) * int(worldPos.z) ) % 3;
}

#endif // PATTERN_INCLUDED
