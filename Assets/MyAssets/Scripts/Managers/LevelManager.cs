using MyAssets.Scripts.Utils;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private TransformSceneReference MapCenter;
    [SerializeField] private TransformSceneReference Player;
    [SerializeField] private TransformSceneReference DistanceTextTransform;

    [SerializeField] private bool UseRandomSeed;
    [SerializeField] private int UnityRandomSeed = 0;

    public static LevelManager Instance;

    public float DistanceFromCenter {get; private set;}

    private TMP_Text DistanceText;
    private string textPrefix;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        InputManager.Instance.onRestartScene += () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        };
        
        DistanceText = DistanceTextTransform.Value.GetComponent<TMP_Text>();
        textPrefix = DistanceText.text;

        if (UseRandomSeed)
        {
            UnityRandomSeed = Random.state.GetHashCode();
        }
        Random.InitState(UnityRandomSeed);
    }

    void Update()
    {
        DistanceFromCenter = Vector3.Distance(MapCenter.Value.position.xoz(), Player.Value.position.xoz());
        DistanceText.text = textPrefix + DistanceFromCenter.ToString("N0");
    }
}
