using UnityEngine;

namespace MyAssets.Scripts.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// Get the distance from the center of a rectangle to a point on its edge in direction of dir.
        /// Can multiply this result by dir to get the vector to that point on edge from the center
        /// </summary>
        /// <param name="rectWidth"></param>
        /// <param name="rectHeight"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static float GetDistanceToRectangleEdgeFromCenter(float rectWidth, float rectHeight, Vector3 dir)
        {
            float dirAngle = Vector2.Angle(Vector2.up, dir) * Mathf.Deg2Rad;
            float d1 = Mathf.Abs((rectHeight / 2f) / Mathf.Cos(dirAngle));
            float d2 = Mathf.Abs((rectWidth / 2f) / Mathf.Sin(dirAngle));
            float distanceToEdge = Mathf.Min(d1, d2);

            return distanceToEdge;
        }

        public static float MapRange(this float value, float from1, float to1, float from2, float to2, bool clamp = false)
        {
            var remapped = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            return clamp ? Mathf.Clamp(remapped, from2, to2) : remapped;
        }
    }
}
