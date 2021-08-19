using MapMagic.Core;
using UnityEngine;

public class EnableInfiniteMap : MonoBehaviour
{
    [SerializeField] private MapMagicObject MapMagic;

    private void OnTriggerExit(Collider other)
    {
        MapMagic.tiles.generateInfinite = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        MapMagic.tiles.generateInfinite = false;
    }
}
