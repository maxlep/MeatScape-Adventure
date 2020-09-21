using System;
using AmplifyShaderEditor;
using UnityEngine;

namespace MyAssets.Scripts.Utils
{
    public static class VectorUtils
    {
        #region Swizzles
        public static Vector2 xz(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        
        public static Vector2 xy(this Vector4 vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        public static Vector2 xz(this Vector4 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        #endregion

        public static Vector2 RoundNearZero(this Vector2 vec)
        {
            var ret = new Vector2(vec.x, vec.y);
            ret.x = ret.x.RoundNearZero();
            ret.y = ret.y.RoundNearZero();
            return ret;
        }
        
        public static Vector3 RoundNearZero(this Vector3 vec)
        {
            var ret = new Vector3(vec.x, vec.y, vec.z);
            ret.x = ret.x.RoundNearZero();
            ret.y = ret.y.RoundNearZero();
            ret.z = ret.z.RoundNearZero();
            return ret;
        }
        
        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation) {
            var dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }
        
        /// <summary>
        /// Flatten Vector by zeroing y component
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 Flatten(this Vector3 vec) {
            return new Vector3(vec.x, 0f, vec.z);
        }
    }

    [Serializable]
    public enum Vector3Axes
    {
        X, Y, Z
    }
}