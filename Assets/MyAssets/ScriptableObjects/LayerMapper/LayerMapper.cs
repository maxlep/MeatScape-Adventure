using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LayerMapper", menuName = "LayerMapper", order = 0)]
public class LayerMapper : SerializedScriptableObject
{
    [SerializeField] private Dictionary<LayerEnum, string> LayerDict = new Dictionary<LayerEnum, string>();

    public int GetLayer(LayerEnum layer)
    {
        if (LayerDict.ContainsKey(layer))
            return LayerMask.NameToLayer(LayerDict[layer]);

        else
        {
            Debug.LogError($"Layer {layer} is not a key in LayerDict!");
            return -1;
        }
    }
}

public enum LayerEnum
{
    Player,
    PlayerProjectile,
    Enemy,
    EnemyJumpTrigger,
    Bounce,
    InteractableTrigger
}
