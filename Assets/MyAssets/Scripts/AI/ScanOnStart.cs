using System.Collections;
using System.Collections.Generic;
using MapMagic.Core;
using UnityEngine;

public class ScanOnStart : MonoBehaviour
{
    [SerializeField] private MapMagicObject MapMagic;
    [SerializeField] private AstarPath Pathfinder;

    private bool scanCompleted;

    // Update is called once per frame
    void Update()
    {
        if (scanCompleted) return;

        if (Mathf.Approximately(1f, MapMagic.GetProgress()))
        {
            Pathfinder.Scan();
            scanCompleted = true;
        }
    }
}
