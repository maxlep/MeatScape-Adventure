/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net

  Based on project "webgl-noise" by Ashima Arts.
  Description : Array and textureless GLSL 2D simplex noise function.
      Author  : Ian McEwan, Ashima Arts.
  Maintainer  : ijm
      Lastmod : 20110822 (ijm)
      License : Copyright (C) 2011 Ashima Arts. All rights reserved.
                Distributed under the MIT License. See LICENSE file.
                https://github.com/ashima/webgl-noise
*/
/******************************************************************************/

#ifndef MUDBUN_CACHED_NOISE_3D
#define MUDBUN_CACHED_NOISE_3D

#include "NoiseCommon.cginc"
#include "ClassicNoise3D.cginc"

Texture3D<float> noiseCache;
SamplerState noiseCache_trilinear_repeat_sampler;
float3 noiseCacheDimension;
float noiseCacheDensity;

// range: [-0.5, 0.5]
float cached_noise(float3 p)
{
  float3 unitsPerPeriod = noiseCacheDimension / noiseCacheDensity;
  p = p / unitsPerPeriod;
  return noiseCache.SampleLevel(noiseCache_trilinear_repeat_sampler, p, 0.0f, 0);
}

// multiple octave
DEFINE_NOISE_FUNC_MULTIPLE_OCTAVES(cached_noise, float, float3, 0.5)

#endif
