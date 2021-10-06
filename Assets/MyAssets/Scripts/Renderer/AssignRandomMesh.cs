using System.Collections.Generic;
using UnityEngine;

public class AssignRandomMesh : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer Renderer;
    [SerializeField] private List<Mesh> RandomMeshes;

    public void Assign()
    {
        int randomIndex = Random.Range(0, RandomMeshes.Count);
        Renderer.sharedMesh = RandomMeshes[randomIndex];
    }
}
