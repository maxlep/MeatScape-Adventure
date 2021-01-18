/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_BONE_FUNCS
#define MUDBUN_BONE_FUNCS

#include "BrushDefs.cginc"
#include "BrushFuncs.cginc"

float4 normalize_bone_weight(float4 boneWeight)
{
  return saturate(boneWeight / comp_sum(boneWeight) - 0.01f);
}

int blend_bone_weights(float brushRes, int brushBoneIndex, inout float4 boneRes, inout int4 boneIndex, inout float4 boneWeight)
{
  int iBone = -1;

  // this could probably be vectorized, but this is for off-line compute jobs so it's not that important
  if (brushRes < boneRes.x)
  {
    boneRes.xyzw = float4(brushRes, boneRes.xyz);
    boneIndex.xyzw = float4(brushBoneIndex, boneIndex.xyz);
    iBone = 0;
  }
  else if (brushRes < boneRes.y)
  {
    boneRes.yzw = float3(brushRes, boneRes.yz);
    boneIndex.yzw = float3(brushBoneIndex, boneIndex.yz);
    iBone = 1;
  }
  else if (brushRes < boneRes.z)
  {
    boneRes.zw = float2(brushRes, boneRes.z);
    boneIndex.zw = float2(brushBoneIndex, boneIndex.z);
    iBone = 2;
  }
  else if (brushRes < boneRes.w)
  {
    boneRes.w = brushRes;
    boneIndex.w = brushBoneIndex;
    iBone = 3;
  }
  else
  {
    return -1;
  }

  boneWeight = 1.0f / max(kEpsilon, boneRes);

  // TODO: variable tightness?
  //boneWeight = pow(boneWeight, 0.25f);

  boneWeight = normalize_bone_weight(boneWeight);

  return iBone;
}

#include "../Customization/CustomBone.cginc"

void sdf_apply_brush_bone_weights(float3 p, SdfBrush b, float brushRes, inout float4 boneRes, inout int4 boneIndex, inout float4 boneWeight)
{
  float3 pRel = quat_rot(quat_inv(b.rotation), p - b.position);

  switch (b.type)
  {
    case kSdfCurveSimple:
    {
      float2 curveRes = sdf_bezier(p, b.data0.xyz, b.data2.xyz, b.data1.xyz);
      float resA = sdf_sphere(p - b.data0.xyz, b.data0.w);
      float resB = sdf_sphere(p - b.data1.xyz, b.data1.w);
      float resC = sdf_sphere(p - bezier_quad(b.data0.xyz, b.data2.xyz, b.data1.xyz, 0.5f), b.data2.w);
      int brushBoneIndexA = b.boneIndex;
      int brushBoneIndexB = b.boneIndex + 1;
      int brushBoneIndexC = b.boneIndex + 2;

      blend_bone_weights(resA, brushBoneIndexA, boneRes, boneIndex, boneWeight);
      blend_bone_weights(resB, brushBoneIndexB, boneRes, boneIndex, boneWeight);
      blend_bone_weights(resC, brushBoneIndexC, boneRes, boneIndex, boneWeight);

      break;
    }

    case kSdfCurveFull:
    {
      int numPoints = int(b.data0.x) - 2;
      bool useNoise = false;//(b.data0.z > 0.0f);
      for (int i = 0; i < numPoints; ++i)
      {
        int iBrush = b.index + (useNoise ? 3 : 2) + i;
        float3 pointPos = aBrush[iBrush].data0.xyz;

        float maxSegDist = 0.0f;
        if (i > 0)
        {
          float3 prevPointPos = aBrush[iBrush - 1].data0.xyz;
          maxSegDist = max(maxSegDist, length(prevPointPos - pointPos));
        }
        if (i < numPoints - 1)
        {
          float3 nextPointPos = aBrush[iBrush - 1].data0.xyz;
          maxSegDist = max(maxSegDist, length(nextPointPos - pointPos));
        }

        float pDist = length(pRel - pointPos);
        if (maxSegDist > 0.0f && pDist > maxSegDist)
          continue;

        float pointRes = sdf_sphere(pRel - aBrush[iBrush].data0.xyz, aBrush[iBrush].data0.w);
        int pointBoneIndex = b.boneIndex + i;
        blend_bone_weights(pointRes, pointBoneIndex, boneRes, boneIndex, boneWeight);
      }
      break;
    }

    case kSdfBox:
    case kSdfSphere:
    case kSdfCylinder:
    case kSdfTorus:
    case kSdfSolidAngle:
    case kSdfParticle:
    case kSdfParticleSystem:
    case kSdfNoiseVolume:
    {
      blend_bone_weights(brushRes, b.boneIndex, boneRes, boneIndex, boneWeight);
      break;
    }

    default:
    {
      apply_custom_brush_bone_weights(p, pRel, b, brushRes, boneRes, boneIndex, boneWeight);
      break;
    }
  }
}

void compute_brush_bone_weights(float3 p, out int4 boneIndex, out float4 boneWeight)
{
  boneIndex = -1;
  boneWeight = 0.0f;

  float res = kInfinity;
  float4 boneRes = kInfinity;
  for (int iBrush = 0; iBrush < numBrushes; ++iBrush)
  {
    res = sdf_brush(res, p, aBrush[iBrush]);

    // not sure why Metal doesn't like this check...
    #if !defined(SHADER_API_METAL)
      if ((aBrush[iBrush].flags & kSdfBrushFlagsCountAsBone) == 0)
        continue;
    #endif

    sdf_apply_brush_bone_weights(p, aBrush[iBrush], res, boneRes, boneIndex, boneWeight);
  }
}

#endif

