using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// this class is intended to be put on the focus object itself
/// </summary>
[ExecuteInEditMode]
public class DOFFocusOnObject : MonoBehaviour
{
    [SerializeField] private TransformSceneReference camTransformRef;
    [SerializeField] private TransformSceneReference playerTransformRef;
    [SerializeField] private PostProcessProfile postProcessProfile;
    [SerializeField] private LayerMask dofMask;
    
    [SerializeField] private float minHitDistance = 40f;
    [SerializeField] private float maxHitDistance = 80f;
    [SerializeField] private float distanceSmoothTime = .1f;
    [SerializeField] private float minAperture = 13f;
    [SerializeField] private float maxAperture = 20f;
    [SerializeField] private float minFocalLength = 100f;
    [SerializeField] private float maxFocalLength = 220f;
    [SerializeField] private float focusDistance = 10f;

    [Range(0f, 1f)]
    [SerializeField] private float originBiasX = .5f;
    
    [Range(0f, 1f)]
    [SerializeField] private float originBiasY = .5f;

    private DepthOfField dof;
    private Camera cam;
    private float hitDistance;
    private float percentToMaxDist;
    private float aperture;
    private float focalLength;
    private float currentFocusDistance;

    private void Awake()
    {
        dof = postProcessProfile.GetSetting<DepthOfField>();
        cam = camTransformRef.Value.GetComponent<Camera>();
    }

    void Update()
    {
        Ray camRay = cam.ViewportPointToRay(new Vector3(originBiasX, originBiasY, 0f));
        RaycastHit hit;
        if (Physics.Raycast(camRay, out hit, Mathf.Infinity, dofMask))
            hitDistance = Mathf.Min(hit.distance, maxHitDistance);   
        else
            hitDistance = maxHitDistance;

        //Smooth to target hitDistance (Prevent sudden jumps in DoF)
        float vel = ((hitDistance - currentFocusDistance) / distanceSmoothTime) * Time.deltaTime;
        currentFocusDistance += vel;
        
        percentToMaxDist = Mathf.InverseLerp(minHitDistance, maxHitDistance, currentFocusDistance);
        
        //Lerp properties based on percent to max distance
        aperture = Mathf.Lerp(minAperture, maxAperture, percentToMaxDist);
        focalLength = Mathf.Lerp(minFocalLength, maxFocalLength, percentToMaxDist);

        dof.aperture.value = aperture;
        dof.focusDistance.value = focusDistance;
        dof.focalLength.value = focalLength;
        
    }
}