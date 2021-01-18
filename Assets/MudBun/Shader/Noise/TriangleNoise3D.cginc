/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_TRIANGLE_NOISE_3D
#define MUDBUN_TRIANGLE_NOISE_3D

#include "NoiseCommon.cginc"
#include "../Math/Vector.cginc"

// https://www.shadertoy.com/view/MlXSWX
float3 unit_tri_wave(in float3 x) { return abs(x - floor(x) - 0.5f); }
float triangle_noise(float3 p)
{
  float3 n = unit_tri_wave(p + unit_tri_wave(p * 0.41f + unit_tri_wave(p * 0.23f).yzx).zxy);
  return comp_sum(n) - 0.5f;
}

// multiple octave
DEFINE_NOISE_FUNC_MULTIPLE_OCTAVES(triangle_noise, float, float3, 0.5)

#endif
