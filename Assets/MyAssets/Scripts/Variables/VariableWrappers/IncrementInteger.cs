using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class IncrementInteger : MonoBehaviour
{
    [SerializeField] [Required] private IntVariable intVariable;

    public void Increment(int inc)
    {
        intVariable.Value += inc;
    }
}
