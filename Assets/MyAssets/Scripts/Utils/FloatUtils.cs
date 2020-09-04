using UnityEngine;
using UnityEngine.ProBuilder;

namespace MyAssets.Scripts.Utils
{
    public static class FloatUtils
    {
        public static float RoundNearZero(this float val)
        {
            return Mathf.Abs(val) < 0.001f ? 0 : val;
        }
    }
}