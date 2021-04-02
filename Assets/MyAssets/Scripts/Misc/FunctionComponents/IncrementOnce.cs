using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using UnityEngine;

public class IncrementOnce : MonoBehaviour
{
    [SerializeField] private IntVariable _value;
    
    private bool _done = false;
    
    public void TryIncrement(int value)
    {
        if (_done) return;
        
        _done = true;
        _value.Value += value;
    }
}
