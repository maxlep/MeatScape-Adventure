/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_AUTO_SMOOTH_DEFS
#define MUDBUN_AUTO_SMOOTH_DEFS

#define kNullAutoSmoothCacheId        (0)
#define kMaxAutoSmoothNormalPerVertex (12)

struct AutoSmoothVertData
{
  uint id;
  uint numNormals;
  float aNormal[kMaxAutoSmoothNormalPerVertex];
  float aWeight[kMaxAutoSmoothNormalPerVertex];
};

bool enableAutoSmooth;
float autoSmoothMaxAngle;
RWStructuredBuffer<AutoSmoothVertData> autoSmoothVertDataTable;
int autoSmoothVertDataPoolSize;
bool enableSmoothCorner;
int smoothCornerSubdivision;
float smoothCornerNormalBlur;
float smoothCornerFade;

#endif

