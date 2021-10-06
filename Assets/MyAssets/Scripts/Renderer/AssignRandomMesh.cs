using System.Collections.Generic;
using UnityEngine;

public class AssignRandomMesh : MonoBehaviour
{
    [SerializeField] private List<SkinnedMeshRenderer> Renderers;
    [SerializeField] private List<Mesh> RandomMeshes;

    public void Assign()
    {
        int randomIndex = Random.Range(0, RandomMeshes.Count);
        Renderers.ForEach(r => r.sharedMesh = RandomMeshes[randomIndex]);
    }
}
