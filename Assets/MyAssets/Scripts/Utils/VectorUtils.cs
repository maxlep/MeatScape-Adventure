using System;
using UnityEngine;

namespace MyAssets.Scripts.Utils
{
    public static class VectorUtils
    {
        public static Vector2 xz(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }
        
        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation) {
            var dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }
    }

    [Serializable]
    public enum Vector3Axes
    {
        X, Y, Z
    }
}