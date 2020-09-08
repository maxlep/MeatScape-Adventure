﻿using System;
using AmplifyShaderEditor;
using UnityEngine;

namespace MyAssets.Scripts.Utils
{
    public static class VectorUtils
    {
        public static Vector2 xz(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        public static Vector2 RoundNearZero(this Vector2 vec)
        {
            var ret = new Vector2(vec.x, vec.y);
            ret.x = ret.x.RoundNearZero();
            ret.y = ret.y.RoundNearZero();
            return ret;
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