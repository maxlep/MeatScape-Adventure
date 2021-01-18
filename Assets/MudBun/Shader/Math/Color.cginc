/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_COLOR
#define MUDBUN_COLOR

float3 hsv2rgb(float3 hsv)
{
  hsv.x = hsv.x - floor(hsv.x);
  int h = ((int) (hsv.x * 6));
  float f = hsv.x * 6.0 - h;
  float p = hsv.z * (1.0 - hsv.y);
  float q = hsv.z * (1.0 - f * hsv.y);
  float t = hsv.z * (1.0 - (1.0 - f) * hsv.y);

  switch (h)
  {
    default:
    case 0: return float3(hsv.z, t, p);
    case 1: return float3(q, hsv.z, p);
    case 2: return float3(p, hsv.z, t);
    case 3: return float3(p, q, hsv.z);
    case 4: return float3(t, p, hsv.z);
    case 5: return float3(hsv.z, p, q);
  }
}

#endif
