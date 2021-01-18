/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_NORMAL_FUNCS
#define MUDBUN_NORMAL_FUNCS

#include "NormalDefs.cginc"
#include "VoxelDefs.cginc"

#include "../Customization/CustomBrush.cginc"

// http://iquilezles.org/www/articles/normalsSDF/normalsSDF.htm

// central differences
#define sdf_normal_diff(p, sdf_func, iBrushMask, h)                                                                    \
  normalize_safe                                                                                                       \
  (                                                                                                                    \
    float3                                                                                                             \
    (                                                                                                                  \
      sdf_func((p), float3(   (h), 0.0f, 0.0f), (iBrushMask)) - sdf_func((p), float3((-h), 0.0f, 0.0f), (iBrushMask)), \
      sdf_func((p), float3(0.0f,    (h), 0.0f), (iBrushMask)) - sdf_func((p), float3(0.0f, (-h), 0.0f), (iBrushMask)), \
      sdf_func((p), float3(0.0f, 0.0f,    (h)), (iBrushMask)) - sdf_func((p), float3(0.0f, 0.0f, (-h)), (iBrushMask))  \
    ),                                                                                                                 \
    float3(0.0f, 0.0f, 0.0f)                                                                                           \
  )

// tetrahedron technique
#define sdf_normal_tetra(p, sdf_func, iBrushMask, h)                                       \
  normalize_safe                                                                           \
  (                                                                                        \
      float3( 1.0f, -1.0f, -1.0f) * sdf_func((p), float3( (h), -(h), -(h)), (iBrushMask))  \
    + float3(-1.0f, -1.0f,  1.0f) * sdf_func((p), float3(-(h), -(h),  (h)), (iBrushMask))  \
    + float3(-1.0f,  1.0f, -1.0f) * sdf_func((p), float3(-(h),  (h), -(h)), (iBrushMask))  \
    + float3( 1.0f,  1.0f,  1.0f) * sdf_func((p), float3( (h),  (h),  (h)), (iBrushMask)), \
    float3(0.0f, 0.0f, 0.0f)                                                               \
  )

// use tetrahedron technique as default
#define sdf_normal(p, sdf_func, iBrushMask, h) sdf_normal_tetra(p, sdf_func, iBrushMask, h)

// macro that generates less inline code
#define SDF_NORMAL(normal, p, sdf_func, iBrushMask, h)                          \
  {                                                                             \
    float3 aSign[4] =                                                           \
    {                                                                           \
      float3( 1.0f, -1.0f, -1.0f),                                              \
      float3(-1.0f, -1.0f,  1.0f),                                              \
      float3(-1.0f,  1.0f, -1.0f),                                              \
      float3( 1.0f,  1.0f,  1.0f),                                              \
    };                                                                          \
    float3 aDelta[4] =                                                          \
    {                                                                           \
      float3( (h), -(h), -(h)),                                                 \
      float3(-(h), -(h),  (h)),                                                 \
      float3(-(h),  (h), -(h)),                                                 \
      float3( (h * 1.0001f), (h * 1.0002f), (h * 1.0003f)),                     \
    };                                                                          \
    float3 s = 0.0f;                                                            \
    SdfBrushMaterial nmat;                                                      \
    [loop] for (int iDelta = 0; iDelta < 4; ++iDelta)                           \
      s += aSign[iDelta] * sdf_func((p) + aDelta[iDelta], (iBrushMask), nmat);  \
    normal = normalize_safe(s, float3(0.0f, 0.0f, 0.0f));                       \
  }

// macro that generates less inline code
#define SDF_NORMAL_2D(normal, p, sdf_func, iBrushMask, h)                       \
  {                                                                             \
    float3 aSign[4] =                                                           \
    {                                                                           \
      float3( 1.0f, -1.0f, 0.0f),                                               \
      float3(-1.0f, -1.0f, 0.0f),                                               \
      float3(-1.0f,  1.0f, 0.0f),                                               \
      float3( 1.0f,  1.0f, 0.0f),                                               \
    };                                                                          \
    float3 aDelta[4] =                                                          \
    {                                                                           \
      float3( (h), -(h), 0.0f),                                                 \
      float3(-(h), -(h), 0.0f),                                                 \
      float3(-(h),  (h), 0.0f),                                                 \
      float3( (h * 1.0001f), (h * 1.0002f), 0.0f),                              \
    };                                                                          \
    float3 s = 0.0f;                                                            \
    SdfBrushMaterial nmat;                                                      \
    [loop] for (int iDelta = 0; iDelta < 4; ++iDelta)                           \
      s += aSign[iDelta] * sdf_func((p) + aDelta[iDelta], (iBrushMask), nmat);  \
    normal = normalize_safe(float3(s.xy, 0.0f), float3(0.0f, 0.0f, -1.0f));     \
  }

// macro that generates less inline code
#define SDF_NORMAL_FULL(normal, p, sdf_func, iBrushMask, h)                     \
  {                                                                             \
    float3 aDelta[6] =                                                          \
    {                                                                           \
      float3(-h, 0.0f, 0.0f),                                                   \
      float3( h, 0.0f, 0.0f),                                                   \
      float3(0.0f, -h, 0.0f),                                                   \
      float3(0.0f,  h, 0.0f),                                                   \
      float3(0.0f, 0.0f, -h),                                                   \
      float3(0.0f, 0.0f,  h),                                                   \
    };                                                                          \
    float aRes[6];                                                              \
    float3 s = 0.0f;                                                            \
    SdfBrushMaterial nmat;                                                      \
    [loop] for (int iDelta = 0; iDelta < 6; ++iDelta)                           \
      aRes[iDelta] = sdf_func((p) + aDelta[iDelta], (iBrushMask), nmat);        \
    normal =                                                                    \
      normalize_safe                                                            \
      (                                                                         \
        float3                                                                  \
        (                                                                       \
          aRes[1] - aRes[0],                                                    \
          aRes[3] - aRes[2],                                                    \
          aRes[5] - aRes[4]                                                     \
        ),                                                                      \
        float3(0.0f, 0.0f, 0.0f)                                                \
      );                                                                        \
  }

float3 normal_2d_blend(float3 norm2d, float sdfValue)
{
  float t = 
    normal2dFadeDist < kEpsilon 
      ? 0.0f 
      : saturate((normal2dFadeDist + sdfValue) / normal2dFadeDist);

  float3 n = lerp(float3(0.0f, 0.0f, -1.0f), norm2d, t);
  n = lerp(float3(0.0f, 0.0f, -1.0f), n, normal2dStrength * saturate(normal2dFadeDist / voxelSize));

  return n;
}

#endif

