using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class ChargeClumpScaler : MonoBehaviour
{
    [SerializeField]
    private FloatReference ClumpThrowPercentToMaxCharge;

    [SerializeField]
    private FloatReference ClumpScaleFactor;
    void Update()
    {
        transform.localScale = ClumpThrowPercentToMaxCharge.Value * (Vector3.one * 2 * ClumpScaleFactor.Value);
    }

    public void SetScaleToZero()
    {
        transform.localScale = Vector3.zero;
    }
}
