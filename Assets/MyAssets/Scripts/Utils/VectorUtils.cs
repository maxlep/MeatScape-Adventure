using System;
using Cinemachine.Utility;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

namespace MyAssets.Scripts.Utils
{
    public static class VectorUtils
    {
        #region Swizzles
        
        public static Vector2 xz(this Vector3 vec) { return new Vector2(vec.x, vec.z); }
        public static Vector3 xoo(this Vector3 vec) { return new Vector3(vec.x, 0, 0); }
        public static Vector3 oyo(this Vector3 vec) { return new Vector3(0, vec.y, 0); }
        public static Vector3 ooz(this Vector3 vec) { return new Vector3(0, 0, vec.z); }
        public static Vector3 xoz(this Vector3 vec) { return new Vector3(vec.x, 0f, vec.z); }
    
        public static Vector2 xy(this Vector4 vec) { return new Vector2(vec.x, vec.y); }
        public static Vector2 xz(this Vector4 vec) { return new Vector2(vec.x, vec.z); }
        
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

        public static Vector3 Inverse(this Vector3 vec)
        {
            return new Vector3(1/vec.x, 1/vec.y, 1/vec.z);
        }

        public static Vector3 ProjectComponents(this Vector3 vec, Vector3 onNormal)
        {
            return Vector3.Project(vec.xoo(), onNormal).Abs()
                   + Vector3.Project(vec.oyo(), onNormal).Abs()
                   + Vector3.Project(vec.ooz(), onNormal).Abs();
        }
    }

    [Serializable]
    public enum Vector3Axes
    {
        X, Y, Z
    }
}