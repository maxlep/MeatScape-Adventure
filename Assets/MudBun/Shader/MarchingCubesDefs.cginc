/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_MARCHING_CUBES_DEFS
#define MUDBUN_MARCHING_CUBES_DEFS

StructuredBuffer<int> triTable;
StructuredBuffer<int> vertTable;
StructuredBuffer<int> triTable2d;

static const float3 vertPosLs[8] = 
{
  float3(-0.5f, -0.5f, -0.5f), 
  float3(0.5f, -0.5f, -0.5f), 
  float3(0.5f, -0.5f,  0.5f), 
  float3(-0.5f, -0.5f,  0.5f), 
  float3(-0.5f,  0.5f, -0.5f), 
  float3(0.5f,  0.5f, -0.5f), 
  float3(0.5f,  0.5f,  0.5f), 
  float3(-0.5f,  0.5f,  0.5f), 
};

static const float3 vertPosLs2d[4] = 
{
  float3(-0.5f, -0.5f, 0.0f), 
  float3(0.5f, -0.5f, 0.0f), 
  float3(0.5f, 0.5f, 0.0f), 
  float3(-0.5f, 0.5f, 0.0f), 
};

#endif

