/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_SHADER_COMMON
#define MUDBUN_SHADER_COMMON

#include "../Math/Codec.cginc"
#include "../Math/MathConst.cginc"
#include "../Noise/RandomNoise.cginc"

#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_PS4) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_SWITCH)
  #define MUDBUN_VALID (1)
#endif

#ifdef MUDBUN_VALID
  #include "../Noise/ClassicNoise3D.cginc"
  #include "../Noise/SimplexNoise3D.cginc"
#endif

#ifdef SHADERPASS // HDRP & URP
  #if SHADERPASS == SHADERPASS_SHADOWS
    #define SHADERPASS_SHADOWCASTER
  #endif
#endif

#if defined(UNITY_PASS_SHADOWCASTER) && !defined(SHADERPASS_SHADOWCASTER) // standard
  #define SHADERPASS_SHADOWCASTER
#endif

int enable2dMode;

float4 _Color;
float4 _Emission;
float _Metallic;
float _Smoothness;

#ifdef MUDBUN_BUILT_IN_RP
float _AlphaCutoutThreshold;
float _Dithering;
int _RandomDither;

sampler2D _DitherTexture;
int _DitherTextureSize;

int _UseTex0;
sampler2D _MainTex;
float4 _MainTex_ST;
int _MainTexX;
int _MainTexY;
int _MainTexZ;

int _UseTex1;
sampler2D _Tex1;
float4 _Tex1_ST;
int _Tex1X;
int _Tex1Y;
int _Tex1Z;

int _UseTex2;
sampler2D _Tex2;
float4 _Tex2_ST;
int _Tex2X;
int _Tex2Y;
int _Tex2Z;

int _UseTex3;
sampler2D _Tex3;
float4 _Tex3_ST;
int _Tex3X;
int _Tex3Y;
int _Tex3Z;
#endif

float voxelSize;
float splatSize;
float splatSizeJitter;
float splatNormalShift;
float splatNormalShiftJitter;
float splatColorJitter;
float splatPositionJitter;
float splatRotationJitter;
float splatOrientationJitter;
float splatJitterNoisiness;
float splatCameraFacing;
float splatScreenSpaceFlattening;

float4x4 localToWorld;
float4x4 localToWorldIt;
float4 localToWorldScale;
float4x4 worldToLocal;

struct Vertex
{
  float4 vertex    : POSITION;
  float3 normal    : NORMAL;
  float3 tangent   : TANGENT;
  float4 color     : COLOR;
  float4 texcoord1 : TEXCOORD1;
  float4 texcoord2 : TEXCOORD2;
  uint id          : SV_VertexID;

  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

#ifndef SHADER_GRAPH
struct Input
{
  float2 tex                 : TEXCOORD0;
  float4 color               : COLOR;
  float4 emissionHash        : TEXCOORD3;
  float2 metallicSmoothness  : TEXCOORD4;
  float4 texWeight           : TEXCOORD5;
  float3 localPos            : TEXCOORD6;
  float3 localNorm           : TEXCOORD7;
  float4 screenPos;
};
#endif

void computeOpaqueTransparency
(
  float2 screenPos, 
  float3 pos, 
  float hash, 
  sampler2D ditherTexture, 
  int ditherTextureSize, 
  bool useRandomDither, 
  float alphaCutoutThreshold, 
  float ditheringBlend, 
  inout float alpha, 
  out float alphaThreshold
)
{
  alpha = saturate(1.02f * (alpha - 0.5f) + 0.5f);

  float ditherThreshold = 0.0f;
  if (useRandomDither > 0)
  {
    ditherThreshold = rand(pos);
  }
  else
  {
    ditherThreshold = tex2D(ditherTexture, screenPos / ditherTextureSize).r;
  }

  ditherThreshold = 0.98f * (ditherThreshold - 0.5f) + 0.5f;

  alphaThreshold = lerp(alphaCutoutThreshold, max(alphaCutoutThreshold, ditherThreshold), ditheringBlend);
}

float4 tex2D_triplanar
(
  sampler2D tex, 
  float4 texSt, 
  float3 weight, 
  float3 localPos, 
  bool projectX, 
  bool projectY, 
  bool projectZ
)
{
  float4 color = 0.0f;
  float totalWeight = 0.0f;
  if (projectX)
  {
    color += tex2D(tex, localPos.yz * texSt.xy + texSt.zw) * weight.x;
    totalWeight += weight.x;
  }
  if (projectY)
  {
    color += tex2D(tex, localPos.zx * texSt.xy + texSt.zw) * weight.y;
    totalWeight += weight.y;
  }
  if (projectZ)
  {
    color += tex2D(tex, localPos.xy * texSt.xy + texSt.zw) * weight.z;
    totalWeight += weight.z;
  }

  if (totalWeight <= 0.0f)
    return 1.0f;

  return color / totalWeight;
}

#endif

