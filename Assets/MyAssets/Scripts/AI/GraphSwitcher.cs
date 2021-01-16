using System.Collections;
using System.Collections.Generic;
using System.Security;
using Pathfinding;
using UnityEngine;

public class GraphSwitcher : MonoBehaviour
{
    [SerializeField] private TransformSceneReference playerTransformSceneReference;
    [SerializeField] private Seeker seeker;
    [SerializeField] private float radiusOfHighRes = 150f;
    
    private NavGraph lowResGraph;
    private NavGraph highResGraph;
    private float sqrRadius;

    void Start()
    {
        lowResGraph = AstarPath.active.graphs[0];
        highResGraph = AstarPath.active.graphs[1];
        sqrRadius = radiusOfHighRes * radiusOfHighRes;
    }

    void Update()
    {
        if (Vector3.SqrMagnitude(transform.position - playerTransformSceneReference.Value.position) < sqrRadius)
        {
            seeker.graphMask = GraphMask.FromGraph(highResGraph);
        }
        else
        {
            seeker.graphMask = GraphMask.FromGraph(lowResGraph);
        }
        
    }
}
