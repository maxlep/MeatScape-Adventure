using System.Collections.Generic;
using UnityEngine;

namespace MyAssets.Scripts.Utils
{
    public enum LineStyle
    {
        Solid = 0,
        Dashed = 1
    }
    
    public static class GreatGizmos
    {
        public static float DashSize = 0.1f;

        public static void DrawLine(Vector3 from, Vector3 to, LineStyle lineStyle = LineStyle.Solid)
        {
            var diff = to - from;
            var sqrDist = diff.magnitude;
            var numDashes = sqrDist / (2 * DashSize);
            var dir = diff.normalized;

            var start = from;
            for (int i = 0; i < numDashes; i++)
            {
                Gizmos.DrawLine(start, start + dir * DashSize);
                start += dir * DashSize * 2;
            }
        }
    }
}