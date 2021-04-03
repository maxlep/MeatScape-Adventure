using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    [TitleGroup("Distance")]
    [SerializeField] private TransformSceneReference MapCenter;
    [SerializeField] private TransformSceneReference Player;
    [SerializeField] private TransformSceneReference DistanceTextTransform;

    [TitleGroup("Boss Progress")]
    [SerializeField] private IntReference _bossShrineRequirement;
    [SerializeField] private IntReference _playerShrinesVisited;
    [SerializeField] private Color _bossSkyboxColor;
    private Color _originalSkyboxColor;
    [SerializeField] private float _bossSkyboxFlowAmount;
    private float _originalSkyboxFlowAmount;
    [SerializeField] private TransformSceneReference ShrineTextTransform;
    
    [TitleGroup("Level Effects")]
    [SerializeField] private Material _skybox;

    [TitleGroup("Seed")]
    [SerializeField] private bool UseRandomSeed;
    [SerializeField] private int UnityRandomSeed = 0;

    public static LevelManager Instance;

    public float DistanceFromCenter {get; private set;}

    private TMP_Text DistanceText;
    private string distanceTextPrefix;
    
    private TMP_Text shrineText;
    private string shrineTextPrefix;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        InputManager.Instance.onRestartScene += () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        };
        
        DistanceText = DistanceTextTransform.Value.GetComponent<TMP_Text>();
        distanceTextPrefix = DistanceText.text;

        shrineText = ShrineTextTransform.Value.GetComponent<TMP_Text>();
        shrineTextPrefix = shrineText.text;
        
        TrySpawnBoss();
        _playerShrinesVisited.Subscribe(TrySpawnBoss);

        if (UseRandomSeed)
        {
            UnityRandomSeed = Random.state.GetHashCode();
        }
        Random.InitState(UnityRandomSeed);
    }

    private void OnDestroy()
    {
        _playerShrinesVisited.Unsubscribe(TrySpawnBoss);

        if (_originalSkyboxColor != default)
        {
            _skybox.SetColor("_Color2", _originalSkyboxColor);
            _skybox.SetFloat("_FlowAmount", _originalSkyboxFlowAmount);
        }
    }

    void Update()
    {
        DistanceFromCenter = Vector3.Distance(MapCenter.Value.position.xoz(), Player.Value.position.xoz());
        DistanceText.text = distanceTextPrefix + DistanceFromCenter.ToString("N0");
    }

    private void TrySpawnBoss()
    {
        shrineText.text = shrineTextPrefix + _playerShrinesVisited.Value.ToString("N0");
        
        if (_playerShrinesVisited.Value >= _bossShrineRequirement.Value)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        
        _originalSkyboxColor = _skybox.GetColor("_Color2");
        _skybox.SetColor("_Color2", _bossSkyboxColor);
        
        _originalSkyboxFlowAmount = _skybox.GetFloat("_FlowAmount");
        _skybox.SetFloat("_FlowAmount", _bossSkyboxFlowAmount);
    }
}
