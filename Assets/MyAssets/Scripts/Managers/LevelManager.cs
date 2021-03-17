using MyAssets.Scripts.Utils;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private TransformSceneReference MapCenter;
    [SerializeField] private TransformSceneReference Player;
    [SerializeField] private TransformSceneReference DistanceTextTransform;

    public static LevelManager Instance;

    public float DistanceFromCenter {get; private set;}

    private TMP_Text DistanceText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        DistanceText = DistanceTextTransform.Value.GetComponent<TMP_Text>();
    }

    void Update() {
        DistanceFromCenter = Vector3.Distance(MapCenter.Value.position.xoz(), Player.Value.position.xoz());
        DistanceText.text = "Dist: " + DistanceFromCenter.ToString("0");
    }
}
