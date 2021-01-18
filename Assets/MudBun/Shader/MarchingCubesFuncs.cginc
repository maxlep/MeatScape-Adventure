/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_MARCHING_CUBES_FUNCS
#define MUDBUN_MARCHING_CUBES_FUNCS

#include "MarchingCubesDefs.cginc"

#include "BrushFuncs.cginc"
#include "NormalFuncs.cginc"
#include "SDF/SDF.cginc"

// cubeMat = whole-cube properties (for flat normal mode only)
// tStmtPre = statements pre-processing "iTri" for new triangle
// vStmt = statements processing "iVert", "aVertPos", "aVertNorm", and "aVertaMat" for new triangle
// tStmtPost = statements post-processing "iTri" a new triangle
#define MARCHING_CUBES(                                                        \
  center, size, sdf_func, iBrushMask, smoothNormal, cubeMat,                   \
  tStmtPre, vStmt, tStmtPost                                                   \
)                                                                              \
{                                                                              \
  int cubeIndex = 0;                                                           \
  float d[8];                                                                  \
  SdfBrushMaterial aMat[8];                                                    \
  cubeMat = init_brush_material();                                             \
  {                                                                            \
    [loop] for (int iVert = 0; iVert < 8; ++iVert)                             \
    {                                                                          \
      float3 vertPos = center + size * vertPosLs[iVert];                       \
      d[iVert] = sdf_func(vertPos, iBrushMask, aMat[iVert]);                   \
      cubeIndex |= (int(step(0.0f, -d[iVert])) << iVert);                      \
    }                                                                          \
                                                                               \
    if (!smoothNormal)                                                         \
      cubeMat = aMat[0];                                                       \
  }                                                                            \
                                                                               \
  int iTriListBase = cubeIndex * 16;                                           \
  [loop] for (int iTri = 0; iTri < 5; ++iTri)                                  \
  {                                                                            \
    int iTriBase = iTri * 3;                                                   \
    int aiEdge[3];                                                             \
    aiEdge[0] = triTable[iTriListBase + iTriBase];                             \
    if (aiEdge[0] < 0)                                                         \
      break;                                                                   \
                                                                               \
    aiEdge[1] = triTable[iTriListBase + iTriBase + 1];                         \
    aiEdge[2] = triTable[iTriListBase + iTriBase + 2];                         \
                                                                               \
    tStmtPre                                                                   \
                                                                               \
    float3 aVertPos[3];                                                        \
    float3 aVertNorm[3];                                                       \
    float3 aEdgeCenter[3];                                                     \
    float3 goodNorm = float3(0.0f, 0.0f, 0.0f);                                \
    SdfBrushMaterial aVertMat[3];                                              \
    [loop] for (int jVert = 0; jVert < 3; ++jVert)                             \
    {                                                                          \
      int iEdgeVert0 = vertTable[aiEdge[jVert] * 2];                           \
      int iEdgeVert1 = vertTable[aiEdge[jVert] * 2 + 1];                       \
      float3 p0Ls = vertPosLs[iEdgeVert0];                                     \
      float3 p1Ls = vertPosLs[iEdgeVert1];                                     \
      float t = -d[iEdgeVert0] / (d[iEdgeVert1] - d[iEdgeVert0]);              \
      aVertPos[jVert] = center + size * lerp(p0Ls, p1Ls, t);                   \
      aEdgeCenter[jVert] = center + size * 0.5f * (p0Ls + p1Ls);               \
      if (smoothNormal)                                                        \
      {                                                                        \
        aVertMat[jVert] = lerp(aMat[iEdgeVert0], aMat[iEdgeVert1], t);         \
        SDF_NORMAL(aVertNorm[jVert], aVertPos[jVert], sdf_func, iBrushMask, normalDifferentiationStep); \
        if (dot(aVertNorm[jVert], aVertNorm[jVert]) > kEpsilon)                \
          goodNorm = aVertNorm[jVert];                                         \
      }                                                                        \
    }                                                                          \
                                                                               \
    if (smoothNormal)                                                          \
    {                                                                          \
      for (int kVert = 0; kVert < 3; ++kVert)                                  \
      {                                                                        \
        if (dot(aVertNorm[kVert], aVertNorm[kVert]) <= kEpsilon)               \
          aVertNorm[kVert] = goodNorm;                                         \
      }                                                                        \
    }                                                                          \
    else                                                                       \
    {                                                                          \
      float3 flatNorm =                                                        \
        normalize                                                              \
        (                                                                      \
          cross(aVertPos[1] - aVertPos[0], aVertPos[2] - aVertPos[0])          \
        );                                                                     \
      for (int kVert = 0; kVert < 3; ++kVert)                                  \
        aVertNorm[kVert] = flatNorm;                                           \
    }                                                                          \
                                                                               \
    for (int iVert = 0; iVert < 3; ++iVert)                                    \
    {                                                                          \
      vStmt                                                                    \
    }                                                                          \
                                                                               \
    tStmtPost                                                                  \
  }                                                                            \
}


