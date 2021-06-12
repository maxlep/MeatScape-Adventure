
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

    [TitleGroup("Events")]
    [SerializeField] private GameEvent onBossPortalTriggered;
    [SerializeField] private GameEvent onShrineTriggered;

    [TitleGroup("Level Effects")]
    [SerializeField] private Material _skybox;

    [TitleGroup("Seed")]
    [SerializeField] private bool UseRandomSeed;
    [SerializeField] private int UnityRandomSeed = 0;

    [TitleGroup("World State")]
    [SerializeField]
    private BoolReference _isPlayerDead;
    [SerializeField] private GameObject _deathText;
    [SerializeField] private GameEvent onRestartGame;

    [TitleGroup("MapMagicSeed")]
    [SerializeField] TransformSceneReference MapMagicSceneReference;
    [DisableIf("UseSetSeed")] [SerializeField] bool GenerateRandomSeed;
    [DisableIf("GenerateRandomSeed")] [SerializeField] bool UseSetSeed;
    [ShowIf("UseSetSeed")] [SerializeField] string Seed;

    [TitleGroup("AreaGameObjects")]
    [SerializeField] private GameObject MeatScape;
    [SerializeField] private GameObject Boss;

    public static LevelManager Instance;

    public float DistanceFromCenter { get; private set; }

    private TMP_Text DistanceText;
    private string distanceTextPrefix;

    private TMP_Text shrineText;
    private string shrineTextPrefix;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(this);

        InputManager.Instance.onRestartScene += () => {
            RestartScene();
        };

        InputManager.Instance.onStart += () => {
            if(_isPlayerDead.Value)
            {
                _deathText.SetActive(false);
                RestartScene();
            }
        };

        _isPlayerDead.Subscribe(TryEnableDeathText);

        onBossPortalTriggered.Subscribe(EnterBossArena);
        onShrineTriggered.Subscribe(TeleportPlayerToMapCenter);

        DistanceText = DistanceTextTransform.Value.GetComponent<TMP_Text>();
        distanceTextPrefix = DistanceText.text;

        if(UseRandomSeed)
        {
            UnityRandomSeed = Random.state.GetHashCode();
        }
        Random.InitState(UnityRandomSeed);

        MapMagicObject mapMagic = MapMagicSceneReference.Value.GetComponent<MapMagicObject>();
        if(GenerateRandomSeed || mapMagic.graph.random.Seed == 0)
        {
            mapMagic.graph.random.Seed = Random.Range(10000, 9999999);
            mapMagic.Refresh();
        }
        if(UseSetSeed)
        {
            mapMagic.graph.random.Seed = int.Parse(Seed);
            mapMagic.Refresh();
        }
    }

    void Update()
    {
        DistanceFromCenter = Vector3.Distance(MapCenter.Value.position.xoz(), Player.Value.position.xoz());
        DistanceText.text = distanceTextPrefix + DistanceFromCenter.ToString("N0");
    }

    private void TeleportPlayerToMapCenter()
    {
        Player.Value.GetComponent<PlayerController>().SetPlayerPosition(MapCenter.Value.position);
    }

    private void EnterBossArena()
    {
        MeatScape.SetActive(false);
        Boss.SetActive(true);
        var newPos = Boss.transform.position;
        newPos.y += 100;
        Player.Value.GetComponent<PlayerController>().SetPlayerPosition(newPos);
    }

    private void RestartScene()
    {
        onRestartGame.Raise();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TryEnableDeathText(bool prev, bool isDead)
    {
        if(isDead) _deathText.SetActive(true);
    }
}
