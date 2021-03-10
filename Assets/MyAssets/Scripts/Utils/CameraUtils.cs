using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRenderer;
using UnityEngine;

public static class CameraUtils 
{
    /// <summary>
    /// Check if given bounds around specified pos are within view of the camera
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="boundSize"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static bool IsVisible(Vector3 pos, Vector3 boundSize, Camera camera) {
        var bounds = new Bounds(pos, boundSize);
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
    
}
