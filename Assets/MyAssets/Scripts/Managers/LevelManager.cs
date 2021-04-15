using System;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MapMagic.Core;
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
    
    [TitleGroup("Boss Event")]
    [SerializeField] private IntReference _bossShrineRequirement;
    [SerializeField] private IntReference _playerShrinesVisited;
    [SerializeField] private Color _bossSkyboxColor;
    private Color _originalSkyboxColor;
    [SerializeField] private float _bossSkyboxFlowAmount;
    private float _originalSkyboxFlowAmount;
    [SerializeField] private TransformSceneReference ShrineTextTransform;
    [SerializeField] private Light _sceneLight;
    [SerializeField] private Color _bossLightColor;
    private Color _originalLightColor;
    [SerializeField] private float _portalDelay;
    [SerializeField] private GameObject _bossPortal;
    [SerializeField] private Transform _playerTransform;
    
    [TitleGroup("Level Effects")]
    [SerializeField] private Material _skybox;

    [TitleGroup("Seed")]
    [SerializeField] private bool UseRandomSeed;
    [SerializeField] private int UnityRandomSeed = 0;

    [TitleGroup("World State")] [SerializeField]
    private BoolReference _isPlayerDead;
    [SerializeField] private GameObject _deathText;
    [SerializeField] private GameEvent onRestartGame;
    
    [TitleGroup("MapMagicSeed")]
    [SerializeField] TransformSceneReference MapMagicSceneReference;
    [DisableIf("UseSetSeed")] [SerializeField] bool GenerateRandomSeed;
    [DisableIf("GenerateRandomSeed")] [SerializeField] bool UseSetSeed;
    [ShowIf("UseSetSeed")] [SerializeField] string Seed;

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
            RestartScene();
        };

        InputManager.Instance.onStart += () =>
        {
            if (_isPlayerDead.Value)
            {
                _deathText.SetActive(false);
                RestartScene();
            }
        };

        _isPlayerDead.Subscribe(TryEnableDeathText);
        
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

        MapMagicObject mapMagic = MapMagicSceneReference.Value.GetComponent<MapMagicObject>();
        if(GenerateRandomSeed || mapMagic.graph.random.Seed == 0) {
            mapMagic.graph.random.Seed = Random.Range(10000, 9999999);
            mapMagic.Refresh();
        }
        if(UseSetSeed) {
            mapMagic.graph.random.Seed = int.Parse(Seed);
            mapMagic.Refresh();
        }
    }

    private void OnDestroy()
    {
        _playerShrinesVisited.Unsubscribe(TrySpawnBoss);

        if (_originalSkyboxColor != default)
        {
            _skybox.SetColor("_Color2", _originalSkyboxColor);
            _skybox.SetFloat("_FlowAmount", _originalSkyboxFlowAmount);
            _sceneLight.color = _originalLightColor;
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

    // TODO this whole boss event implementation is super janky, should be replaced with something more modular when we build it out
    private void SpawnBoss()
    {
        _originalSkyboxColor = _skybox.GetColor("_Color2");
        _skybox.SetColor("_Color2", _bossSkyboxColor);
        
        _originalSkyboxFlowAmount = _skybox.GetFloat("_FlowAmount");
        _skybox.SetFloat("_FlowAmount", _bossSkyboxFlowAmount);

        _originalLightColor = _sceneLight.color;
        _sceneLight.color = _bossLightColor;

        var portal = Instantiate(_bossPortal, _playerTransform.position, Quaternion.identity);
    }

    private void RestartScene()
    {
        onRestartGame.Raise();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TryEnableDeathText(bool prev, bool isDead)
    {
        if (isDead) _deathText.SetActive(true);
    }
}
