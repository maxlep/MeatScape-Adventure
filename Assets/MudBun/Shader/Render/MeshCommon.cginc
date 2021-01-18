/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_MESH_COMMON
#define MUDBUN_MESH_COMMON

#ifdef MUDBUN_VALID

#include "../BrushDefs.cginc"
#include "../GenPointDefs.cginc"
#include "../Math/Codec.cginc"
#include "../Math/Vector.cginc"
#include "../NormalDefs.cginc"
#include "../RenderModeDefs.cginc"

#endif

void mudbun_mesh_vert
(
  uint id, 
  out float4 vertexWs, 
  out float3 vertexLs, 
  out float3 normalWs, 
  out float3 normalLs, 
  out float4 color, 
  out float hash, 
  out float sdfValue, 
  out float3 normal2dLs, 
  out float3 normal2dWs
)
{
#if MUDBUN_VALID

  vertexLs = aGenPoint[id].posNorm.xyz;
  vertexWs = mul(localToWorld, float4(vertexLs, 1.0f));
  normalLs = unpack_normal(aGenPoint[id].posNorm.w);
  normal2dLs = unpack_normal(aGenPoint[id].norm2d);

  if (should_quantize_normal())
  {
    normalLs = quantize_normal(normalLs);
    normal2dLs = quantize_normal(normal2dLs);
  }

  normalWs = normalize(mul(localToWorldIt, float4(normalLs, 0.0f)).xyz);
  normal2dWs = normalize(mul(localToWorldIt, float4(normal2dLs, 0.0f)).xyz);

  color = unpack_rgba(aGenPoint[id].material.color);

  hash = aGenPoint[id].material.hash;

  sdfValue = aGenPoint[id].sdfValue;

#endif
}

void mudbun_mesh_vert
(
  uint id, 
  out float4 vertexWs, 
  out float3 vertexLs, 
  out float3 normalWs, 
  out float3 normalLs, 
  out float4 color, 
  out float4 emissionHash,
  out float2 metallicSmoothness, 
  out float4 intWeight, 
  out float sdfValue, 
  out float3 normal2dLs, 
  out float3 normal2dWs
)
{
#if MUDBUN_VALID

  mudbun_mesh_vert(id, vertexWs, vertexLs, normalWs, normalLs, color, emissionHash.a, sdfValue, normal2dLs, normal2dWs);

  emissionHash.rgb = unpack_rgba(aGenPoint[id].material.emissionTightness).rgb;
  metallicSmoothness = unpack_saturated(aGenPoint[id].material.metallicSmoothness);
  intWeight = unpack_rgba(aGenPoint[id].material.intWeight);

#else

  vertexWs = float4(0.0f, 0.0f, 0.0f, 1.0f);
  vertexLs = float3(0.0f, 0.0f, 0.0f);
  normalWs = float3(0.0f, 0.0f, 0.0f);
  normalLs = float3(0.0f, 0.0f, 0.0f);
  normalWs = float3(0.0f, 0.0f, 0.0f);
  color = float4(0.0f, 0.0f, 0.0f, 1.0f);
  emissionHash = float4(0.0f, 0.0f, 0.0f, 0.0f);
  metallicSmoothness = float2(0.0f, 0.0f);
  intWeight = int4(1.0f, 0.0f, 0.0f, 0.0f);
  sdfValue = 0.0f;
  normal2dLs = float3(0.0f, 0.0f, 0.0f);
  normal2dWs = float3(0.0f, 0.0f, 0.0f);

#endif
}

#endif