// cubeMat = whole-cube properties (for flat normal mode only)
// tStmtPre = statements pre-processing "iTri" for new triangle
// vStmt = statements processing "iVert", "aVertPos", "aVertNorm", and "aVertaMat" for new triangle
// tStmtPost = statements post-processing "iTri" a new triangle
#define MARCHING_CUBES_2D(                                                     \
  center, size, sdf_func, iBrushMask, smoothNormal, cubeMat,                   \
  tStmtPre, vStmt, tStmtPost                                                   \
)                                                                              \
{                                                                              \
  int squareIndex = 0;                                                         \
  float d[4];                                                                  \
  float dCenter;                                                               \
  SdfBrushMaterial aMat[4];                                                    \
  cubeMat = init_brush_material();                                             \
  {                                                                            \
    [loop] for (int iVert = 0; iVert < 4; ++iVert)                             \
    {                                                                          \
      float3 vertPos = center + size * vertPosLs2d[iVert];                     \
      d[iVert] = sdf_func(vertPos, iBrushMask, aMat[iVert]);                   \
      squareIndex |= (int(step(0.0f, -d[iVert])) << iVert);                    \
    }                                                                          \
  }                                                                            \
  if (!smoothNormal)                                                           \
    dCenter = sdf_func(center, iBrushMask, cubeMat);                           \
                                                                               \
  int iTriListBase = squareIndex * 12;                                         \
  [loop] for (int iTri = 0; iTri < 4; ++iTri)                                  \
  {                                                                            \
    int iTriBase = iTri * 3;                                                   \
    int aiVert[3];                                                             \
    aiVert[0] = triTable2d[iTriListBase + iTriBase];                           \
    if (aiVert[0] < 0)                                                         \
      break;                                                                   \
                                                                               \
    aiVert[1] = triTable2d[iTriListBase + iTriBase + 1];                       \
    aiVert[2] = triTable2d[iTriListBase + iTriBase + 2];                       \
                                                                               \
    tStmtPre                                                                   \
                                                                               \
    float3 aVertPos[3];                                                        \
    float3 aVertNorm[3];                                                       \
    float aVertSdfValue[3];                                                    \
    SdfBrushMaterial aVertMat[3];                                              \
    [loop] for (int jVert = 0; jVert < 3; ++jVert)                             \
    {                                                                          \
      if (aiVert[jVert] < 4)                                                   \
      {                                                                        \
        aVertPos[jVert] = center + size * vertPosLs2d[aiVert[jVert]];          \
        aVertMat[jVert] = aMat[aiVert[jVert]];                                 \
        aVertSdfValue[jVert] = d[aiVert[jVert]];                               \
      }                                                                        \
      else                                                                     \
      {                                                                        \
        int iVert0 = aiVert[jVert] - 4;                                        \
        int iVert1 = ((aiVert[jVert] == 7) ? 0 : (aiVert[jVert] - 3));         \
        float3 p0Ls = vertPosLs2d[iVert0];                                     \
        float3 p1Ls = vertPosLs2d[iVert1];                                     \
        float t = -d[iVert0] / (d[iVert1] - d[iVert0]);                        \
        aVertPos[jVert] = center + size * lerp(p0Ls, p1Ls, t);                 \
        aVertMat[jVert] = lerp(aMat[iVert0], aMat[iVert1], t);                 \
        aVertSdfValue[jVert] = lerp(d[iVert0], d[iVert1], t);                  \
      }                                                                        \
    }                                                                          \
                                                                               \
    float3 aVertNorm2d[3];                                                     \
    float3 norm;                                                               \
    float3 norm2d;                                                             \
    if (!smoothNormal)                                                         \
    {                                                                          \
      SDF_NORMAL_2D(norm2d, center, sdf_func, iBrushMask, normalDifferentiationStep); \
      norm = normal_2d_blend(norm2d, dCenter);                                 \
    }                                                                          \
    for (int kVert = 0; kVert < 3; ++kVert)                                    \
    {                                                                          \
      int iVertNorm = aiVert[kVert];                                           \
      if (smoothNormal)                                                        \
      {                                                                        \
        SDF_NORMAL_2D(norm2d, aVertPos[kVert], sdf_func, iBrushMask, normalDifferentiationStep); \
        norm = normal_2d_blend(norm2d, d[iVertNorm]);                          \
      }                                                                        \
      aVertNorm[kVert] = norm;                                                 \
      aVertNorm2d[kVert] = norm2d;                                             \
    }                                                                          \
                                                                               \
    for (int iVert = 0; iVert < 3; ++iVert)                                    \
    {                                                                          \
      vStmt                                                                    \
    }                                                                          \
                                                                               \
    tStmtPost                                                                  \
  }                                                                            \
}

#endif

