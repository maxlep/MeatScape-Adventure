using System;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class ChargeClumpScaler : MonoBehaviour
{
    [SerializeField] private FloatReference ClumpThrowPercentToMaxCharge;
    [SerializeField] private FloatReference ClumpScaleFactor;
    [SerializeField] private float ClumpStartScale = 1f;
    [SerializeField] private TriggerVariable TriggerLockScale;

    private Vector3 originalScale;

    private bool IsScaleLocked = false;
    private float lockedScaleFactor;
    private float scaleFactor => Mathf.Lerp(ClumpStartScale, ClumpScaleFactor.Value, ClumpThrowPercentToMaxCharge.Value);

    private void Awake()
    {
        originalScale = transform.localScale;
        TriggerLockScale?.Subscribe(LockScale);
    }

    private void OnEnable()
    {
        IsScaleLocked = false;
    }

    private void OnDestroy()
    {
        TriggerLockScale?.Unsubscribe(LockScale);
    }

    void Update()
    {
        if (!IsScaleLocked)
        {
            lockedScaleFactor = scaleFactor;
        }
        transform.localScale = originalScale * lockedScaleFactor;
    }

    private void LockScale()
    {
        IsScaleLocked = true;
    }

    public void SetScaleToZero()
    {
        transform.localScale = Vector3.zero;
    }
}
