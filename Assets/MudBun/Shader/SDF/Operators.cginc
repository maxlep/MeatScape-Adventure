/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_SDF_OPERATORS
#define MUDBUN_SDF_OPERATORS

#include "../Math/MathConst.cginc"

// http://www.iquilezles.org/www/articles/smin/smin.htm
// http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm


// union
//-----------------------------------------------------------------------------

// raw union
float sdf_uni(float a, float b)
{
  return min(a, b);
}

// smooth quadratic polynomial union (C1 continuity, order-dependent concatenation)
float sdf_uni_quad(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / max(k, kEpsilon);
  return min(a, b) - h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial union (C2 continuity, order-dependent concatenation)
float sdf_uni_cubic(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / max(k, kEpsilon);
  return min(a, b) - h * h * h * k * (1.0f / 6.0f);
}

// smooth exponential union (infinite continuity, order-independent concatenation)
// max(k, kEpsilon); = 30.0f is a good default
#define sdf_uni_exp_concat_term(x, k) (exp2(-(k) * (x)))
#define sdf_uni_exp_concat_res(sum, k) (-log2(sum) / (k))
float sdf_uni_exp(float a, float b, float k) // 2-term concatenation
{
  float sum = sdf_uni_exp_concat_term(a, k) + sdf_uni_exp_concat_term(b, k);
  return sdf_uni_exp_concat_res(sum, k);
}

// smooth power union (infinite continuity, order-independent concatenation)
// max(k, kEpsilon); = 8.0f is a good default
#define sdf_uni_pow_concat_term(x, k) (pow((x), (k)))
#define sdf_uni_pow_concat_res(sum, prod, k) pow((prod) / (sum), 1.0f / (k))
float sdf_uni_pow(float a, float b, float k) // 2-term concatenation
{
  a = sdf_uni_pow_concat_term(a, k);
  b = sdf_uni_pow_concat_term(b, k);
  return sdf_uni_pow_concat_res(a + b, a * b, k);
}

// use cubic polynomial union as default
float sdf_uni_smooth(float a, float b, float k)
{
  return sdf_uni_cubic(a, b, k);
}

//-----------------------------------------------------------------------------
// end: union


// subtraction
//-----------------------------------------------------------------------------

// raw subtraction
float sdf_sub(float a, float b)
{
  return max(a, -b);
}

// smooth quadratic polynomial subtraction (C1 continuity, order-dependent concatenation)
float sdf_sub_quad(float a, float b, float k)
{
  float h = max(k - abs(a + b), 0.0f) / max(k, kEpsilon);
  return max(a, -b) + h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial subtraction (C2 continuity, order-dependent concatenation)
float sdf_sub_cubic(float a, float b, float k)
{
  float h = max(k - abs(a + b), 0.0f) / max(k, kEpsilon);
  return max(a, -b) + h * h * h * k * (1.0f / 6.0f);
}

// use cubic polynomial subtraction as default
float sdf_sub_smooth(float a, float b, float k)
{
  return sdf_sub_cubic(a, b, k);
}

//-----------------------------------------------------------------------------
// end: subtraction


// intersection
//-----------------------------------------------------------------------------

// raw intersection
float sdf_int(float a, float b)
{
  return max(a, b);
}

// smooth quadratic polynomial intersection (C1 continuity, order-dependent concatenation)
float sdf_int_quad(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / max(k, kEpsilon);
  return max(a, b) + h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial intersection (C2 continuity, order-dependent concatenation)
float sdf_int_cubic(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / max(k, kEpsilon);
  return max(a, b) + h * h * h * k * (1.0f / 6.0f);
}

// use cubic polynomial intersection as default
float sdf_int_smooth(float a, float b, float k)
{
  return sdf_int_cubic(a, b, k);
}

//-----------------------------------------------------------------------------
// end: intersection


#endif
