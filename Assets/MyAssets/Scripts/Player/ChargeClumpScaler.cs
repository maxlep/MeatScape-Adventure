using System;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class ChargeClumpScaler : MonoBehaviour
{
    [SerializeField] private FloatReference ClumpThrowPercentToMaxCharge;
    [SerializeField] private FloatReference ClumpScaleFactor;
    [SerializeField] private float ClumpStartScale = 1f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = originalScale * Mathf.Lerp(ClumpStartScale, ClumpScaleFactor.Value, ClumpThrowPercentToMaxCharge.Value);
    }

    public void SetScaleToZero()
    {
        transform.localScale = Vector3.zero;
    }
}
