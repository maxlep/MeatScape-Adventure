/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_COMPUTE_COMMON
#define MUDBUN_COMPUTE_COMMON

#define MUDBUN_IS_COMPUTE_SHADER (1)

#define kThreadGroupExtent (4)
#define kThreadGroupSize (kThreadGroupExtent * kThreadGroupExtent * kThreadGroupExtent)
#define kClearThreadGroupSize (256)

#endif

