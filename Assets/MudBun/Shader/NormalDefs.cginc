/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_NORMAL_DEFS
#define MUDBUN_NORMAL_DEFS

#include "Math/Vector.cginc"

float normalDifferentiationStep;
float normalQuantization;

float normal2dFadeDist;
float normal2dStrength;

bool should_quantize_normal()
{
  return normalQuantization > kEpsilon;
}

float3 quantize_normal(float3 n)
{
  float step = lerp(0.01f, 1.0f, normalQuantization);
  return normalize_safe(quantize(n, step), 0.0f);
}

#endif

