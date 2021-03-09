using Sirenix.OdinInspector;
using UnityEngine;
using MapMagic.Core;

public class MapMagicSeedManager : MonoBehaviour
{
    [SerializeField] MapMagicObject MapMagic;
    [DisableIf("UseSetSeed")] [SerializeField] bool GenerateRandomSeed;
    [DisableIf("GenerateRandomSeed")] [SerializeField] bool UseSetSeed;
    [ShowIf("UseSetSeed")] [SerializeField] string Seed;

    void Awake()
    {
        if(GenerateRandomSeed || MapMagic.graph.random.Seed == 0) {
            MapMagic.graph.random.Seed = Random.Range(10000, 9999999);
            MapMagic.Refresh();
        }
        if(UseSetSeed) {
            MapMagic.graph.random.Seed = int.Parse(Seed);
            MapMagic.Refresh();
        }
    }

}
